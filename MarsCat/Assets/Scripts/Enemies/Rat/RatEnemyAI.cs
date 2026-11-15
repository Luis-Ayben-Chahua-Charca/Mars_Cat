using System;
using UnityEngine;
using UnityEngine.AI;

public enum EstadoRaton { Patrulla, Persecucion, Busqueda, Regreso, Aturdido, Muerto }

/// <summary>
/// Detección y movimiento del ratón con NavMeshAgent (malla de navegación de su zona, sin atravesar muros).
///  - Patrulla entre puntos o espera en su posición inicial.
///  - Detecta a Nio dentro de un radio y con línea de visión, y lo persigue recalculando la ruta.
///  - Si lo pierde de vista, va a la última posición conocida (búsqueda) y después vuelve a patrullar.
///  - Cuando DoorSecurityController indica que Nio está a salvo (fuera de la zona con las puertas cerradas),
///    abandona la persecución en ese mismo frame, cancela el ataque y regresa a su posición inicial.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public class RatEnemyAI : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Nio_Player.")]
    public Transform jugador;
    public DoorSecurityController seguridad;
    public RatEnemyCombat combate;
    public Transform[] puntosPatrulla = new Transform[0];

    [Header("Detección")]
    [Min(0.5f)] public float radioDeteccion = 6f;
    [Tooltip("Durante la persecución, distancia a partir de la cual lo pierde.")]
    [Min(0.5f)] public float radioPerdida = 9f;
    [Tooltip("Segundos sin línea de visión para dejar de perseguir y pasar a búsqueda.")]
    [Min(0f)] public float tiempoParaPerderlo = 1.5f;
    public float alturaOjos = 0.2f;
    public float alturaObjetivo = 0.3f;

    [Header("Movimiento")]
    [Min(0.1f)] public float velocidadPersecucion = 2.6f;
    [Min(0.1f)] public float velocidadPatrulla = 1.2f;
    [Min(0.1f)] public float aceleracion = 20f;
    [Min(0f)] public float esperaEnPunto = 1.5f;
    [Min(0f)] public float duracionBusqueda = 2.5f;
    [Min(0.05f)] public float intervaloRuta = 0.15f;

    public EstadoRaton Estado { get; private set; }
    public Vector3 PosicionInicial { get; private set; }
    public bool VeAlJugador { get; private set; }
    public float UltimoCambioEstado { get; private set; }
    public int RutasCalculadas { get; private set; }
    public event Action<EstadoRaton> AlCambiarEstado;

    NavMeshAgent agente;
    Salud saludJugador;
    Collider[] propios = new Collider[0];
    float tUltimaVista, tEspera, finAturdido, tRuta;
    int indicePatrulla;
    Vector3 ultimaPosicionVista;
    bool esperando;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.updateRotation = false;
        if (combate == null) combate = GetComponent<RatEnemyCombat>();
        propios = GetComponentsInChildren<Collider>(true);
        PosicionInicial = transform.position;
        ultimaPosicionVista = transform.position;
    }

    void OnEnable() { if (seguridad != null) seguridad.AlCambiarSeguridad += AlCambiarSeguridad; }
    void OnDisable() { if (seguridad != null) seguridad.AlCambiarSeguridad -= AlCambiarSeguridad; }

    void Start()
    {
        if (jugador == null)
            foreach (GameObject r in gameObject.scene.GetRootGameObjects())
            {
                var m = r.GetComponentInChildren<NioMovement>(true);
                if (m != null) { jugador = m.transform; break; }
            }
        if (jugador != null) saludJugador = jugador.GetComponent<Salud>();
        if (!agente.isOnNavMesh && NavMesh.SamplePosition(transform.position, out NavMeshHit h, 2f, NavMesh.AllAreas)) agente.Warp(h.position);
        agente.acceleration = aceleracion;
        Cambiar(puntosPatrulla.Length > 0 ? EstadoRaton.Patrulla : EstadoRaton.Regreso);
    }

    bool JugadorSeguro => seguridad != null && seguridad.JugadorSeguro;
    bool JugadorVivo => saludJugador == null || !saludJugador.Muerto;
    float DistanciaJugador
    {
        get
        {
            if (jugador == null) return float.MaxValue;
            Vector3 d = jugador.position - transform.position;
            d.y = 0f;
            return d.magnitude;
        }
    }

    void Cambiar(EstadoRaton nuevo)
    {
        if (Estado == nuevo && UltimoCambioEstado > 0f) return;
        Estado = nuevo;
        UltimoCambioEstado = Time.time;
        esperando = false;
        tRuta = 0f;
        if (!agente.enabled || !agente.isOnNavMesh) { AlCambiarEstado?.Invoke(nuevo); return; }
        switch (nuevo)
        {
            case EstadoRaton.Persecucion:
                agente.speed = velocidadPersecucion;
                agente.stoppingDistance = combate != null ? combate.distanciaAtaque * 0.8f : 0.6f;
                agente.isStopped = false;
                break;
            case EstadoRaton.Busqueda:
                agente.speed = velocidadPersecucion;
                agente.stoppingDistance = 0.2f;
                agente.isStopped = false;
                IrA(ultimaPosicionVista);
                break;
            case EstadoRaton.Regreso:
                agente.speed = velocidadPatrulla;
                agente.stoppingDistance = 0.1f;
                agente.isStopped = false;
                IrA(PosicionInicial);
                break;
            case EstadoRaton.Patrulla:
                agente.speed = velocidadPatrulla;
                agente.stoppingDistance = 0.1f;
                agente.isStopped = false;
                if (puntosPatrulla.Length > 0) IrA(puntosPatrulla[indicePatrulla % puntosPatrulla.Length].position);
                break;
            case EstadoRaton.Aturdido:
            case EstadoRaton.Muerto:
                agente.ResetPath();
                agente.velocity = Vector3.zero;
                agente.isStopped = true;
                break;
        }
        AlCambiarEstado?.Invoke(nuevo);
    }

    void IrA(Vector3 destino)
    {
        if (!agente.isOnNavMesh) return;
        if (NavMesh.SamplePosition(destino, out NavMeshHit h, 2.5f, NavMesh.AllAreas))
        {
            agente.SetDestination(h.position);
            RutasCalculadas++;
        }
    }

    bool Llego => !agente.pathPending && agente.remainingDistance <= Mathf.Max(agente.stoppingDistance, 0.15f);

    void AlCambiarSeguridad(bool seguro)
    {
        if (!seguro || Estado == EstadoRaton.Muerto) return;
        if (combate != null) combate.Cancelar();
        if (Estado == EstadoRaton.Persecucion || Estado == EstadoRaton.Busqueda || Estado == EstadoRaton.Aturdido)
            Cambiar(EstadoRaton.Regreso);
    }

    /// <summary>¿Hay línea de visión directa con Nio (sin muros ni puertas cerradas en medio)?</summary>
    public bool LineaDeVision()
    {
        if (jugador == null) return false;
        Vector3 o = transform.position + Vector3.up * alturaOjos;
        Vector3 p = jugador.position + Vector3.up * alturaObjetivo;
        Vector3 d = p - o;
        RaycastHit[] hits = Physics.RaycastAll(o, d.normalized, d.magnitude, ~0, QueryTriggerInteraction.Ignore);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit h in hits)
        {
            if (Array.IndexOf(propios, h.collider) >= 0) continue;
            return h.collider.transform.IsChildOf(jugador);
        }
        return true;
    }

    bool Detecta(float radio) => jugador != null && DistanciaJugador <= radio && LineaDeVision();

    /// <summary>Lo usa RatEnemyCombat en el momento del impacto.</summary>
    public bool AtaqueSigueValido() => !JugadorSeguro && JugadorVivo && Estado == EstadoRaton.Persecucion && LineaDeVision();

    public void Aturdir(float segundos)
    {
        if (Estado == EstadoRaton.Muerto) return;
        if (combate != null) combate.Cancelar();
        finAturdido = Time.time + segundos;
        Cambiar(EstadoRaton.Aturdido);
    }

    public void Morir()
    {
        if (combate != null) combate.Cancelar();
        Cambiar(EstadoRaton.Muerto);
        agente.enabled = false;
        enabled = false;
    }

    void Update()
    {
        if (!agente.enabled || !agente.isOnNavMesh) return;
        bool puedePerseguir = !JugadorSeguro && JugadorVivo;

        switch (Estado)
        {
            case EstadoRaton.Aturdido:
                VeAlJugador = false;
                if (Time.time >= finAturdido)
                    Cambiar(puedePerseguir && Detecta(radioPerdida) ? EstadoRaton.Persecucion : EstadoRaton.Busqueda);
                return;

            case EstadoRaton.Persecucion:
                if (!puedePerseguir) { VeAlJugador = false; if (combate != null) combate.Cancelar(); Cambiar(EstadoRaton.Regreso); return; }
                VeAlJugador = Detecta(radioPerdida);
                if (VeAlJugador) { tUltimaVista = Time.time; ultimaPosicionVista = jugador.position; }
                else if (Time.time - tUltimaVista > tiempoParaPerderlo) { Cambiar(EstadoRaton.Busqueda); return; }
                Perseguir();
                return;

            default:
                VeAlJugador = puedePerseguir && Detecta(radioDeteccion);
                if (VeAlJugador)
                {
                    tUltimaVista = Time.time;
                    ultimaPosicionVista = jugador.position;
                    Cambiar(EstadoRaton.Persecucion);
                    Perseguir();
                    return;
                }
                break;
        }

        if (Estado == EstadoRaton.Busqueda && Llego)
        {
            if (!esperando) { esperando = true; tEspera = Time.time; }
            else if (Time.time - tEspera >= duracionBusqueda) Cambiar(puntosPatrulla.Length > 0 ? EstadoRaton.Patrulla : EstadoRaton.Regreso);
        }
        else if (Estado == EstadoRaton.Regreso && Llego)
        {
            agente.ResetPath();
            if (puntosPatrulla.Length > 0)
            {
                if (!esperando) { esperando = true; tEspera = Time.time; }
                else if (Time.time - tEspera >= esperaEnPunto * 2f) Cambiar(EstadoRaton.Patrulla);
            }
        }
        else if (Estado == EstadoRaton.Patrulla && puntosPatrulla.Length > 0 && Llego)
        {
            if (!esperando) { esperando = true; tEspera = Time.time; }
            else if (Time.time - tEspera >= esperaEnPunto)
            {
                esperando = false;
                indicePatrulla = (indicePatrulla + 1) % puntosPatrulla.Length;
                IrA(puntosPatrulla[indicePatrulla].position);
            }
        }
    }

    void Perseguir()
    {
        float dist = DistanciaJugador;
        bool atacando = combate != null && combate.Atacando;
        agente.isStopped = atacando;
        if (!atacando && Time.time >= tRuta)
        {
            tRuta = Time.time + intervaloRuta;
            IrA(jugador.position);
        }
        if (combate != null && !atacando && dist <= combate.distanciaAtaque && VeAlJugador)
            combate.IntentarAtacar(jugador, saludJugador, AtaqueSigueValido);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
        Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.6f);
        if (combate != null) Gizmos.DrawWireSphere(transform.position, combate.distanciaAtaque);
    }
}
