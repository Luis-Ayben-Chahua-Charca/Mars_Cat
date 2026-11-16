using System;
using MarsCat.Logic.Buildings;
using UnityEngine;

namespace MarsCat.Unity.View
{
    /// <summary>
    /// Va en la raíz del prefab de cada edificio de la escena. Muestra el estado del Building de la lógica
    /// (color por estado, resaltado de selección, ícono de atención). No contiene reglas del juego.
    /// Necesita un Collider en este objeto o en alguno de sus hijos: SelectionController lo usa para encontrarlo.
    /// El depósito (depot) no tiene Building en la lógica: queda como "solo información" (Building es null).
    /// </summary>
    [DisallowMultipleComponent]
    public class BuildingView : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [Tooltip("GameBootstrap de la escena.")]
        [SerializeField] private GameBootstrap _bootstrap;

        [Tooltip("Id del edificio: solar_panels, moxie, water_extractor, plant_farm, rat_farm, organic_recycler o depot.")]
        [SerializeField] private string _buildingId;

        [Tooltip("Renderers que se tiñen según el estado. Se usa MaterialPropertyBlock (no se instancian materiales).")]
        [SerializeField] private Renderer[] _renderers;

        [Header("Colores por estado")]
        [SerializeField] private Color _damaged = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color _repairing = new Color(0.95f, 0.7f, 0.2f);
        [SerializeField] private Color _unprepared = new Color(0.9f, 0.9f, 0.3f);
        [SerializeField] private Color _running = new Color(0.3f, 0.8f, 0.4f);
        [SerializeField] private Color _depleted = new Color(0.5f, 0.5f, 0.55f);

        [Tooltip("Opcional: se activa mientras el edificio está seleccionado.")]
        [SerializeField] private GameObject _selectionHighlight;

        [Tooltip("Opcional: ícono flotante que se activa en Sin preparar y Agotado.")]
        [SerializeField] private GameObject _needsAttentionIcon;

        private MaterialPropertyBlock _block;
        private Building _building;

        public string BuildingId => _buildingId;

        /// <summary>Edificio de la lógica; null para el depósito o antes de inicializar.</summary>
        public Building Building => _building;

        public bool IsSelected { get; private set; }

        /// <summary>Cambió la selección de este edificio.</summary>
        public event Action<BuildingView, bool> SelectedChanged;

        public void SetSelected(bool selected)
        {
            if (IsSelected == selected) return;
            IsSelected = selected;
            if (_selectionHighlight != null) _selectionHighlight.SetActive(selected);
            SelectedChanged?.Invoke(this, selected);
        }

        private void Awake()
        {
            if (_selectionHighlight != null) _selectionHighlight.SetActive(false);
            if (_needsAttentionIcon != null) _needsAttentionIcon.SetActive(false);
        }

        private void Start()
        {
            if (_bootstrap == null)
            {
                Debug.LogWarning("BuildingView: falta asignar GameBootstrap.", this);
                return;
            }
            _bootstrap.WhenReady(Bind);
        }

        private void Bind()
        {
            _building = _bootstrap.GetBuilding(_buildingId);
            if (_building == null) return; // depot: solo información
            _building.OnStateChanged += HandleStateChanged;
            Apply(_building.State);
        }

        private void HandleStateChanged(Building building, BuildingState from, BuildingState to) => Apply(to);

        private void Apply(BuildingState state)
        {
            Color color;
            switch (state)
            {
                case BuildingState.Damaged: color = _damaged; break;
                case BuildingState.Repairing: color = _repairing; break;
                case BuildingState.Unprepared: color = _unprepared; break;
                case BuildingState.Running: color = _running; break;
                default: color = _depleted; break;
            }

            if (_renderers != null)
            {
                if (_block == null) _block = new MaterialPropertyBlock();
                foreach (Renderer r in _renderers)
                {
                    if (r == null) continue;
                    r.GetPropertyBlock(_block);
                    _block.SetColor(BaseColorId, color);
                    _block.SetColor(ColorId, color);
                    r.SetPropertyBlock(_block);
                }
            }

            if (_needsAttentionIcon != null)
                _needsAttentionIcon.SetActive(state == BuildingState.Unprepared || state == BuildingState.Depleted);
        }

        private void OnDestroy()
        {
            if (_building != null) _building.OnStateChanged -= HandleStateChanged;
        }
    }
}
