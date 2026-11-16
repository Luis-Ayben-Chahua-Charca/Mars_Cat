// Asistencia de IA: escrito con Claude Code (Anthropic).
using UnityEngine;

/// <summary>
/// Controlador general del ratón: conecta Salud con la IA, el combate y la animación.
///  - Daño recibido: interrumpe el ataque, aturde un momento y reproduce Damage.
///  - Muerte: detiene la IA y el combate, desactiva el collider, reproduce Death y desvanece el sprite.
/// Botones de prueba en el menú contextual del componente (⋮ en el Inspector).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Salud))]
public class RatEnemyController : MonoBehaviour
{
    public RatEnemyAI ia;
    public RatEnemyCombat combate;
    public RatEnemyAnimation animacion;
    [Tooltip("Collider del cuerpo (bloquea a Nio). Se desactiva al morir.")]
    public Collider cuerpo;
    [Tooltip("Segundos aturdido tras recibir daño.")]
    public float aturdimiento = 0.4f;
    [Tooltip("Tras morir: espera antes de desvanecerse y duración del desvanecimiento.")]
    public float esperaDesvanecer = 0.8f, duracionDesvanecer = 1.2f;

    public Salud Salud { get; private set; }

    void Awake()
    {
        Salud = GetComponent<Salud>();
        if (ia == null) ia = GetComponent<RatEnemyAI>();
        if (combate == null) combate = GetComponent<RatEnemyCombat>();
        if (animacion == null) animacion = GetComponentInChildren<RatEnemyAnimation>();
        if (cuerpo == null) cuerpo = GetComponent<Collider>();
    }

    void OnEnable()
    {
        Salud.AlRecibirDanio += AlRecibirDanio;
        Salud.AlMorir += AlMorir;
    }

    void OnDisable()
    {
        Salud.AlRecibirDanio -= AlRecibirDanio;
        Salud.AlMorir -= AlMorir;
    }

    void AlRecibirDanio(Salud s, int cantidad, GameObject origen)
    {
        if (s.Muerto) return;
        if (combate != null) combate.Cancelar();
        if (ia != null) ia.Aturdir(aturdimiento);
        if (animacion != null) animacion.Herido();
    }

    void AlMorir(Salud s)
    {
        if (combate != null) { combate.Cancelar(); combate.enabled = false; }
        if (ia != null) ia.Morir();
        if (cuerpo != null) cuerpo.enabled = false;
        if (animacion != null) animacion.Morir(esperaDesvanecer, duracionDesvanecer);
    }

    [ContextMenu("Prueba: recibir 1 de daño")]
    public void PruebaDanio() => Salud.RecibirDanio(1, null);

    [ContextMenu("Prueba: morir")]
    public void PruebaMorir() => Salud.Matar(null);
}
