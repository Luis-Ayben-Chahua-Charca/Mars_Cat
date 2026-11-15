// Controlador en primera persona basado en CharacterController.
// Controles: WASD/flechas = mover · ratón = mirar · Shift = correr · Espacio = saltar · clic = capturar ratón · Esc = liberarlo.
// Funciona con el Input System nuevo (ENABLE_INPUT_SYSTEM) y con el Input Manager clásico (ENABLE_LEGACY_INPUT_MANAGER).
// CabinaPruebaRecorrido lo mueve en modo automático con MoverAutomatico().
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class CabinaControladorPrimeraPersona : MonoBehaviour
{
    public Transform camara;
    public float velocidad = 3.5f;
    public float velocidadCorrer = 6f;
    public float alturaSalto = 1.0f;
    public float gravedad = -19.6f;
    [Tooltip("Sensibilidad del ratón (grados por píxel con el Input System nuevo).")]
    public float sensibilidadRaton = 0.12f;
    [Tooltip("Sensibilidad con el Input Manager clásico (grados por unidad de eje).")]
    public float sensibilidadClasica = 2.0f;
    public float limiteVertical = 85f;
    [Tooltip("Desactivado por CabinaPruebaRecorrido durante la prueba automática.")]
    public bool controlManual = true;

    CharacterController cc;
    float pitch, vy;
    Vector3 movimientoAutomatico;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (camara == null && Camera.main != null) camara = Camera.main.transform;
    }

    /// <summary>Velocidad horizontal en mundo (m/s) usada cuando controlManual = false.</summary>
    public void MoverAutomatico(Vector3 velocidadMundo)
    {
        velocidadMundo.y = 0f;
        movimientoAutomatico = velocidadMundo;
    }

    void Update()
    {
        Vector2 mov = Vector2.zero, mirar = Vector2.zero;
        bool correr = false, saltar = false;
        if (controlManual) LeerEntrada(ref mov, ref mirar, ref correr, ref saltar);

        transform.Rotate(0f, mirar.x, 0f, Space.Self);
        pitch = Mathf.Clamp(pitch - mirar.y, -limiteVertical, limiteVertical);
        if (camara != null) camara.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        Vector3 horizontal = controlManual
            ? (transform.right * mov.x + transform.forward * mov.y) * (correr ? velocidadCorrer : velocidad)
            : movimientoAutomatico;
        if (controlManual && mov.sqrMagnitude > 1f) horizontal = horizontal.normalized * (correr ? velocidadCorrer : velocidad);

        if (cc.isGrounded && vy < 0f) vy = -2f;
        if (saltar && cc.isGrounded) vy = Mathf.Sqrt(2f * alturaSalto * -gravedad);
        vy += gravedad * Time.deltaTime;
        cc.Move((horizontal + Vector3.up * vy) * Time.deltaTime);
    }

    void LeerEntrada(ref Vector2 mov, ref Vector2 mirar, ref bool correr, ref bool saltar)
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        var m = Mouse.current;
        if (k != null)
        {
            mov.x = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1f : 0f) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1f : 0f);
            mov.y = (k.wKey.isPressed || k.upArrowKey.isPressed ? 1f : 0f) - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1f : 0f);
            correr = k.leftShiftKey.isPressed;
            saltar = k.spaceKey.wasPressedThisFrame;
            if (k.escapeKey.wasPressedThisFrame) LiberarRaton();
        }
        if (m != null)
        {
            if (m.leftButton.wasPressedThisFrame) CapturarRaton();
            if (Cursor.lockState == CursorLockMode.Locked) mirar = m.delta.ReadValue() * sensibilidadRaton;
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        mov = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        correr = Input.GetKey(KeyCode.LeftShift);
        saltar = Input.GetKeyDown(KeyCode.Space);
        if (Input.GetKeyDown(KeyCode.Escape)) LiberarRaton();
        if (Input.GetMouseButtonDown(0)) CapturarRaton();
        if (Cursor.lockState == CursorLockMode.Locked)
            mirar = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * sensibilidadClasica;
#endif
    }

    static void CapturarRaton() { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    static void LiberarRaton() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
}
