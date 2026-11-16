// Asistencia de IA: escrito con Claude Code (Anthropic).
using System.Collections;
using UnityEngine;

/// <summary>
/// Respuesta de Nio al daño (no modifica NioMovement): parpadeo rojo durante la invulnerabilidad, pequeño empuje
/// alejándose del atacante y, si la vida llega a 0, control desactivado y reaparición en el punto de inicio.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Salud))]
public class NioRespuestaDanio : MonoBehaviour
{
    public NioMovement movimiento;
    [Tooltip("SpriteRenderer de Nio_Player/Visual.")]
    public SpriteRenderer sprite;
    [Header("Golpe")]
    [Tooltip("Velocidad inicial del empuje (m/s).")]
    public float empuje = 3f;
    public float duracionEmpuje = 0.15f;
    public Color colorDanio = new Color(1f, 0.4f, 0.4f, 1f);
    public float intervaloParpadeo = 0.08f;
    [Header("Muerte")]
    public bool reaparecerAlMorir = true;
    public float esperaReaparicion = 2f;

    public bool Parpadeando { get; private set; }
    public float UltimoEmpuje { get; private set; }

    Salud salud;
    CharacterController cc;
    Vector3 puntoInicio;
    Vector3 velEmpuje;
    float tEmpuje;
    Coroutine parpadeo;

    void Awake()
    {
        salud = GetComponent<Salud>();
        cc = GetComponent<CharacterController>();
        if (movimiento == null) movimiento = GetComponent<NioMovement>();
        puntoInicio = transform.position;
    }

    void OnEnable()
    {
        salud.AlRecibirDanio += AlRecibirDanio;
        salud.AlMorir += AlMorir;
    }

    void OnDisable()
    {
        salud.AlRecibirDanio -= AlRecibirDanio;
        salud.AlMorir -= AlMorir;
    }

    void AlRecibirDanio(Salud s, int cantidad, GameObject origen)
    {
        if (origen != null)
        {
            Vector3 d = transform.position - origen.transform.position;
            d.y = 0f;
            if (d.sqrMagnitude > 1e-4f) { velEmpuje = d.normalized * empuje; tEmpuje = duracionEmpuje; UltimoEmpuje = Time.time; }
        }
        if (parpadeo != null) StopCoroutine(parpadeo);
        if (sprite != null && !s.Muerto) parpadeo = StartCoroutine(Parpadear());
    }

    void Update()
    {
        if (tEmpuje > 0f && cc != null && cc.enabled)
        {
            float k = tEmpuje / Mathf.Max(0.01f, duracionEmpuje);
            cc.Move(velEmpuje * k * Time.deltaTime);
            tEmpuje -= Time.deltaTime;
        }
    }

    IEnumerator Parpadear()
    {
        Parpadeando = true;
        float fin = Time.time + Mathf.Max(0.2f, salud.invulnerabilidad);
        bool rojo = true;
        while (Time.time < fin && !salud.Muerto)
        {
            sprite.color = rojo ? colorDanio : new Color(1f, 1f, 1f, 0.45f);
            rojo = !rojo;
            yield return new WaitForSeconds(intervaloParpadeo);
        }
        if (!salud.Muerto) sprite.color = Color.white;
        Parpadeando = false;
        parpadeo = null;
    }

    void AlMorir(Salud s) => StartCoroutine(Muerte());

    IEnumerator Muerte()
    {
        bool teniaControl = movimiento != null && movimiento.controlActivo;
        if (movimiento != null) movimiento.controlActivo = false;
        if (sprite != null) sprite.color = new Color(0.55f, 0.55f, 0.6f, 0.6f);
        Debug.Log("[Nio] Nio se ha quedado sin vida" + (reaparecerAlMorir ? "; reaparece en " + esperaReaparicion + " s" : ""));
        if (!reaparecerAlMorir) yield break;
        yield return new WaitForSeconds(esperaReaparicion);
        if (movimiento != null) movimiento.Teletransportar(puntoInicio);
        salud.Restaurar();
        if (sprite != null) sprite.color = Color.white;
        if (movimiento != null) movimiento.controlActivo = teniaControl;
    }
}
