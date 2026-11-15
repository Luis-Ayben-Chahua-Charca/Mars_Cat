using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sistema de vida compartido (Nio_Player y enemigos). Vida en puntos enteros, invulnerabilidad temporal tras
/// cada golpe y eventos de daño / muerte / restauración. No se mueve ni anima nada: cada personaje responde
/// a los eventos con su propio componente (NioRespuestaDanio, RatEnemyController…).
/// </summary>
[DisallowMultipleComponent]
public class Salud : MonoBehaviour
{
    [Min(1)] public int vidaMaxima = 5;
    [Tooltip("Segundos sin poder recibir daño después de un golpe.")]
    [Min(0f)] public float invulnerabilidad = 1f;
    [SerializeField, Tooltip("Solo lectura: vida actual (se actualiza en Play).")]
    int vidaActual;

    [Header("Eventos para el Inspector")]
    public UnityEvent alRecibirDanio = new UnityEvent();
    public UnityEvent alMorir = new UnityEvent();

    /// <summary>(salud, cantidad, origen del daño).</summary>
    public event Action<Salud, int, GameObject> AlRecibirDanio;
    public event Action<Salud> AlMorir;
    public event Action<Salud> AlRestaurar;

    public int Vida { get => vidaActual; private set => vidaActual = value; }
    public bool Muerto => Vida <= 0;
    public bool Invulnerable => Time.time < finInvulnerable;
    public int GolpesRecibidos { get; private set; }
    public int GolpesIgnorados { get; private set; }

    float finInvulnerable = float.NegativeInfinity;

    void Awake() { Vida = vidaMaxima; }

    /// <summary>Aplica daño si no está muerto ni en invulnerabilidad. Devuelve true si el daño se aplicó.</summary>
    public bool RecibirDanio(int cantidad, GameObject origen = null)
    {
        if (cantidad <= 0 || Muerto) return false;
        if (Invulnerable) { GolpesIgnorados++; return false; }
        Vida = Mathf.Max(0, Vida - cantidad);
        GolpesRecibidos++;
        finInvulnerable = Time.time + invulnerabilidad;
        AlRecibirDanio?.Invoke(this, cantidad, origen);
        alRecibirDanio?.Invoke();
        if (Vida == 0) Morir();
        return true;
    }

    /// <summary>Lleva la vida a 0 ignorando la invulnerabilidad.</summary>
    public void Matar(GameObject origen = null)
    {
        if (Muerto) return;
        Vida = 0;
        Morir();
    }

    public void Restaurar()
    {
        Vida = vidaMaxima;
        finInvulnerable = float.NegativeInfinity;
        AlRestaurar?.Invoke(this);
    }

    void Morir()
    {
        AlMorir?.Invoke(this);
        alMorir?.Invoke();
    }

    [ContextMenu("Prueba: recibir 1 de daño")]
    void PruebaDanio() => RecibirDanio(1, null);

    [ContextMenu("Prueba: morir")]
    void PruebaMorir() => Matar(null);

    [ContextMenu("Prueba: restaurar vida")]
    void PruebaRestaurar() => Restaurar();
}
