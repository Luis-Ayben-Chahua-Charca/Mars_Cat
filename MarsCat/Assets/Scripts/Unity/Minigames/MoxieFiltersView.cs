// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Linq;
using MarsCat.Logic.Air;
using MarsCat.Logic.Buildings;
using MarsCat.Logic.Cats;
using MarsCat.Logic.Preparation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsCat.Unity.Minigames
{
    /// <summary>
    /// Panel de filtros del MOXIE (#59). Es mantenimiento, no el ciclo de preparación: con el MOXIE sin preparar o
    /// agotado ofrece "Poner en marcha"; con el MOXIE en marcha deja pedir la limpieza de 1, 2 o 5 filtros (la hace
    /// un gato como tarea puntual). Se puede cerrar en cualquier momento: la limpieza sigue sola.
    /// Adaptador: la lógica vive en MoxieFilters y GameBootstrap.
    /// </summary>
    public class MoxieFiltersView : MinigameView
    {
        [Serializable]
        public class WorkerRow
        {
            [Tooltip("Fila completa (se oculta si sobra).")]
            public GameObject Root;

            [Tooltip("Nombre del edificio y del gato que trabaja en él.")]
            public TMP_Text Label;

            [Tooltip("Botón 'Liberar gato' de la fila.")]
            public Button ReleaseButton;

            [Tooltip("Texto del botón de la fila.")]
            public TMP_Text ReleaseLabel;
        }

        [Header("Filtros y eficiencia")]
        [Tooltip("Un ícono por filtro (en orden). Los que sobren respecto a la cantidad de filtros del balance se ocultan.")]
        [SerializeField] private Image[] _filterIcons = new Image[0];

        [Tooltip("Sprite de filtro limpio (opcional; si falta solo se usa el color).")]
        [SerializeField] private Sprite _cleanSprite;

        [Tooltip("Sprite de filtro sucio (opcional; si falta solo se usa el color).")]
        [SerializeField] private Sprite _dirtySprite;

        [Tooltip("Color de un filtro limpio.")]
        [SerializeField] private Color _cleanColor = new Color(0.4f, 0.9f, 0.55f, 1f);

        [Tooltip("Color de un filtro sucio.")]
        [SerializeField] private Color _dirtyColor = new Color(0.45f, 0.32f, 0.22f, 1f);

        [Tooltip("Relleno (Image Filled) de la barra de eficiencia = Yield del MOXIE.")]
        [SerializeField] private Image _efficiencyFill;

        [Tooltip("Texto de la eficiencia (porcentaje).")]
        [SerializeField] private TMP_Text _efficiencyText;

        [Tooltip("Texto de estado: quién limpia, si se espera un gato, etc.")]
        [SerializeField] private TMP_Text _statusText;

        [Header("Poner en marcha")]
        [Tooltip("Grupo visible solo si el MOXIE está sin preparar o agotado.")]
        [SerializeField] private GameObject _startGroup;

        [SerializeField] private Button _startButton;
        [SerializeField] private TMP_Text _startLabel;

        [Header("Mantenimiento")]
        [Tooltip("Grupo visible solo con el MOXIE en marcha.")]
        [SerializeField] private GameObject _maintenanceGroup;

        [Tooltip("Botones de limpieza, en el orden de _cleanCounts (1, 2, 5).")]
        [SerializeField] private Button[] _cleanButtons = new Button[0];

        [Tooltip("Textos de esos botones (mismo orden).")]
        [SerializeField] private TMP_Text[] _cleanLabels = new TMP_Text[0];

        [Tooltip("Cuántos filtros limpia cada botón (la lógica solo admite 1, 2 o 5).")]
        [SerializeField] private int[] _cleanCounts = { 1, 2, 5 };

        [Header("Elegir gato")]
        [Tooltip("Grupo visible si no hay gatos libres (lista de edificios con trabajador).")]
        [SerializeField] private GameObject _freeCatsGroup;

        [Tooltip("Mensaje sobre la lista (no hay gatos libres).")]
        [SerializeField] private TMP_Text _freeCatsMessage;

        [Tooltip("Filas preasignadas (hasta 6); las sobrantes se ocultan.")]
        [SerializeField] private WorkerRow[] _workerRows = new WorkerRow[0];

        [Tooltip("Cada cuántos segundos reales se refresca el panel además de los eventos.")]
        [SerializeField] private float _refreshInterval = 0.25f;

        private readonly string[] _rowBuildingIds = new string[8];
        private int _filterCount = 5;
        private float _sinceRefresh;
        private bool _listenersAdded;

        protected override void Awake()
        {
            base.Awake();
            AddUiListeners();
        }

        protected override void OnOpened()
        {
            _filterCount = Mathf.Max(1, MoxieFilterConfig.FromBalance(Bootstrap.Balance).FilterCount);
            _sinceRefresh = 0f;
            Bootstrap.FiltersCleaned += HandleFiltersCleaned;
            Bootstrap.WorkerChanged += HandleWorkerChanged;
            if (Bootstrap.MoxieFilters != null) Bootstrap.MoxieFilters.OnFilterDirty += HandleFilterDirty;
            Refresh();
        }

        protected override void OnClosed()
        {
            if (Bootstrap == null) return;
            Bootstrap.FiltersCleaned -= HandleFiltersCleaned;
            Bootstrap.WorkerChanged -= HandleWorkerChanged;
            if (Bootstrap.MoxieFilters != null) Bootstrap.MoxieFilters.OnFilterDirty -= HandleFilterDirty;
        }

        protected override void Tick(float realDt)
        {
            _sinceRefresh += realDt;
            if (_sinceRefresh < _refreshInterval) return;
            _sinceRefresh = 0f;
            Refresh();
        }

        private void OnDestroy()
        {
            RemoveUiListeners();
        }

        // ---------------- Eventos ----------------

        private void HandleFiltersCleaned(int count) => Refresh();
        private void HandleWorkerChanged(string buildingId, Cat cat) => Refresh();
        private void HandleFilterDirty() => Refresh();

        private void AddUiListeners()
        {
            if (_listenersAdded) return;
            _listenersAdded = true;
            if (_startButton != null) _startButton.onClick.AddListener(OnStartClicked);
            for (int i = 0; i < _cleanButtons.Length; i++)
            {
                if (_cleanButtons[i] == null) continue;
                int index = i;
                _cleanButtons[i].onClick.AddListener(() => OnCleanClicked(index));
            }
            for (int i = 0; i < _workerRows.Length; i++)
            {
                if (_workerRows[i]?.ReleaseButton == null) continue;
                int index = i;
                _workerRows[i].ReleaseButton.onClick.AddListener(() => OnReleaseClicked(index));
            }
        }

        private void RemoveUiListeners()
        {
            if (!_listenersAdded) return;
            _listenersAdded = false;
            if (_startButton != null) _startButton.onClick.RemoveListener(OnStartClicked);
            foreach (Button button in _cleanButtons) if (button != null) button.onClick.RemoveAllListeners();
            foreach (WorkerRow row in _workerRows)
                if (row?.ReleaseButton != null) row.ReleaseButton.onClick.RemoveAllListeners();
        }

        private void OnStartClicked()
        {
            if (!IsOpen || Bootstrap.MoxieFilters == null) return;
            float yield = Bootstrap.MoxieFilters.Yield;
            PreparationResult result = yield > 0f
                ? PreparationResult.Ok(yield)
                : PreparationResult.Fail("prep.moxie.filters_dirty");
            Finish(result, close: false);
            Refresh();
        }

        private void OnCleanClicked(int index)
        {
            if (!IsOpen || index < 0 || index >= _cleanCounts.Length) return;
            Bootstrap.StartFilterCleaning(_cleanCounts[index]);
            Refresh();
        }

        private void OnReleaseClicked(int index)
        {
            if (!IsOpen || index < 0 || index >= _rowBuildingIds.Length) return;
            string buildingId = _rowBuildingIds[index];
            if (string.IsNullOrEmpty(buildingId)) return;
            Bootstrap.ReleaseWorker(buildingId);
            Refresh();
        }

        // ---------------- Pintado ----------------

        private void Refresh()
        {
            if (!IsOpen || Bootstrap == null || Bootstrap.MoxieFilters == null) return;
            MoxieFilters filters = Bootstrap.MoxieFilters;
            Building moxie = Bootstrap.GetBuilding(GameBootstrap.MoxieId);
            BuildingState state = moxie != null ? moxie.State : BuildingState.Damaged;
            bool canStart = state == BuildingState.Unprepared || state == BuildingState.Depleted;
            bool running = state == BuildingState.Running;
            bool cleaning = Bootstrap.IsCleaningFilters;
            int clean = filters.CleanFilters;
            int dirty = Mathf.Max(0, _filterCount - clean);

            PaintFilters(clean);

            if (_efficiencyFill != null) _efficiencyFill.fillAmount = Mathf.Clamp01(filters.Yield);
            if (_efficiencyText != null)
                _efficiencyText.text = Bootstrap.Texts.Format("minigame.moxie.efficiency", Mathf.RoundToInt(filters.Yield * 100f));

            if (_startGroup != null) _startGroup.SetActive(canStart);
            if (_startButton != null) _startButton.interactable = canStart;
            if (_startLabel != null) _startLabel.text = Bootstrap.Texts.Get("minigame.moxie.start");

            if (_maintenanceGroup != null) _maintenanceGroup.SetActive(running);
            bool cleaningWithCat = cleaning && Bootstrap.GetWorker(GameBootstrap.MoxieId) != null;
            for (int i = 0; i < _cleanButtons.Length; i++)
            {
                if (_cleanButtons[i] == null) continue;
                _cleanButtons[i].interactable = running && !cleaning && dirty > 0;
                if (i < _cleanLabels.Length && _cleanLabels[i] != null && i < _cleanCounts.Length)
                    _cleanLabels[i].text = Bootstrap.Texts.Format("minigame.moxie.clean_n", _cleanCounts[i]);
            }

            if (_statusText != null)
            {
                string key = null;
                if (cleaning) key = cleaningWithCat ? "minigame.moxie.cleaning" : "minigame.moxie.waiting_cat";
                else if (running && dirty == 0) key = "minigame.moxie.all_clean";
                _statusText.text = key == null ? string.Empty : Bootstrap.Texts.Get(key);
            }

            bool noFreeCats = !Bootstrap.Colony.FreeCats.Any();
            bool showList = running && noFreeCats && !cleaningWithCat && dirty > 0;
            if (_freeCatsGroup != null) _freeCatsGroup.SetActive(showList);
            if (_freeCatsMessage != null) _freeCatsMessage.text = Bootstrap.Texts.Get("minigame.moxie.no_free_cats");
            if (showList) PaintWorkerRows();
        }

        private void PaintFilters(int clean)
        {
            for (int i = 0; i < _filterIcons.Length; i++)
            {
                Image icon = _filterIcons[i];
                if (icon == null) continue;
                bool exists = i < _filterCount;
                if (icon.gameObject.activeSelf != exists) icon.gameObject.SetActive(exists);
                if (!exists) continue;
                bool isClean = i < clean;
                Sprite sprite = isClean ? _cleanSprite : _dirtySprite;
                if (sprite != null) icon.sprite = sprite;
                icon.color = isClean ? _cleanColor : _dirtyColor;
            }
        }

        private void PaintWorkerRows()
        {
            int used = 0;
            foreach (var pair in Bootstrap.Buildings)
            {
                if (used >= _workerRows.Length) break;
                Cat worker = Bootstrap.GetWorker(pair.Key);
                if (worker == null) continue;
                WorkerRow row = _workerRows[used];
                if (row == null) { used++; continue; }
                _rowBuildingIds[used] = pair.Key;
                if (row.Root != null) row.Root.SetActive(true);
                if (row.Label != null)
                    row.Label.text = Bootstrap.Texts.Format("minigame.moxie.worker_row",
                        Bootstrap.Texts.Get("building." + pair.Key + ".name"), worker.Id);
                if (row.ReleaseLabel != null) row.ReleaseLabel.text = Bootstrap.Texts.Get("minigame.moxie.release");
                used++;
            }
            for (int i = used; i < _workerRows.Length; i++)
            {
                if (i < _rowBuildingIds.Length) _rowBuildingIds[i] = null;
                if (_workerRows[i]?.Root != null) _workerRows[i].Root.SetActive(false);
            }
        }
    }
}
