// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Text;
using MarsCat.Logic.Buildings;
using MarsCat.Logic.Cats;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;
using MarsCat.Unity.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsCat.Unity.UI
{
    /// <summary>
    /// Panel de detalle del edificio seleccionado (§9, abajo a la derecha). Solo muestra lo que dice la lógica
    /// y lanza las acciones (reparar, preparar); no tiene reglas propias.
    /// </summary>
    public class DetailPanel : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _bootstrap;
        [SerializeField] private SelectionController _selection;

        [Tooltip("Raíz visual del panel; se oculta cuando no hay selección.")]
        [SerializeField] private GameObject _root;

        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _state;

        [Tooltip("Texto multilínea: rendimiento, trabajador y energía.")]
        [SerializeField] private TMP_Text _details;

        [SerializeField] private Button _repairButton;
        [SerializeField] private TMP_Text _repairLabel;
        [SerializeField] private Button _prepareButton;
        [SerializeField] private TMP_Text _prepareLabel;

        [Tooltip("Cada cuántos segundos se refresca el panel (la energía cambia sin eventos).")]
        [SerializeField] private float _refreshInterval = 0.5f;

        [Tooltip("Sin minijuego suscrito, Preparar aplica una preparación perfecta para probar el ciclo.")]
        [SerializeField] private bool _debugAutoPrepare = true;

        private readonly StringBuilder _sb = new StringBuilder();
        private BuildingView _current;
        private Building _currentBuilding;
        private ResourcePool _pool;
        private float _timer;
        private bool _bound;

        /// <summary>El jugador pidió preparar el edificio (buildingId); lo abre el minijuego.</summary>
        public event Action<string> PrepareRequested;

        private void OnEnable()
        {
            if (_repairButton != null) _repairButton.onClick.AddListener(OnRepairClicked);
            if (_prepareButton != null) _prepareButton.onClick.AddListener(OnPrepareClicked);
            if (_selection != null) _selection.SelectionChanged += OnSelectionChanged;
            if (_bootstrap != null) _bootstrap.WhenReady(Bind);
            Refresh();
        }

        private void OnDisable()
        {
            if (_repairButton != null) _repairButton.onClick.RemoveListener(OnRepairClicked);
            if (_prepareButton != null) _prepareButton.onClick.RemoveListener(OnPrepareClicked);
            if (_selection != null) _selection.SelectionChanged -= OnSelectionChanged;
            Unbind();
            DetachBuilding();
        }

        private void Bind()
        {
            if (_bound || !isActiveAndEnabled) return;
            _bound = true;
            _pool = _bootstrap.Resources;
            _pool.OnChanged += OnResourceChanged;
            _bootstrap.WorkerChanged += OnWorkerChanged;
            if (_selection != null) OnSelectionChanged(_selection.Selected);
        }

        private void Unbind()
        {
            if (!_bound) return;
            _bound = false;
            if (_pool != null) _pool.OnChanged -= OnResourceChanged;
            if (_bootstrap != null) _bootstrap.WorkerChanged -= OnWorkerChanged;
            _pool = null;
        }

        private void DetachBuilding()
        {
            if (_currentBuilding != null) _currentBuilding.OnStateChanged -= OnBuildingStateChanged;
            _currentBuilding = null;
        }

        private void OnSelectionChanged(BuildingView view)
        {
            DetachBuilding();
            _current = view;
            if (_current != null && _bootstrap != null && _bootstrap.IsReady)
            {
                _currentBuilding = _bootstrap.GetBuilding(_current.BuildingId);
                if (_currentBuilding != null) _currentBuilding.OnStateChanged += OnBuildingStateChanged;
            }
            Refresh();
        }

        private void OnBuildingStateChanged(Building b, BuildingState from, BuildingState to) => Refresh();
        private void OnResourceChanged(ResourceId id, float value) => Refresh();
        private void OnWorkerChanged(string id, Cat cat) => Refresh();

        private void Update()
        {
            if (_current == null) return;
            _timer += Time.unscaledDeltaTime;
            if (_timer < _refreshInterval) return;
            _timer = 0f;
            Refresh();
        }

        private string T(string key) => _bootstrap != null && _bootstrap.Texts != null ? _bootstrap.Texts.Get(key) : key;

        private string F(string key, params object[] args) =>
            _bootstrap != null && _bootstrap.Texts != null ? _bootstrap.Texts.Format(key, args) : key;

        private void Refresh()
        {
            bool visible = _current != null && _bootstrap != null && _bootstrap.IsReady;
            if (_root != null) _root.SetActive(visible);
            if (!visible) return;

            string id = _current.BuildingId;
            SetText(_title, T("building." + id + ".name"));

            if (_currentBuilding == null)
            {
                RefreshDepot();
                return;
            }

            BuildingState state = _currentBuilding.State;
            SetText(_state, T("building.state." + state.ToString().ToLowerInvariant()));
            RefreshDetails(id, state);
            RefreshButtons(id, state);
        }

        private void RefreshDepot()
        {
            SetText(_state, string.Empty);
            _sb.Clear();
            if (_pool != null)
            {
                _sb.Append(T("resource.cat_waste.name")).Append(": ").Append(Mathf.FloorToInt(_pool.Get(ResourceId.CatWaste))).Append('\n');
                _sb.Append(T("resource.rat_waste.name")).Append(": ").Append(Mathf.FloorToInt(_pool.Get(ResourceId.RatWaste)));
            }
            SetText(_details, _sb.ToString());
            SetActive(_repairButton, false);
            SetActive(_prepareButton, false);
        }

        private void RefreshDetails(string id, BuildingState state)
        {
            _sb.Clear();
            if (state == BuildingState.Running)
                _sb.Append(F("ui.panel.yield", _currentBuilding.Yield)).Append('\n');

            Cat worker = _bootstrap.GetWorker(id);
            _sb.Append(worker != null ? F("ui.panel.worker", worker.Id) : T("ui.panel.no_worker")).Append('\n');

            if (_bootstrap.Energy != null)
                _sb.Append(_bootstrap.Energy.IsPowered(id) ? T("ui.panel.powered") : T("ui.panel.unpowered"));
            SetText(_details, _sb.ToString());
        }

        private void RefreshButtons(string id, BuildingState state)
        {
            bool damaged = state == BuildingState.Damaged;
            SetActive(_repairButton, damaged);
            if (damaged)
            {
                float metal = _bootstrap.Balance.Get("building." + id + ".repair_metal_cost");
                float debris = _bootstrap.Balance.Get("building." + id + ".repair_debris_cost");
                bool enough = _pool != null && _pool.Get(ResourceId.Metal) >= metal && _pool.Get(ResourceId.Debris) >= debris;
                if (_repairButton != null) _repairButton.interactable = enough;
                SetText(_repairLabel, T("ui.panel.repair") + "\n" + F("ui.panel.repair_cost", metal, debris)
                    + (enough ? string.Empty : "\n" + T("ui.panel.not_enough")));
            }

            bool needsPrep = state == BuildingState.Unprepared || state == BuildingState.Depleted;
            SetActive(_prepareButton, needsPrep);
            if (needsPrep) SetText(_prepareLabel, T("ui.panel.open_minigame"));
        }

        private void OnRepairClicked()
        {
            if (_current == null || _bootstrap == null) return;
            _bootstrap.RequestRepair(_current.BuildingId);
            Refresh();
        }

        private void OnPrepareClicked()
        {
            if (_current == null || _bootstrap == null) return;
            string id = _current.BuildingId;
            if (PrepareRequested != null) PrepareRequested.Invoke(id);
            else if (_debugAutoPrepare) _bootstrap.ApplyPreparation(id, PreparationResult.Ok(1f));
            Refresh();
        }

        private static void SetText(TMP_Text label, string text)
        {
            if (label != null) label.text = text;
        }

        private static void SetActive(Button button, bool active)
        {
            if (button != null && button.gameObject.activeSelf != active) button.gameObject.SetActive(active);
        }
    }
}
