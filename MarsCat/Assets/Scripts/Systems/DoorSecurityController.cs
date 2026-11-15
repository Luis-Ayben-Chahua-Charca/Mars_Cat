using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Estado de seguridad del jugador respecto a una zona cerrada por puertas (por ejemplo, la zona técnica del ratón).
/// El jugador está A SALVO cuando está fuera de la zona Y todas las puertas de la zona están cerradas de verdad
/// (CabinaPuertaAutomatica.Cerrada). Se reevalúa con los eventos AlCerrarse / AlAbrirse de las puertas y cuando
/// el jugador entra o sale de la zona; no depende de la distancia al enemigo.
/// </summary>
[DisallowMultipleComponent]
public class DoorSecurityController : MonoBehaviour
{
    [Tooltip("Nio_Player.")]
    public Transform jugador;
    [Tooltip("Puertas que comunican la zona con el exterior.")]
    public CabinaPuertaAutomatica[] puertas = new CabinaPuertaAutomatica[0];
    [Tooltip("Centro de la zona (mundo).")]
    public Vector3 centroZona;
    [Tooltip("Tamaño de la zona (mundo). La altura no se tiene en cuenta.")]
    public Vector3 tamanoZona = new Vector3(2f, 3f, 20f);

    [Header("Eventos para el Inspector")]
    public UnityEvent alAsegurarse = new UnityEvent();
    public UnityEvent alPerderSeguridad = new UnityEvent();

    /// <summary>true = jugador a salvo (fuera de la zona con todas las puertas cerradas).</summary>
    public event Action<bool> AlCambiarSeguridad;

    public bool JugadorSeguro { get; private set; }
    public bool JugadorEnZona { get; private set; }
    public int Cambios { get; private set; }
    public float UltimoCambio { get; private set; } = -1f;

    public bool PuertasCerradas
    {
        get
        {
            foreach (CabinaPuertaAutomatica p in puertas)
                if (p != null && !p.Cerrada) return false;
            return true;
        }
    }

    void OnEnable()
    {
        foreach (CabinaPuertaAutomatica p in puertas)
            if (p != null) { p.AlCerrarse += AlCambiarPuerta; p.AlAbrirse += AlCambiarPuerta; }
    }

    void OnDisable()
    {
        foreach (CabinaPuertaAutomatica p in puertas)
            if (p != null) { p.AlCerrarse -= AlCambiarPuerta; p.AlAbrirse -= AlCambiarPuerta; }
    }

    void Start()
    {
        if (jugador == null)
        {
            var nio = FindJugador();
            if (nio != null) jugador = nio;
        }
        Reevaluar(true);
    }

    Transform FindJugador()
    {
        foreach (GameObject r in gameObject.scene.GetRootGameObjects())
        {
            var m = r.GetComponentInChildren<NioMovement>(true);
            if (m != null) return m.transform;
        }
        return null;
    }

    void Update()
    {
        if (jugador == null) return;
        if (Contiene(jugador.position) != JugadorEnZona) Reevaluar(false);
    }

    void AlCambiarPuerta(CabinaPuertaAutomatica puerta) => Reevaluar(false);

    void Reevaluar(bool inicial)
    {
        JugadorEnZona = jugador != null && Contiene(jugador.position);
        bool seguro = jugador != null && !JugadorEnZona && PuertasCerradas;
        if (!inicial && seguro == JugadorSeguro) return;
        JugadorSeguro = seguro;
        if (inicial) return;
        Cambios++;
        UltimoCambio = Time.time;
        AlCambiarSeguridad?.Invoke(seguro);
        if (seguro) alAsegurarse?.Invoke(); else alPerderSeguridad?.Invoke();
    }

    /// <summary>¿El punto está dentro de la zona (en planta)?</summary>
    public bool Contiene(Vector3 p)
    {
        Vector3 d = p - centroZona;
        return Mathf.Abs(d.x) <= tamanoZona.x * 0.5f && Mathf.Abs(d.z) <= tamanoZona.z * 0.5f;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Application.isPlaying ? (JugadorSeguro ? new Color(0.2f, 1f, 0.3f, 0.9f) : new Color(1f, 0.3f, 0.2f, 0.9f)) : new Color(1f, 0.8f, 0.2f, 0.9f);
        Gizmos.DrawWireCube(centroZona, tamanoZona);
        foreach (CabinaPuertaAutomatica p in puertas)
            if (p != null) Gizmos.DrawLine(centroZona, p.transform.position);
    }
}
