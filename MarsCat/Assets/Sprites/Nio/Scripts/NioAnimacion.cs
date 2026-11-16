// Asistencia de IA: escrito con Claude Code (Anthropic).
using UnityEngine;

/// <summary>
/// Envía los parámetros Speed, IsWorking e IsBack al Animator de Nio y orienta
/// el sprite hacia la cámara (rotación solo en Y) para que se vea bien en la escena 3D.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class NioAnimacion : MonoBehaviour
{
    [Header("Estado (desde otros scripts o el Inspector)")]
    public bool trabajando;
    public bool deEspaldas;
    [Tooltip("Lo activa NioMovement cuando Nio camina alejándose de la cámara: mientras se mueve (Speed > 0.1) se muestra Back.")]
    public bool alejandose;
    [Tooltip("Velocidad fija en m/s. Con un valor negativo se calcula a partir del movimiento del objeto.")]
    public float velocidadManual = -1f;
    [Tooltip("Suavizado de la velocidad calculada (evita parpadeos Walk/Idle en los cambios de sentido).")]
    public float suavizado = 20f;

    [Header("Orientación en la escena 3D")]
    public bool mirarACamara = true;
    [Tooltip("Vacío = Camera.main")]
    public Camera camara;
    [Tooltip("Los sprites de Nio_Walk miran hacia la derecha.")]
    public bool spritesMiranDerecha = true;

    public float Velocidad { get; private set; }

    static readonly int pSpeed = Animator.StringToHash("Speed");
    static readonly int pIsWorking = Animator.StringToHash("IsWorking");
    static readonly int pIsBack = Animator.StringToHash("IsBack");

    Animator animator;
    SpriteRenderer sprite;
    Vector3 posicionAnterior;

    void Awake()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        posicionAnterior = transform.position;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        Vector3 delta = transform.position - posicionAnterior;
        delta.y = 0f;
        posicionAnterior = transform.position;

        if (velocidadManual >= 0f) Velocidad = velocidadManual;
        else if (dt > 0f) Velocidad = Mathf.Lerp(Velocidad, delta.magnitude / dt, 1f - Mathf.Exp(-suavizado * dt));

        animator.SetFloat(pSpeed, Velocidad);
        animator.SetBool(pIsWorking, trabajando);
        // IsBack: de espaldas en reposo (manual) o caminando alejándose de la cámara. Se evalúa con la misma
        // velocidad que Speed para que Back y Walk/Idle cambien en el mismo frame.
        animator.SetBool(pIsBack, deEspaldas || (alejandose && Velocidad > 0.1f));

        Camera cam = camara != null ? camara : Camera.main;
        if (cam == null) return;

        if (mirarACamara)
        {
            Vector3 f = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up);
            if (f.sqrMagnitude > 1e-6f) transform.rotation = Quaternion.LookRotation(f.normalized, Vector3.up);
        }

        float lateral = Vector3.Dot(delta, cam.transform.right);
        if (Mathf.Abs(lateral) > 1e-4f) sprite.flipX = spritesMiranDerecha ? lateral < 0f : lateral > 0f;
    }
}
