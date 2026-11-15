using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarsCat.Unity.View
{
    /// <summary>
    /// Puerta automática de la cabina. Va en el objeto trigger COL_TRG_PUERTA_&lt;id&gt; (Collider con Is Trigger).
    /// Se abre mientras haya un <see cref="CatActor"/> dentro del trigger y se cierra tras <c>_closeDelay</c> segundos.
    /// Adaptada de CabinaPuertaAutomatica (prototipo): misma mecánica, pero los desplazamientos y el centro
    /// de giro se interpretan en el espacio local de <c>_space</c> (la cabina), no en coordenadas de mundo.
    /// </summary>
    [DisallowMultipleComponent]
    public class AutoDoor : MonoBehaviour
    {
        // El orden debe coincidir con el enum original (Deslizar = 0, GirarSobreEje = 1) para conservar los datos.
        public enum DoorMode { Slide, Rotate }

        [FormerlySerializedAs("puertaId")]
        [SerializeField] private string _doorId;

        [Tooltip("Objeto cuyo espacio local interpreta _offsets y _pivot (la raíz de la cabina). " +
                 "Vacío = coordenadas de mundo (solo válido si la cabina está sin rotar ni mover).")]
        [SerializeField] private Transform _space;

        [Tooltip("Hojas de la puerta (objetos PUERTA_*).")]
        [FormerlySerializedAs("hojas")]
        [SerializeField] private Transform[] _leaves = new Transform[0];

        [Tooltip("Modo Slide: desplazamiento de cada hoja con la puerta abierta, en el espacio local de _space.")]
        [FormerlySerializedAs("desplazamientos")]
        [SerializeField] private Vector3[] _offsets = new Vector3[0];

        [FormerlySerializedAs("modo")]
        [SerializeField] private DoorMode _mode = DoorMode.Slide;

        [Tooltip("Modo Rotate: punto del eje vertical de giro (centro de la sala), en el espacio local de _space.")]
        [FormerlySerializedAs("centroGiro")]
        [SerializeField] private Vector3 _pivot;

        [Tooltip("Modo Rotate: ángulo de apertura en grados (con signo).")]
        [FormerlySerializedAs("anguloGrados")]
        [SerializeField] private float _angleDegrees;

        [Tooltip("Collider que bloquea el paso con la puerta cerrada (COL_PUERTA_<id>).")]
        [FormerlySerializedAs("colliderBloqueo")]
        [SerializeField] private Collider _blocker;

        [Tooltip("Si está marcada, la puerta no se abre.")]
        [FormerlySerializedAs("bloqueada")]
        [SerializeField] private bool _locked;

        [Tooltip("Invierte el sentido de apertura si una hoja se abre hacia el lado equivocado.")]
        [FormerlySerializedAs("invertirSentido")]
        [SerializeField] private bool _invertDirection;

        [Tooltip("Velocidad de las hojas en m/s.")]
        [FormerlySerializedAs("velocidad")]
        [SerializeField] private float _speed = 4f;

        [Tooltip("Segundos que sigue abierta tras salir el último gato.")]
        [FormerlySerializedAs("retardoCierre")]
        [SerializeField] private float _closeDelay = 0.6f;

        [Tooltip("Opcional: renderer de la baliza INT_LuzAdvertencia_<id> (encendida mientras la puerta se mueve).")]
        [FormerlySerializedAs("indicador")]
        [SerializeField] private Renderer _indicator;

        /// <summary>La puerta dejó de estar cerrada (empieza a abrirse).</summary>
        public event Action<AutoDoor> Opened;

        /// <summary>La puerta terminó de cerrarse y bloquea el paso.</summary>
        public event Action<AutoDoor> Closed;

        public string DoorId => _doorId;

        /// <summary>Apertura de 0 (cerrada) a 1 (abierta del todo).</summary>
        public float OpenAmount => _open;

        public bool IsFullyOpen => _open >= 0.999f;

        /// <summary>Hojas cerradas y collider de bloqueo activo: no se puede pasar.</summary>
        public bool IsClosed => _open <= 0.001f && (_blocker == null || _blocker.enabled);

        public bool Locked
        {
            get => _locked;
            set => _locked = value;
        }

        private Vector3[] _closedPositions;
        private Quaternion[] _closedRotations;
        private float _open;
        private float _travel = 0.05f;      // recorrido (m) para convertir velocidad en m/s a apertura 0..1
        private float _exitTime;
        private bool _wasClosed = true;
        private readonly HashSet<Collider> _occupants = new HashSet<Collider>();
        private readonly List<Collider> _stale = new List<Collider>();

        // Con _space vacío se usan coordenadas de mundo (identidad).
        private Vector3 ToWorldPoint(Vector3 local) => _space != null ? _space.TransformPoint(local) : local;
        private Vector3 ToWorldDirection(Vector3 local) => _space != null ? _space.TransformDirection(local) : local;
        private Vector3 Up => _space != null ? _space.up : Vector3.up;

        private void Awake()
        {
            if (_space == null)
            {
                Debug.LogWarning($"AutoDoor '{name}': falta _space; se usan coordenadas de mundo y la puerta se abrirá mal si la cabina está rotada o movida.", this);
            }

            _closedPositions = new Vector3[_leaves.Length];
            _closedRotations = new Quaternion[_leaves.Length];
            for (int i = 0; i < _leaves.Length; i++)
            {
                if (_leaves[i] == null) continue;
                _closedPositions[i] = _leaves[i].position;
                _closedRotations[i] = _leaves[i].rotation;
                if (_mode == DoorMode.Slide && i < _offsets.Length)
                    _travel = Mathf.Max(_travel, ToWorldDirection(_offsets[i]).magnitude);
            }

            if (_mode == DoorMode.Rotate && _leaves.Length > 0 && _leaves[0] != null)
            {
                // Longitud del arco que recorre la primera hoja alrededor del eje.
                Vector3 radius = Vector3.ProjectOnPlane(_leaves[0].position - ToWorldPoint(_pivot), Up);
                _travel = Mathf.Max(0.05f, Mathf.Abs(_angleDegrees) * Mathf.Deg2Rad * radius.magnitude);
            }

            // El trigger no debe verse ni bloquear: solo detecta.
            var trigger = GetComponent<Collider>();
            if (trigger != null) trigger.isTrigger = true;
            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null) meshRenderer.enabled = false;
        }

        private void OnDisable()
        {
            _occupants.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<CatActor>() != null) _occupants.Add(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (_occupants.Remove(other)) _exitTime = Time.time;
        }

        private void Update()
        {
            PurgeStaleOccupants();

            bool wantOpen = !_locked && _occupants.Count > 0;
            float target = wantOpen ? 1f : (Time.time - _exitTime > _closeDelay ? 0f : _open);
            _open = Mathf.MoveTowards(_open, target, Time.deltaTime * _speed / _travel);

            ApplyPose();

            if (_blocker != null) _blocker.enabled = _open <= 0.001f && !wantOpen;
            if (_indicator != null)
                _indicator.enabled = (_open > 0.001f && _open < 0.999f) || (_locked && (Time.time * 2f) % 1f > 0.5f);

            bool closed = IsClosed;
            if (closed != _wasClosed)
            {
                _wasClosed = closed;
                if (closed) Closed?.Invoke(this);
                else Opened?.Invoke(this);
            }
        }

        // OnTriggerExit no se llama si el collider se destruye o desactiva dentro del trigger.
        private void PurgeStaleOccupants()
        {
            if (_occupants.Count == 0) return;
            _stale.Clear();
            foreach (var c in _occupants)
                if (c == null || !c.enabled || !c.gameObject.activeInHierarchy) _stale.Add(c);
            if (_stale.Count == 0) return;
            foreach (var c in _stale) _occupants.Remove(c);
            _exitTime = Time.time;
        }

        private void ApplyPose()
        {
            float sign = _invertDirection ? -1f : 1f;

            if (_mode == DoorMode.Rotate)
            {
                Vector3 pivot = ToWorldPoint(_pivot);
                Quaternion q = Quaternion.AngleAxis(_angleDegrees * _open * sign, Up);
                for (int i = 0; i < _leaves.Length; i++)
                {
                    if (_leaves[i] == null) continue;
                    _leaves[i].SetPositionAndRotation(
                        pivot + q * (_closedPositions[i] - pivot),
                        q * _closedRotations[i]);
                }
            }
            else
            {
                for (int i = 0; i < _leaves.Length && i < _offsets.Length; i++)
                {
                    if (_leaves[i] == null) continue;
                    _leaves[i].position = _closedPositions[i] + ToWorldDirection(_offsets[i]) * (_open * sign);
                }
            }
        }
    }
}
