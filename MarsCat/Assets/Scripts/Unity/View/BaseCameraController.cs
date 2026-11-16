using UnityEngine;
using UnityEngine.InputSystem;

namespace MarsCat.Unity.View
{
    /// <summary>
    /// Cámara 2.5D de la base (#25). Va en un objeto "rig" (pivote en el suelo) con la cámara como hija.
    /// El rig se inclina en X para mirar desde arriba en ángulo; el zoom cambia la distancia de la cámara
    /// al pivote (posición local (0, 0, -distancia)). Funciona en pausa (usa tiempo sin escala).
    /// </summary>
    public class BaseCameraController : MonoBehaviour
    {
        [Tooltip("Opcional: avisa al tutorial la primera vez que el jugador mueve la cámara.")]
        [SerializeField] private GameBootstrap _bootstrap;

        [Tooltip("Cámara hija del rig. Si es null no hay zoom.")]
        [SerializeField] private Transform _cameraTransform;

        [Header("Movimiento")]
        [Tooltip("Velocidad de movimiento con teclado (unidades por segundo).")]
        [SerializeField] private float _moveSpeed = 12f;

        [Tooltip("Sensibilidad al arrastrar con botón central o derecho (unidades por pixel, escalada con el zoom).")]
        [SerializeField] private float _dragSpeed = 0.02f;

        [Tooltip("Suavizado del movimiento (más alto = alcanza el objetivo más rápido).")]
        [SerializeField] private float _smoothing = 10f;

        [Tooltip("Cuánto debe alejarse (unidades) del punto inicial para contar como 'mover la cámara' en el tutorial.")]
        [SerializeField] private float _tutorialMoveThreshold = 1f;

        [Header("Zoom")]
        [Tooltip("Distancia mínima de la cámara al pivote.")]
        [SerializeField] private float _minDistance = 6f;

        [Tooltip("Distancia máxima de la cámara al pivote.")]
        [SerializeField] private float _maxDistance = 40f;

        [Tooltip("Distancia inicial.")]
        [SerializeField] private float _startDistance = 20f;

        [Tooltip("Cuánto cambia la distancia por cada paso de la rueda.")]
        [SerializeField] private float _zoomSpeed = 2f;

        [Header("Rotación")]
        [Tooltip("Permitir rotar el rig con Q/E.")]
        [SerializeField] private bool _allowRotation = true;

        [Tooltip("Velocidad de rotación (grados por segundo).")]
        [SerializeField] private float _rotationSpeed = 90f;

        [Header("Límites del área (plano XZ)")]
        [Tooltip("Esquina mínima (X, Z) del área por donde se puede mover el pivote.")]
        [SerializeField] private Vector2 _boundsMin = new Vector2(-30f, -30f);

        [Tooltip("Esquina máxima (X, Z) del área por donde se puede mover el pivote.")]
        [SerializeField] private Vector2 _boundsMax = new Vector2(30f, 30f);

        private Vector3 _targetPosition;
        private float _targetDistance;
        private float _currentDistance;
        private float _targetYaw;
        private float _currentYaw;
        private Vector3 _startPosition;
        private bool _notified;

        /// <summary>Acerca la cámara suavemente a un punto del mundo.</summary>
        public void FocusOn(Vector3 worldPoint)
        {
            _targetPosition = ClampToBounds(new Vector3(worldPoint.x, transform.position.y, worldPoint.z));
        }

        private void Awake()
        {
            _targetPosition = transform.position;
            _startPosition = _targetPosition;
            _targetYaw = _currentYaw = transform.eulerAngles.y;
            _targetDistance = _currentDistance = Mathf.Clamp(_startDistance, _minDistance, _maxDistance);
            ApplyDistance();
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            ReadInput(dt);

            float t = 1f - Mathf.Exp(-_smoothing * dt);
            transform.position = Vector3.Lerp(transform.position, _targetPosition, t);
            _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, t);
            _currentYaw = Mathf.LerpAngle(_currentYaw, _targetYaw, t);

            Vector3 euler = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(euler.x, _currentYaw, euler.z);
            ApplyDistance();

            NotifyTutorialIfMoved();
        }

        private void ReadInput(float dt)
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
            Vector3 right = transform.right;
            right.y = 0f;
            right = right.sqrMagnitude > 0.0001f ? right.normalized : Vector3.right;

            if (keyboard != null)
            {
                Vector3 move = Vector3.zero;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) move += forward;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) move -= forward;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) move += right;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) move -= right;
                if (move.sqrMagnitude > 1f) move.Normalize();
                _targetPosition += move * (_moveSpeed * dt);

                if (_allowRotation)
                {
                    if (keyboard.qKey.isPressed) _targetYaw -= _rotationSpeed * dt;
                    if (keyboard.eKey.isPressed) _targetYaw += _rotationSpeed * dt;
                }
            }

            if (mouse != null)
            {
                if (mouse.middleButton.isPressed || mouse.rightButton.isPressed)
                {
                    Vector2 delta = mouse.delta.ReadValue();
                    float scale = _dragSpeed * (_targetDistance / Mathf.Max(0.01f, _startDistance));
                    _targetPosition -= (right * delta.x + forward * delta.y) * scale;
                }

                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                    _targetDistance = Mathf.Clamp(_targetDistance - Mathf.Sign(scroll) * _zoomSpeed, _minDistance, _maxDistance);
            }

            _targetPosition = ClampToBounds(_targetPosition);
        }

        private Vector3 ClampToBounds(Vector3 position)
        {
            position.x = Mathf.Clamp(position.x, Mathf.Min(_boundsMin.x, _boundsMax.x), Mathf.Max(_boundsMin.x, _boundsMax.x));
            position.z = Mathf.Clamp(position.z, Mathf.Min(_boundsMin.y, _boundsMax.y), Mathf.Max(_boundsMin.y, _boundsMax.y));
            return position;
        }

        private void ApplyDistance()
        {
            if (_cameraTransform == null) return;
            _cameraTransform.localPosition = new Vector3(0f, 0f, -_currentDistance);
        }

        private void NotifyTutorialIfMoved()
        {
            if (_notified || _bootstrap == null) return;
            if ((_targetPosition - _startPosition).sqrMagnitude < _tutorialMoveThreshold * _tutorialMoveThreshold) return;
            _notified = true;
            _bootstrap.NotifyCameraMoved();
        }
    }
}
