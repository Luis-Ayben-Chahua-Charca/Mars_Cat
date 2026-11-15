using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Control de Nio_Player con CharacterController.
///   C        activa / desactiva el control (empieza desactivado)
///   W A S D  mover en el plano XZ, relativo a MovementReference (ejes del escenario, no de la pantalla)
///   Shift    correr
///   E        trabajar (Work) estando quieto; moverse lo cancela
/// La gravedad actúa en Y. Las animaciones las aplica NioAnimacion (en Visual) a partir del movimiento real.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class NioMovement : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Transform alineado con los ejes del escenario: W = su Forward, D = su Right.")]
    public Transform movementReference;
    [Tooltip("Hijo con SpriteRenderer + Animator.")]
    public Transform visual;
    public NioAnimacion animacion;
    [Tooltip("Punto que sigue la cámara.")]
    public Transform cameraTarget;
    [Tooltip("Sombra circular bajo los pies (opcional).")]
    public Transform sombra;

    [Header("Control")]
    public bool controlActivo = false;
    [Min(0f)] public float velocidad = 2.2f;
    [Min(0f)] public float velocidadCorrer = 3.6f;
    [Tooltip("m/s²: lo rápido que arranca y frena.")]
    [Min(0.1f)] public float aceleracion = 20f;
    public float gravedad = -20f;

    [Header("Animación")]
    [Tooltip("Al caminar alejándose de la cámara se muestra Back (Nio de espaldas). Si se desactiva, se usa Walk.")]
    public bool usarBackAlAlejarse = true;
    [Tooltip("Coseno del ángulo con la dirección de la cámara a partir del cual se considera que Nio se aleja (0.5 = 60°).")]
    [Range(0f, 1f)] public float umbralAlejarse = 0.5f;

    [Header("Integración visual")]
    [Tooltip("Coloca los pies del sprite y la sombra exactamente sobre el suelo.")]
    public bool ajustarPiesAlSuelo = true;
    public float elevacionVisual = 0.003f;
    public float elevacionSombra = 0.012f;
    public LayerMask capasSuelo = ~0;

    [Header("Pruebas automáticas")]
    public bool usarEntradaSimulada;
    public Vector2 entradaSimulada;
    public bool correrSimulado;

    public Vector3 VelocidadPlano => velPlano;
    public bool EnSuelo => cc != null && cc.isGrounded;
    public bool HaySuelo { get; private set; }
    public float AlturaSuelo { get; private set; }
    public bool Alejandose { get; private set; }

    CharacterController cc;
    Vector3 velPlano;
    float velY;
    bool pulsarAlternar, pulsarTrabajar;
    readonly RaycastHit[] impactos = new RaycastHit[8];

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (animacion == null && visual != null) animacion = visual.GetComponent<NioAnimacion>();
    }

    /// <summary>Equivale a pulsar C (para pruebas).</summary>
    public void PulsarAlternarControl() => pulsarAlternar = true;
    /// <summary>Equivale a pulsar E (para pruebas).</summary>
    public void PulsarTrabajar() => pulsarTrabajar = true;

    public void Teletransportar(Vector3 posicion)
    {
        cc.enabled = false;
        transform.position = posicion;
        cc.enabled = true;
        velPlano = Vector3.zero;
        velY = 0f;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        bool alternar = pulsarAlternar, trabajar = pulsarTrabajar;
        pulsarAlternar = pulsarTrabajar = false;
        Vector2 entrada = Vector2.zero;
        bool correr = false;

#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k != null)
        {
            if (k.cKey.wasPressedThisFrame) alternar = true;
            if (k.eKey.wasPressedThisFrame) trabajar = true;
            if (!usarEntradaSimulada)
            {
                if (k.wKey.isPressed) entrada.y += 1f;
                if (k.sKey.isPressed) entrada.y -= 1f;
                if (k.dKey.isPressed) entrada.x += 1f;
                if (k.aKey.isPressed) entrada.x -= 1f;
                correr = k.leftShiftKey.isPressed || k.rightShiftKey.isPressed;
            }
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.C)) alternar = true;
        if (Input.GetKeyDown(KeyCode.E)) trabajar = true;
        if (!usarEntradaSimulada)
        {
            if (Input.GetKey(KeyCode.W)) entrada.y += 1f;
            if (Input.GetKey(KeyCode.S)) entrada.y -= 1f;
            if (Input.GetKey(KeyCode.D)) entrada.x += 1f;
            if (Input.GetKey(KeyCode.A)) entrada.x -= 1f;
            correr = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }
#endif
        if (usarEntradaSimulada)
        {
            entrada = entradaSimulada;
            correr = correrSimulado;
        }

        if (alternar)
        {
            controlActivo = !controlActivo;
            Debug.Log("[Nio] Control " + (controlActivo ? "activado" : "desactivado") + " (tecla C)");
        }
        if (!controlActivo) entrada = Vector2.zero;
        if (entrada.sqrMagnitude > 1f) entrada.Normalize(); // diagonal a la misma velocidad

        // Direcciones del escenario proyectadas sobre el plano horizontal.
        Transform r = movementReference != null ? movementReference : null;
        Vector3 adelante = r != null ? Vector3.ProjectOnPlane(r.forward, Vector3.up).normalized : Vector3.forward;
        Vector3 derecha = r != null ? Vector3.ProjectOnPlane(r.right, Vector3.up).normalized : Vector3.right;
        Vector3 deseada = (derecha * entrada.x + adelante * entrada.y) * (correr ? velocidadCorrer : velocidad);

        velPlano = controlActivo ? Vector3.MoveTowards(velPlano, deseada, aceleracion * dt) : Vector3.zero;
        if (cc.isGrounded && velY < 0f) velY = -2f;
        else velY += gravedad * dt;
        cc.Move((velPlano + Vector3.up * velY) * dt);

        if (animacion != null && controlActivo)
        {
            bool hayEntrada = entrada.sqrMagnitude > 0.01f;
            if (hayEntrada) animacion.trabajando = false;
            else if (trabajar) animacion.trabajando = !animacion.trabajando;

            Camera cam = Camera.main;
            if (velPlano.sqrMagnitude > 0.0025f && cam != null)
            {
                Vector3 cf = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
                Alejandose = Vector3.Dot(velPlano.normalized, cf) >= umbralAlejarse;
            }
            // Se mantiene el último sentido: NioAnimacion solo usa Back mientras Speed > 0.1.
            animacion.alejandose = usarBackAlAlejarse && Alejandose;
        }

        AjustarVisual();
    }

    void AjustarVisual()
    {
        HaySuelo = false;
        if (!ajustarPiesAlSuelo) return;
        Vector3 origen = transform.position + Vector3.up * 0.3f;
        int n = Physics.RaycastNonAlloc(origen, Vector3.down, impactos, 0.65f, capasSuelo, QueryTriggerInteraction.Ignore);
        float mejor = float.MaxValue;
        for (int i = 0; i < n; i++)
        {
            if (impactos[i].collider == cc || impactos[i].collider.transform.IsChildOf(transform)) continue;
            if (impactos[i].distance < mejor) { mejor = impactos[i].distance; AlturaSuelo = impactos[i].point.y; HaySuelo = true; }
        }
        if (!HaySuelo) return;
        float dy = AlturaSuelo - transform.position.y;
        if (visual != null) visual.localPosition = new Vector3(0f, dy + elevacionVisual, 0f);
        if (sombra != null) sombra.localPosition = new Vector3(0f, dy + elevacionSombra, 0f);
    }
}
