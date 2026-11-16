// Asistencia de IA: escrito con Claude Code (Anthropic).
using UnityEngine;

namespace MarsCat.Unity.View
{
    /// <summary>
    /// Vista 2D de un gato en la escena 3D. Va en el objeto del sprite (Animator + SpriteRenderer).
    /// - Billboard solo en Y: el sprite se orienta como la cámara, sin inclinarse.
    /// - Flip horizontal según el movimiento relativo a la cámara.
    /// - Envía Speed, IsWorking e IsBack al Animator (controller Nio.controller).
    /// No conoce MarsCat.Logic: el puente (#26) llama a <see cref="SetWorking"/>.
    /// El movimiento lo decide otro objeto; aquí solo se observa el desplazamiento del transform.
    /// El suelo es plano (y = 0), así que se ignora el eje vertical.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class CatView : MonoBehaviour
    {
        private static readonly int SpeedParam = Animator.StringToHash("Speed");
        private static readonly int IsWorkingParam = Animator.StringToHash("IsWorking");
        private static readonly int IsBackParam = Animator.StringToHash("IsBack");

        // Velocidad por encima de la cual el Animator pasa de Idle a Walk (y se permite Back).
        private const float WalkThreshold = 0.1f;

        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _renderer;

        [Tooltip("Vacío = Camera.main.")]
        [SerializeField] private Camera _camera;

        [Tooltip("Los sprites de Nio_Walk miran hacia la derecha.")]
        [SerializeField] private bool _spritesFaceRight = true;

        [Tooltip("Suavizado de la velocidad calculada (evita parpadeos Walk/Idle en los cambios de sentido).")]
        [SerializeField] private float _smoothing = 20f;

        [Tooltip("Coseno mínimo entre el movimiento y la dirección de la cámara para mostrar la animación de espaldas (0.5 = 60°).")]
        [Range(0f, 1f)]
        [SerializeField] private float _backThreshold = 0.5f;

        private Vector3 _previousPosition;
        private bool _working;
        private bool _movingAway;

        /// <summary>Velocidad horizontal suavizada, en m/s.</summary>
        public float Speed { get; private set; }

        public bool IsWorking => _working;

        private void Reset()
        {
            _animator = GetComponent<Animator>();
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<Animator>();
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            _previousPosition = transform.position;
        }

        private void OnEnable()
        {
            // Evita un salto de velocidad si el objeto se movió mientras estaba desactivado.
            _previousPosition = transform.position;
        }

        /// <summary>Activa o desactiva la animación de trabajo (la llama el puente con la lógica).</summary>
        public void SetWorking(bool working)
        {
            _working = working;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Vector3 delta = transform.position - _previousPosition;
            delta.y = 0f;
            _previousPosition = transform.position;

            if (dt > 0f)
                Speed = Mathf.Lerp(Speed, delta.magnitude / dt, 1f - Mathf.Exp(-_smoothing * dt));

            Camera cam = ResolveCamera();
            Vector3 camForward = Vector3.zero;
            if (cam != null)
            {
                camForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up);
                if (camForward.sqrMagnitude > 1e-6f)
                {
                    camForward.Normalize();
                    transform.rotation = Quaternion.LookRotation(camForward, Vector3.up);
                }
            }

            // Se conserva el último sentido: Back solo se usa mientras Speed > WalkThreshold.
            if (cam != null && delta.sqrMagnitude > 1e-8f && camForward != Vector3.zero)
                _movingAway = Vector3.Dot(delta.normalized, camForward) >= _backThreshold;

            _animator.SetFloat(SpeedParam, Speed);
            _animator.SetBool(IsWorkingParam, _working);
            _animator.SetBool(IsBackParam, _movingAway && Speed > WalkThreshold);

            if (cam != null)
            {
                float lateral = Vector3.Dot(delta, cam.transform.right);
                if (Mathf.Abs(lateral) > 1e-4f)
                    _renderer.flipX = _spritesFaceRight ? lateral < 0f : lateral > 0f;
            }
        }

        private Camera ResolveCamera()
        {
            if (_camera != null) return _camera;
            return Camera.main;
        }
    }
}
