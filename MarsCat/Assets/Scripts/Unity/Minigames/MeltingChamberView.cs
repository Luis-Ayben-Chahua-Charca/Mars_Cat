// Asistencia de IA: escrito con Claude Code (Anthropic).
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Water;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MarsCat.Unity.Minigames
{
    /// <summary>
    /// Cámara de derretido del extractor de agua (#30, paso 2): presurizar y calentar. Adaptador de
    /// MeltingChamberSession: lee la entrada (A/D, flechas o arrastre del mouse; Shift o botón para acelerar),
    /// dibuja el dial con la zona verde, el progreso y los avisos. Al terminar entrega el resultado al host.
    /// </summary>
    public sealed class MeltingChamberView : MinigameView
    {
        // Arrastre del mouse: fracción del ancho de pantalla (desde el centro) que equivale a entrada total.
        private const float MouseRangeFraction = 0.25f;
        private const float MessageSeconds = 2.5f;

        [Header("Textos")]
        [Tooltip("Nombre de la fase actual (Presurizar / Calentar).")]
        [SerializeField] private TMP_Text _phaseText;
        [Tooltip("Ayuda de controles bajo el dial.")]
        [SerializeField] private TMP_Text _hintText;
        [Tooltip("Estado del modo acelerado y su costo en electricidad.")]
        [SerializeField] private TMP_Text _accelText;
        [Tooltip("Aviso corto (fase reiniciada, agua hervida...).")]
        [SerializeField] private TMP_Text _messageText;

        [Header("Dial y progreso")]
        [Tooltip("Aguja del dial: pivote en su base, se rota en Z de +90 (izquierda) a -90 (derecha).")]
        [SerializeField] private RectTransform _needle;
        [Tooltip("Zona verde: Image Filled Radial180 que cubre el dial; el script fija su ancho y la centra.")]
        [SerializeField] private Image _greenZone;
        [Tooltip("Círculo de progreso: Image Filled Radial360.")]
        [SerializeField] private Image _progressFill;

        [Header("Acelerar")]
        [Tooltip("Botón de acelerar (alterna). Mantener Shift también acelera.")]
        [SerializeField] private Button _accelButton;
        [Tooltip("Texto del botón de acelerar.")]
        [SerializeField] private TMP_Text _accelButtonLabel;

        [Header("Resultado")]
        [Tooltip("Panel que aparece al terminar (éxito o lote perdido).")]
        [SerializeField] private GameObject _resultPanel;
        [Tooltip("Texto del resultado (litros que entraron o motivo de la pérdida).")]
        [SerializeField] private TMP_Text _resultText;
        [Tooltip("Botón Continuar (solo si salió bien).")]
        [SerializeField] private Button _continueButton;
        [Tooltip("Texto del botón Continuar.")]
        [SerializeField] private TMP_Text _continueLabel;
        [Tooltip("Botón Reintentar (solo si se perdió el lote).")]
        [SerializeField] private Button _retryButton;
        [Tooltip("Texto del botón Reintentar.")]
        [SerializeField] private TMP_Text _retryLabel;
        [Tooltip("Botón Salir (solo si se perdió el lote).")]
        [SerializeField] private Button _exitButton;
        [Tooltip("Texto del botón Salir.")]
        [SerializeField] private TMP_Text _exitLabel;

        private MeltingChamberSession _session;
        private WaterExtraction _extraction;
        private bool _toggleAccelerate;
        private bool _lastWanted;
        private bool _finished;
        private bool _extracted;
        private float _litersAdded;
        private string _lostKey;
        private PreparationResult _result;
        private float _messageTimer;

        protected override void OnOpened()
        {
            if (_accelButton != null) _accelButton.onClick.AddListener(OnAccelerateClicked);
            if (_continueButton != null) _continueButton.onClick.AddListener(OnContinue);
            if (_retryButton != null) _retryButton.onClick.AddListener(OnRetry);
            if (_exitButton != null) _exitButton.onClick.AddListener(OnExit);

            SetText(_hintText, "minigame.melt.hint");
            SetText(_continueLabel, "minigame.melt.continue");
            SetText(_retryLabel, "minigame.melt.retry");
            SetText(_exitLabel, "minigame.melt.exit");
            StartSession();
        }

        protected override void OnClosed()
        {
            if (_accelButton != null) _accelButton.onClick.RemoveListener(OnAccelerateClicked);
            if (_continueButton != null) _continueButton.onClick.RemoveListener(OnContinue);
            if (_retryButton != null) _retryButton.onClick.RemoveListener(OnRetry);
            if (_exitButton != null) _exitButton.onClick.RemoveListener(OnExit);
            DisposeSession();
        }

        protected override void Tick(float realDt)
        {
            if (_session == null || _finished) return;

            bool wanted = _toggleAccelerate || IsShiftHeld();
            if (wanted != _lastWanted)
            {
                _lastWanted = wanted;
                _session.SetAccelerated(wanted);
                if (wanted && !_session.IsAccelerated) ShowMessage("minigame.melt.msg.no_energy");
            }

            _session.Tick(realDt, ReadInput());

            if (_messageTimer > 0f)
            {
                _messageTimer -= realDt;
                if (_messageTimer <= 0f && _messageText != null) _messageText.text = string.Empty;
            }
            if (!_finished) Refresh();
        }

        private void StartSession()
        {
            DisposeSession();
            if (Bootstrap == null || Bootstrap.MeltingConfig == null || Bootstrap.Resources == null || Bootstrap.IceVein == null)
            {
                Debug.LogWarning("MeltingChamberView: falta MeltingConfig, Resources o IceVein en el GameBootstrap.");
                return;
            }

            _finished = false;
            _extracted = false;
            _litersAdded = 0f;
            _lostKey = null;
            _toggleAccelerate = false;
            _lastWanted = false;
            _messageTimer = 0f;
            if (_messageText != null) _messageText.text = string.Empty;
            if (_resultPanel != null) _resultPanel.SetActive(false);

            _session = new MeltingChamberSession(Bootstrap.MeltingConfig, Bootstrap.Resources, Bootstrap.NextMinigameSeed());
            // La extracción se suscribe primero: así sus eventos llegan antes que OnSessionFinished.
            _extraction = new WaterExtraction(Bootstrap.IceVein, _session, new NoTreatment(), Bootstrap.Resources);
            _extraction.OnWaterExtracted += OnWaterExtracted;
            _extraction.OnBatchLost += OnBatchLost;
            _session.OnPhaseCompleted += OnPhaseCompleted;
            _session.OnPhaseFailed += OnPhaseFailed;
            _session.OnFinished += OnSessionFinished;

            PlaceGreenZone();
            Refresh();
        }

        private void DisposeSession()
        {
            if (_extraction != null)
            {
                _extraction.OnWaterExtracted -= OnWaterExtracted;
                _extraction.OnBatchLost -= OnBatchLost;
                _extraction = null;
            }
            if (_session != null)
            {
                _session.OnPhaseCompleted -= OnPhaseCompleted;
                _session.OnPhaseFailed -= OnPhaseFailed;
                _session.OnFinished -= OnSessionFinished;
                _session = null;
            }
        }

        // Zona verde: Radial180 con origen abajo empieza en el extremo izquierdo; se gira para centrarla arriba.
        private void PlaceGreenZone()
        {
            if (_greenZone == null || Bootstrap == null || Bootstrap.MeltingConfig == null) return;
            float width = Mathf.Clamp01(Bootstrap.MeltingConfig.GreenWidth);
            _greenZone.type = Image.Type.Filled;
            _greenZone.fillMethod = Image.FillMethod.Radial180;
            _greenZone.fillOrigin = (int)Image.Origin180.Bottom;
            _greenZone.fillClockwise = true;
            _greenZone.fillAmount = width;
            _greenZone.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -90f * (1f - width));
        }

        private void Refresh()
        {
            if (_session == null) return;
            if (_needle != null) _needle.localRotation = Quaternion.Euler(0f, 0f, 90f - 180f * Mathf.Clamp01(_session.Needle));
            if (_progressFill != null) _progressFill.fillAmount = Mathf.Clamp01(_session.Progress);

            if (_phaseText != null)
            {
                switch (_session.Phase)
                {
                    case MeltPhase.Pressurize: _phaseText.text = T("minigame.melt.phase.pressure"); break;
                    case MeltPhase.Heat: _phaseText.text = T("minigame.melt.phase.heat"); break;
                    default: _phaseText.text = string.Empty; break;
                }
            }

            float cost = Bootstrap != null && Bootstrap.MeltingConfig != null ? Bootstrap.MeltingConfig.AccelerateExtraKwh : 0f;
            string state = _session.IsAccelerated ? T("minigame.melt.accel.on") : T("minigame.melt.accel.off");
            if (_accelText != null) _accelText.text = string.Format(state, cost.ToString("0.#"));
            if (_accelButtonLabel != null) _accelButtonLabel.text = T("minigame.melt.accel.button");
        }

        private float ReadInput()
        {
            float input = 0f;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input += 1f;
            }

            Mouse mouse = Mouse.current;
            if (input == 0f && mouse != null && mouse.leftButton.isPressed)
            {
                // Arrastre: la posición horizontal respecto al centro de la pantalla es la entrada.
                float half = Mathf.Max(1f, Screen.width * MouseRangeFraction);
                input = Mathf.Clamp((mouse.position.ReadValue().x - Screen.width * 0.5f) / half, -1f, 1f);
            }
            return Mathf.Clamp(input, -1f, 1f);
        }

        private static bool IsShiftHeld()
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
        }

        private void OnAccelerateClicked() => _toggleAccelerate = !_toggleAccelerate;

        private void OnPhaseCompleted(MeltPhase phase)
        {
            if (phase == MeltPhase.Pressurize) ShowMessage("minigame.melt.msg.phase_done");
        }

        private void OnPhaseFailed(MeltPhase phase)
        {
            ShowMessage(phase == MeltPhase.Pressurize ? "minigame.melt.msg.pressure_failed" : "minigame.melt.msg.boiled");
        }

        private void OnWaterExtracted(float liters)
        {
            _extracted = true;
            _litersAdded = liters;
        }

        private void OnBatchLost(string reasonKey) => _lostKey = reasonKey;

        private void OnSessionFinished(PreparationResult result)
        {
            _finished = true;
            // Si la sesión salió bien pero el lote no era apto, cuenta como fallo.
            _result = result.Starts && !_extracted && !string.IsNullOrEmpty(_lostKey)
                ? PreparationResult.Fail(_lostKey)
                : result;

            Refresh();
            if (_resultPanel != null) _resultPanel.SetActive(true);
            bool ok = _result.Starts;
            if (_continueButton != null) _continueButton.gameObject.SetActive(ok);
            if (_retryButton != null) _retryButton.gameObject.SetActive(!ok);
            if (_exitButton != null) _exitButton.gameObject.SetActive(!ok);
            if (_resultText != null)
            {
                _resultText.text = ok
                    ? string.Format(T("minigame.melt.result.ok"), Mathf.FloorToInt(_litersAdded))
                    : T("minigame.melt.result.lost");
            }
        }

        private void OnContinue()
        {
            if (_finished && _result.Starts) Finish(_result);
        }

        private void OnRetry()
        {
            if (_finished && !_result.Starts) StartSession();
        }

        private void OnExit()
        {
            if (_finished && !_result.Starts) Finish(_result);
        }

        private void ShowMessage(string key)
        {
            if (_messageText == null) return;
            _messageText.text = T(key);
            _messageTimer = MessageSeconds;
        }

        private void SetText(TMP_Text target, string key)
        {
            if (target != null) target.text = T(key);
        }

        private string T(string key) => Bootstrap != null && Bootstrap.Texts != null ? Bootstrap.Texts.Get(key) : key;
    }
}
