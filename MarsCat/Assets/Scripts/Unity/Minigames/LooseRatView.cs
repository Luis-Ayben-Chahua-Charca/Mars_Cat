// Asistencia de IA: escrito con Claude Code (Anthropic).
using UnityEngine;

namespace MarsCat.Unity.Minigames
{
    /// <summary>
    /// Una rata suelta de la estación (La Redada, #60). Adaptador: se oculta cuando la lógica la da por tomada
    /// (LooseRats.OnTaken), muestra un indicador cuando el jugador la marca (RatMarked) y lleva el Collider que
    /// RatRaidView usa para detectar el clic. No decide nada: la lógica vive en MarsCat.Logic.Food.
    /// </summary>
    public class LooseRatView : MonoBehaviour
    {
        [Tooltip("Id de la rata en LooseRats (0 .. rat.loose.initial - 1). Debe ser único.")]
        [SerializeField] private int _ratId;

        [SerializeField] private GameBootstrap _bootstrap;

        [Tooltip("Opcional: se enciende cuando la rata está marcada (un gato va por ella).")]
        [SerializeField] private GameObject _markedIndicator;

        [Tooltip("Modelo de la rata; se apaga cuando deja de estar suelta. Si está vacío se usa este mismo objeto " +
                 "(el Collider queda apagado igual por quedar el objeto inactivo).")]
        [SerializeField] private GameObject _visual;

        [Tooltip("Movimiento de reposo (sube y baja). 0 = sin animación.")]
        [SerializeField] private float _bobHeight = 0.05f;

        [SerializeField] private float _bobSpeed = 3f;

        private bool _attached;
        private bool _taken;
        private Vector3 _visualBase;
        private float _phase;

        public int RatId => _ratId;

        private void Awake()
        {
            _phase = Random.value * Mathf.PI * 2f;
            if (_visual != null) _visualBase = _visual.transform.localPosition;
            if (_markedIndicator != null) _markedIndicator.SetActive(false);
        }

        private void OnEnable()
        {
            if (_bootstrap == null) return;
            if (_bootstrap.IsReady) Attach();
            else _bootstrap.Initialized += Attach;
        }

        private void OnDisable() => Detach();

        private void OnDestroy() => Detach();

        private void Attach()
        {
            if (_bootstrap == null || _attached) return;
            _bootstrap.Initialized -= Attach;
            if (_bootstrap.LooseRats == null) return;
            _attached = true;
            _bootstrap.LooseRats.OnTaken += HandleTaken;
            _bootstrap.RatMarked += HandleMarked;
            Refresh();
        }

        private void Detach()
        {
            if (_bootstrap == null) return;
            _bootstrap.Initialized -= Attach;
            if (!_attached) return;
            _attached = false;
            if (_bootstrap.LooseRats != null) _bootstrap.LooseRats.OnTaken -= HandleTaken;
            _bootstrap.RatMarked -= HandleMarked;
        }

        private void Refresh()
        {
            bool loose = _bootstrap.LooseRats.IsLoose(_ratId);
            SetTaken(!loose);
            if (loose && _markedIndicator != null && _bootstrap.RatRaid != null)
                _markedIndicator.SetActive(_bootstrap.RatRaid.IsMarked(_ratId));
        }

        private void HandleTaken(int id)
        {
            if (id == _ratId) SetTaken(true);
        }

        private void HandleMarked(int id)
        {
            if (id == _ratId && !_taken && _markedIndicator != null) _markedIndicator.SetActive(true);
        }

        private void SetTaken(bool taken)
        {
            _taken = taken;
            if (taken && _markedIndicator != null) _markedIndicator.SetActive(false);
            // Con _visual asignado solo se apaga el modelo (el script y el indicador siguen vivos para desuscribir);
            // sin _visual se apaga el objeto entero, que también deja de recibir el clic.
            if (_visual != null) _visual.SetActive(!taken);
            else gameObject.SetActive(!taken);
            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider col in colliders) col.enabled = !taken;
        }

        private void Update()
        {
            if (_taken || _visual == null || _bobHeight <= 0f) return;
            float y = Mathf.Sin(Time.time * _bobSpeed + _phase) * _bobHeight;
            _visual.transform.localPosition = _visualBase + new Vector3(0f, y, 0f);
        }
    }
}
