// Asistencia de IA: escrito con Claude Code (Anthropic).
using MarsCat.Logic.Energy;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MarsCat.Unity.Minigames
{
    /// <summary>
    /// Operación Rescate Solar (edificio solar_panels). Adaptador de UI sobre SolarCleaningSession: dibuja la fila
    /// actual frente a las boquillas, pasa los disparos (Q/W/E o botones) y muestra el resultado al terminar.
    /// Las reglas viven en la sesión; aquí solo hay presentación y entrada.
    /// </summary>
    public sealed class SolarCleaningView : MinigameView
    {
        [Tooltip("Una Image por carril con la celda de la fila actual (cambia de color/sprite si está sucia o limpia).")]
        [SerializeField] private Image[] _cells;

        [Tooltip("Una Image por carril para el destello de la boquilla al disparar.")]
        [SerializeField] private Image[] _nozzleFlashes;

        [Tooltip("Un botón por carril para disparar con el ratón (el carril es el índice del arreglo).")]
        [SerializeField] private Button[] _fireButtons;

        [Tooltip("Barra (Image Filled) con el tiempo transcurrido de la fila actual.")]
        [SerializeField] private Image _rowTimeBar;

        [Tooltip("Texto de filas restantes.")]
        [SerializeField] private TMP_Text _rowsText;

        [Tooltip("Texto de eficiencia en porcentaje.")]
        [SerializeField] private TMP_Text _efficiencyText;

        [Tooltip("Texto de electricidad disponible.")]
        [SerializeField] private TMP_Text _energyText;

        [Tooltip("Aviso breve cuando un disparo no sale por falta de electricidad.")]
        [SerializeField] private TMP_Text _warningText;

        [Tooltip("Panel de resultado final (se muestra al terminar la banda).")]
        [SerializeField] private GameObject _resultPanel;

        [Tooltip("Texto del resultado final.")]
        [SerializeField] private TMP_Text _resultText;

        [Tooltip("Botón Continuar: entrega el resultado al host.")]
        [SerializeField] private Button _continueButton;

        [Tooltip("Texto del botón Continuar.")]
        [SerializeField] private TMP_Text _continueLabel;

        [Tooltip("Sprite de celda sucia (opcional; si falta se usa el color).")]
        [SerializeField] private Sprite _dirtySprite;

        [Tooltip("Sprite de celda limpia (opcional; si falta se usa el color).")]
        [SerializeField] private Sprite _cleanSprite;

        [Tooltip("Color de una celda sucia.")]
        [SerializeField] private Color _dirtyColor = new Color(0.45f, 0.28f, 0.16f, 1f);

        [Tooltip("Color de una celda limpia.")]
        [SerializeField] private Color _cleanColor = new Color(0.25f, 0.55f, 0.95f, 1f);

        [Tooltip("Segundos que tarda en apagarse el destello de la boquilla.")]
        [SerializeField] private float _flashSeconds = 0.25f;

        [Tooltip("Segundos que se muestra el aviso de falta de energía.")]
        [SerializeField] private float _warningSeconds = 1.2f;

        private SolarCleaningSession _session;
        private float _rowElapsed;
        private float _rowSeconds;
        private float _warningLeft;
        private float[] _flashLeft;
        private PreparationResult _result;
        private bool _hasResult;
        private UnityAction[] _fireActions;

        protected override void OnOpened()
        {
            SolarCleaningConfig config = Bootstrap.SolarCleaningConfig;
            _rowSeconds = config.RowSeconds;
            _rowElapsed = 0f;
            _warningLeft = 0f;
            _hasResult = false;
            _flashLeft = new float[_nozzleFlashes != null ? _nozzleFlashes.Length : 0];

            _session = new SolarCleaningSession(config, Bootstrap.Resources, Bootstrap.NextMinigameSeed());
            _session.OnRowPassed += HandleRowPassed;
            _session.OnFinished += HandleFinished;

            int buttons = _fireButtons != null ? _fireButtons.Length : 0;
            _fireActions = new UnityAction[buttons];
            for (int i = 0; i < buttons; i++)
            {
                if (_fireButtons[i] == null) continue;
                int lane = i;
                _fireActions[i] = () => TryFire(lane);
                _fireButtons[i].onClick.AddListener(_fireActions[i]);
            }
            if (_continueButton != null) _continueButton.onClick.AddListener(OnContinue);
            if (_continueLabel != null) _continueLabel.text = Bootstrap.Texts.Get("minigame.solar.continue");
            if (_resultPanel != null) _resultPanel.SetActive(false);
            if (_warningText != null) _warningText.gameObject.SetActive(false);
            for (int i = 0; i < _flashLeft.Length; i++) SetFlashAlpha(i, 0f);

            Refresh();
        }

        protected override void OnClosed()
        {
            if (_session != null)
            {
                _session.OnRowPassed -= HandleRowPassed;
                _session.OnFinished -= HandleFinished;
                _session = null;
            }
            if (_fireButtons != null && _fireActions != null)
            {
                for (int i = 0; i < _fireButtons.Length && i < _fireActions.Length; i++)
                    if (_fireButtons[i] != null && _fireActions[i] != null)
                        _fireButtons[i].onClick.RemoveListener(_fireActions[i]);
            }
            _fireActions = null;
            if (_continueButton != null) _continueButton.onClick.RemoveListener(OnContinue);
        }

        protected override void Tick(float realDt)
        {
            if (_session == null) return;

            if (!_session.IsFinished)
            {
                ReadKeys();
                _rowElapsed += realDt;
                _session.Tick(realDt);
            }
            else
            {
                Keyboard keyboard = Keyboard.current;
                if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
                    OnContinue();
            }

            if (_session == null) return;
            UpdateEffects(realDt);
            UpdateLiveTexts();
        }

        private void ReadKeys()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.qKey.wasPressedThisFrame) TryFire(0);
            if (keyboard.wKey.wasPressedThisFrame) TryFire(1);
            if (keyboard.eKey.wasPressedThisFrame) TryFire(2);
        }

        private void TryFire(int lane)
        {
            if (_session == null || _session.IsFinished) return;
            if (_cells != null && lane >= _cells.Length) return;
            if (_session.Fire(lane))
            {
                if (_flashLeft != null && lane < _flashLeft.Length) _flashLeft[lane] = _flashSeconds;
                RefreshCells();
            }
            else
            {
                _warningLeft = _warningSeconds;
                if (_warningText != null)
                {
                    _warningText.text = Bootstrap.Texts.Get("minigame.solar.no_energy");
                    _warningText.gameObject.SetActive(true);
                }
            }
        }

        private void HandleRowPassed(int cleaned, int missed)
        {
            _rowElapsed = 0f;
            Refresh();
        }

        private void HandleFinished(PreparationResult result)
        {
            _result = result;
            _hasResult = true;
            Refresh();
            if (_resultText != null)
            {
                int percent = Mathf.RoundToInt(_session.Efficiency * 100f);
                _resultText.text = Bootstrap.Texts.Format("minigame.solar.result", percent);
            }
            if (_resultPanel != null) _resultPanel.SetActive(true);
        }

        private void OnContinue()
        {
            if (!_hasResult) return;
            _hasResult = false;
            Finish(_result);
        }

        private void Refresh()
        {
            RefreshCells();
            UpdateLiveTexts();
        }

        private void RefreshCells()
        {
            if (_session == null || _cells == null) return;
            bool[] row = _session.CurrentRow;
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] == null) continue;
                bool dirty = i < row.Length && row[i];
                Sprite sprite = dirty ? _dirtySprite : _cleanSprite;
                if (sprite != null) _cells[i].sprite = sprite;
                _cells[i].color = sprite != null ? Color.white : (dirty ? _dirtyColor : _cleanColor);
            }
        }

        private void UpdateLiveTexts()
        {
            if (_rowTimeBar != null)
                _rowTimeBar.fillAmount = _session.IsFinished ? 1f : Mathf.Clamp01(_rowElapsed / _rowSeconds);
            if (_rowsText != null)
                _rowsText.text = Bootstrap.Texts.Format("minigame.solar.rows", _session.RowsRemaining);
            if (_efficiencyText != null)
                _efficiencyText.text = Bootstrap.Texts.Format("minigame.solar.efficiency", Mathf.RoundToInt(_session.Efficiency * 100f));
            if (_energyText != null)
                _energyText.text = Bootstrap.Texts.Format("minigame.solar.energy", Mathf.FloorToInt(Bootstrap.Resources.Get(ResourceId.Electricity)));
        }

        private void UpdateEffects(float dt)
        {
            if (_flashLeft != null)
            {
                for (int i = 0; i < _flashLeft.Length; i++)
                {
                    if (_flashLeft[i] <= 0f) continue;
                    _flashLeft[i] = Mathf.Max(0f, _flashLeft[i] - dt);
                    SetFlashAlpha(i, _flashSeconds > 0f ? _flashLeft[i] / _flashSeconds : 0f);
                }
            }
            if (_warningLeft > 0f)
            {
                _warningLeft -= dt;
                if (_warningLeft <= 0f && _warningText != null) _warningText.gameObject.SetActive(false);
            }
        }

        private void SetFlashAlpha(int lane, float alpha)
        {
            if (_nozzleFlashes == null || lane >= _nozzleFlashes.Length || _nozzleFlashes[lane] == null) return;
            Color color = _nozzleFlashes[lane].color;
            color.a = alpha;
            _nozzleFlashes[lane].color = color;
        }
    }
}
