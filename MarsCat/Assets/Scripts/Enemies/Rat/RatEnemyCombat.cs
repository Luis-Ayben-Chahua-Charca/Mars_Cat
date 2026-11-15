using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Ataque del ratón: mordisco con animación, impacto a mitad de la animación, tiempo de espera entre ataques y daño
/// aplicado con el componente Salud del objetivo (que respeta su invulnerabilidad). Nunca hace daño en cada frame.
/// </summary>
[DisallowMultipleComponent]
public class RatEnemyCombat : MonoBehaviour
{
    [Min(0)] public int danio = 1;
    [Tooltip("Distancia horizontal (m) a la que el ratón empieza un ataque.")]
    [Min(0.1f)] public float distanciaAtaque = 0.75f;
    [Tooltip("Distancia máxima en el momento del impacto (el objetivo puede apartarse durante el ataque).")]
    [Min(0.1f)] public float alcanceImpacto = 0.95f;
    [Tooltip("Segundos entre el inicio de dos ataques.")]
    [Min(0.1f)] public float cooldown = 1.2f;
    [Tooltip("Segundos desde el inicio del ataque hasta el impacto (fotograma de la embestida).")]
    [Min(0f)] public float retardoImpacto = 0.17f;
    [Tooltip("Duración total del ataque (el clip Raton_Attack dura 0,5 s).")]
    [Min(0.05f)] public float duracionAtaque = 0.5f;
    public RatEnemyAnimation animacion;

    public bool Atacando { get; private set; }
    public bool PuedeAtacar => enabled && !Atacando && Time.time >= proximoAtaque;
    public int AtaquesIniciados { get; private set; }
    public int ImpactosAplicados { get; private set; }
    public int ImpactosSinEfecto { get; private set; }
    public float UltimoAtaque { get; private set; } = -1f;

    /// <summary>(objetivo, daño aplicado). false = fallo, fuera de alcance, condiciones de seguridad o invulnerabilidad.</summary>
    public event Action<Salud, bool> AlImpactar;

    float proximoAtaque;
    Coroutine rutina;

    void Awake()
    {
        if (animacion == null) animacion = GetComponentInChildren<RatEnemyAnimation>();
    }

    /// <summary>
    /// Empieza un ataque si el tiempo de espera lo permite. "sigueValido" se comprueba en el momento del impacto
    /// (línea de visión, jugador fuera de la zona segura…).
    /// </summary>
    public bool IntentarAtacar(Transform objetivo, Salud saludObjetivo, Func<bool> sigueValido)
    {
        if (!PuedeAtacar || objetivo == null) return false;
        rutina = StartCoroutine(Atacar(objetivo, saludObjetivo, sigueValido));
        return true;
    }

    IEnumerator Atacar(Transform objetivo, Salud saludObjetivo, Func<bool> sigueValido)
    {
        Atacando = true;
        AtaquesIniciados++;
        UltimoAtaque = Time.time;
        proximoAtaque = Time.time + cooldown;
        if (animacion != null) animacion.Atacar(objetivo.position);
        yield return new WaitForSeconds(retardoImpacto);

        Vector3 d = objetivo.position - transform.position;
        d.y = 0f;
        bool valido = (sigueValido == null || sigueValido()) && d.magnitude <= alcanceImpacto && saludObjetivo != null;
        bool aplicado = valido && saludObjetivo.RecibirDanio(danio, gameObject);
        if (aplicado) ImpactosAplicados++; else ImpactosSinEfecto++;
        AlImpactar?.Invoke(saludObjetivo, aplicado);

        yield return new WaitForSeconds(Mathf.Max(0f, duracionAtaque - retardoImpacto));
        Atacando = false;
        rutina = null;
    }

    /// <summary>Interrumpe el ataque en curso (daño recibido, jugador a salvo, muerte).</summary>
    public void Cancelar()
    {
        if (rutina != null) StopCoroutine(rutina);
        rutina = null;
        Atacando = false;
    }

    void OnDisable() => Cancelar();
}
