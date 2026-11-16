// Asistencia de IA: escrito con Claude Code (Anthropic).
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Animación del ratón (en Raton_Enemigo/Visual, junto al SpriteRenderer y el Animator).
/// Parámetros del Animator: Speed (float), Attack (trigger), Hurt (trigger), IsDead (bool).
/// Orienta el sprite hacia la cámara (solo eje Y), lo invierte en horizontal según la dirección (los sprites miran
/// a la derecha) y apoya los pies y la sombra sobre el suelo.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class RatEnemyAnimation : MonoBehaviour
{
    public NavMeshAgent agente;
    public Transform sombra;
    public bool mirarACamara = true;
    [Tooltip("Vacío = Camera.main")]
    public Camera camara;
    public bool spritesMiranDerecha = true;
    public bool ajustarPiesAlSuelo = true;
    public float elevacion = 0.003f;
    public float elevacionSombra = 0.011f;

    static readonly int pSpeed = Animator.StringToHash("Speed");
    static readonly int pAttack = Animator.StringToHash("Attack");
    static readonly int pHurt = Animator.StringToHash("Hurt");
    static readonly int pIsDead = Animator.StringToHash("IsDead");

    public float Velocidad { get; private set; }
    public Animator Animator => animator;
    public SpriteRenderer Sprite => sprite;

    Animator animator;
    SpriteRenderer sprite;
    Transform raiz;
    readonly RaycastHit[] impactos = new RaycastHit[8];

    void Awake()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        raiz = transform.parent != null ? transform.parent : transform;
        if (agente == null) agente = GetComponentInParent<NavMeshAgent>();
    }

    Camera Cam => camara != null ? camara : Camera.main;

    public void Atacar(Vector3 objetivo)
    {
        Mirar(objetivo - raiz.position);
        animator.SetTrigger(pAttack);
    }

    public void Herido() => animator.SetTrigger(pHurt);

    public void Morir(float retardo, float duracion)
    {
        animator.ResetTrigger(pAttack);
        animator.ResetTrigger(pHurt);
        animator.SetBool(pIsDead, true);
        StartCoroutine(Desvanecer(retardo, duracion));
    }

    IEnumerator Desvanecer(float retardo, float duracion)
    {
        yield return new WaitForSeconds(retardo);
        float t = 0f;
        Color c = sprite.color;
        SpriteRenderer srSombra = sombra != null ? sombra.GetComponent<SpriteRenderer>() : null;
        Color cs = srSombra != null ? srSombra.color : Color.clear;
        while (t < duracion)
        {
            t += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(t / duracion);
            sprite.color = new Color(c.r, c.g, c.b, c.a * a);
            if (srSombra != null) srSombra.color = new Color(cs.r, cs.g, cs.b, cs.a * a);
            yield return null;
        }
    }

    void Mirar(Vector3 direccion)
    {
        Camera cam = Cam;
        if (cam == null) return;
        float lateral = Vector3.Dot(direccion, cam.transform.right);
        if (Mathf.Abs(lateral) > 0.01f) sprite.flipX = spritesMiranDerecha ? lateral < 0f : lateral > 0f;
    }

    void Update()
    {
        Vector3 v = agente != null && agente.enabled ? agente.velocity : Vector3.zero;
        v.y = 0f;
        Velocidad = v.magnitude;
        animator.SetFloat(pSpeed, Velocidad);

        Camera cam = Cam;
        if (cam != null && mirarACamara)
        {
            Vector3 f = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up);
            if (f.sqrMagnitude > 1e-6f) transform.rotation = Quaternion.LookRotation(f.normalized, Vector3.up);
        }
        if (Velocidad > 0.15f) Mirar(v);
        AjustarAlSuelo();
    }

    void AjustarAlSuelo()
    {
        if (!ajustarPiesAlSuelo) return;
        int n = Physics.RaycastNonAlloc(raiz.position + Vector3.up * 0.4f, Vector3.down, impactos, 1f, ~0, QueryTriggerInteraction.Ignore);
        float mejor = float.MaxValue, y = 0f;
        bool hay = false;
        for (int i = 0; i < n; i++)
        {
            if (impactos[i].collider.transform.IsChildOf(raiz)) continue;
            if (impactos[i].distance < mejor) { mejor = impactos[i].distance; y = impactos[i].point.y; hay = true; }
        }
        if (!hay) return;
        float dy = y - raiz.position.y;
        transform.localPosition = new Vector3(0f, dy + elevacion, 0f);
        if (sombra != null) sombra.localPosition = new Vector3(0f, dy + elevacionSombra, 0f);
    }
}
