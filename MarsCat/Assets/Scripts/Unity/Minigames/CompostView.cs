// Asistencia de IA: escrito con Claude Code (Anthropic).
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Recycling;
using MarsCat.Logic.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MarsCat.Unity.Minigames
{
    /// <summary>
    /// Minijuego del reciclador orgánico (organic_recycler), en dos fases dentro del mismo panel:
    /// 1) Compostador: un slider por insumo y la relación C:N de la mezcla contra su guía y rango (CompostMix).
    /// 2) Operación Sopa de Bacterias: temperatura y humedad en su zona verde durante el ciclo (CompostSession).
    /// Es un adaptador: las reglas viven en MarsCat.Logic.Recycling; aquí solo se dibuja y se lee el teclado
    /// (Q baja / E sube la perilla de calor, Espacio mantenido rocía; el mouse no controla la sopa).
    /// </summary>
    public class CompostView : MinigameView
    {
        private const int InputCount = 3;

        private static readonly ResourceId[] Inputs = { ResourceId.CatWaste, ResourceId.RatWaste, ResourceId.CropResidue };
        private static readonly string[] InputNameKeys =
        {
            "resource.cat_waste.name", "resource.rat_waste.name", "resource.crop_residue.name"
        };

        [Header("Fases")]
        [Tooltip("Fase 1: Compostador (mezcla). Hijo de Root.")]
        [SerializeField] private GameObject _mixPhase;

        [Tooltip("Fase 2: Sopa de Bacterias. Hijo de Root.")]
        [SerializeField] private GameObject _soupPhase;

        [Tooltip("Título del panel (cambia según la fase).")]
        [SerializeField] private TMP_Text _titleText;

        [Header("Fase 1: mezcla (en orden: desechos de gato, cama de rata, restos de cosecha)")]
        [SerializeField] private Slider[] _inputSliders = new Slider[InputCount];
        [SerializeField] private TMP_Text[] _inputLabels = new TMP_Text[InputCount];
        [SerializeField] private TMP_Text[] _inputAmountTexts = new TMP_Text[InputCount];
        [SerializeField] private TMP_Text _ratioText;
        [SerializeField] private TMP_Text _rangeText;
        [SerializeField] private TMP_Text _totalText;
        [Tooltip("Motivo por el que la mezcla no sirve (o aviso de que está en rango).")]
        [SerializeField] private TMP_Text _mixMessageText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private TMP_Text _confirmLabel;

        [Header("Fase 2: indicadores (pistas horizontales; el script coloca zonas y marcas desde CompostConfig)")]
        [SerializeField] private TMP_Text _tempText;
        [SerializeField] private RectTransform _tempGreenZone;
        [SerializeField] private RectTransform _tempOverheatMark;
        [SerializeField] private RectTransform _tempNeedle;
        [SerializeField] private TMP_Text _humidityText;
        [SerializeField] private RectTransform _humidityGreenZone;
        [SerializeField] private RectTransform _humidityDryMark;
        [SerializeField] private RectTransform _humidityDrownMark;
        [SerializeField] private RectTransform _humidityNeedle;

        [Header("Fase 2: controles")]
        [Tooltip("Relleno (Filled) de la perilla de calor, 0 a 1.")]
        [SerializeField] private Image _heatFill;
        [SerializeField] private TMP_Text _heatText;
        [SerializeField] private TMP_Text _heatKeysText;
        [Tooltip("Velocidad de la perilla de calor con Q/E, en fracción (0 a 1) por segundo.")]
        [SerializeField, Min(0.01f)] private float _heatSpeed = 0.5f;
        [SerializeField] private Image _sprinklerLamp;
        [SerializeField] private Color _sprinklerOnColor = new Color(0.3f, 0.7f, 1f, 1f);
        [SerializeField] private Color _sprinklerOffColor = new Color(1f, 1f, 1f, 0.2f);
        [SerializeField] private TMP_Text _sprinklerText;
        [SerializeField] private TMP_Text _sprinklerKeysText;
        [Tooltip("Aviso corto de la IA según lo cerca que esté cada aguja de un borde.")]
        [SerializeField] private TMP_Text _warningText;

        [Header("Fase 2: ciclo y resultado")]
        [Tooltip("Relleno (Filled) del tiempo en verde sobre la duración del ciclo.")]
        [SerializeField] private Image _timeBar;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private TMP_Text _yieldText;
        [Tooltip("Cubre la fase 2 al terminar el ciclo (oculto mientras corre).")]
        [SerializeField] private GameObject _resultGroup;
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private TMP_Text _continueLabel;

        private CompostMix _mix;
        private CompostSession _session;
        private CompostConfig _config;
        private float _mixScore;
        private float _heatLevel;
        private float _elapsed;
        private bool _hasResult;
        private PreparationResult _result;
        private float _tempMin;
        private float _tempMax;

        protected override void OnOpened()
        {
            _mix = null;
            _session = null;
            _hasResult = false;
            if (Bootstrap == null || Bootstrap.Resources == null || Bootstrap.CompostMixConfig == null || Bootstrap.CompostConfig == null)
            {
                Debug.LogWarning("CompostView: el GameBootstrap no tiene la lógica del reciclador lista.", this);
                Close();
                return;
            }
            _config = Bootstrap.CompostConfig;
            _mix = new CompostMix(Bootstrap.CompostMixConfig, Bootstrap.Resources);

            if (_confirmButton != null) _confirmButton.onClick.AddListener(OnConfirmClicked);
            if (_continueButton != null) _continueButton.onClick.AddListener(OnContinueClicked);
            SetupMixPhase();
            ShowPhase(true);
        }

        protected override void OnClosed()
        {
            if (_confirmButton != null) _confirmButton.onClick.RemoveListener(OnConfirmClicked);
            if (_continueButton != null) _continueButton.onClick.RemoveListener(OnContinueClicked);
            for (int i = 0; i < _inputSliders.Length; i++)
                if (_inputSliders[i] != null) _inputSliders[i].onValueChanged.RemoveAllListeners();
            if (_session != null) _session.OnFinished -= HandleSessionFinished;
            _session = null;
            _mix = null;
        }

        protected override void Tick(float realDt)
        {
            if (_session == null || _session.IsFinished) return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                float direction = 0f;
                if (keyboard.eKey.isPressed) direction += 1f;
                if (keyboard.qKey.isPressed) direction -= 1f;
                if (direction != 0f)
                {
                    _heatLevel = Mathf.Clamp01(_heatLevel + direction * _heatSpeed * realDt);
                    _session.SetHeat(_heatLevel);
                }
                _session.SetSprinkler(keyboard.spaceKey.isPressed);
            }

            _elapsed += realDt;
            _session.Tick(realDt);
            RefreshSoup();
        }

        // ---------------- Fase 1: mezcla ----------------

        private void SetupMixPhase()
        {
            SetText(_titleText, Text("minigame.compost.title_mix"));
            SetText(_confirmLabel, Text("minigame.compost.confirm"));
            for (int i = 0; i < InputCount; i++)
            {
                Slider slider = i < _inputSliders.Length ? _inputSliders[i] : null;
                float available = Mathf.Max(0f, Bootstrap.Resources.Get(Inputs[i]));
                if (i < _inputLabels.Length)
                    SetText(_inputLabels[i], Bootstrap.Texts.Format("minigame.compost.input_label", Text(InputNameKeys[i]), available));
                if (slider == null) continue;
                slider.onValueChanged.RemoveAllListeners();
                slider.minValue = 0f;
                slider.maxValue = available;
                slider.SetValueWithoutNotify(0f);
                slider.interactable = available > 0f;
                int index = i;
                slider.onValueChanged.AddListener(value => OnSliderChanged(index, value));
            }
            RefreshMix();
        }

        private void OnSliderChanged(int index, float value)
        {
            if (_mix == null) return;
            _mix.SetAmount(Inputs[index], Mathf.Max(0f, value));
            RefreshMix();
        }

        private void RefreshMix()
        {
            if (_mix == null) return;
            float total = 0f;
            for (int i = 0; i < InputCount; i++)
            {
                float amount = _mix.GetAmount(Inputs[i]);
                total += amount;
                if (i < _inputAmountTexts.Length)
                    SetText(_inputAmountTexts[i], Bootstrap.Texts.Format("minigame.compost.amount", amount));
            }
            StepScore score = _mix.Evaluate();
            bool hasMix = total > 0f;
            SetText(_ratioText, hasMix
                ? Bootstrap.Texts.Format("minigame.compost.ratio", _mix.Ratio)
                : Text("minigame.compost.ratio_empty"));
            if (_ratioText != null) _ratioText.color = !hasMix ? Color.white : score.InRange ? Color.green : Color.red;
            SetText(_rangeText, Bootstrap.Texts.Format("minigame.compost.range",
                Bootstrap.CompostMixConfig.Min, Bootstrap.CompostMixConfig.Max, Bootstrap.CompostMixConfig.Guide));
            SetText(_totalText, Bootstrap.Texts.Format("minigame.compost.total", total));
            SetText(_mixMessageText, score.InRange ? Text("minigame.compost.mix_ok") : Text(score.ReasonKey));
        }

        private void OnConfirmClicked()
        {
            if (_mix == null) return;
            if (!_mix.TryConfirm())
            {
                SetText(_mixMessageText, Text(_mix.LastReasonKey));
                return;
            }
            // La mezcla ya se cobró del pool: de aquí en adelante comienza la Sopa.
            _mixScore = _mix.ConfirmedScore;
            StartSoup();
        }

        // ---------------- Fase 2: sopa ----------------

        private void StartSoup()
        {
            _session = new CompostSession(_config, Bootstrap.Resources);
            _session.OnFinished += HandleSessionFinished;
            _session.Start(_mixScore);
            _heatLevel = 0f;
            _elapsed = 0f;
            _hasResult = false;
            if (_resultGroup != null) _resultGroup.SetActive(false);

            SetText(_titleText, Text("minigame.compost.title_soup"));
            SetText(_heatKeysText, Text("minigame.compost.heat_keys"));
            SetText(_sprinklerKeysText, Text("minigame.compost.sprinkler_keys"));
            SetText(_continueLabel, Text("minigame.compost.continue"));
            PlaceZones();
            ShowPhase(false);
            RefreshSoup();
        }

        // Zonas verdes y marcas, calculadas desde CompostConfig (la escala de temperatura abarca ambiente a calor máximo).
        private void PlaceZones()
        {
            _tempMin = Mathf.Min(_config.AmbientTemp, _config.StartTemp);
            _tempMax = Mathf.Max(_config.MaxHeatTemp, _config.OverheatTemp);
            SetSpan(_tempGreenZone, TempFraction(_config.TempGreenMin), TempFraction(_config.TempGreenMax));
            SetMark(_tempOverheatMark, TempFraction(_config.OverheatTemp));
            SetSpan(_humidityGreenZone, HumidityFraction(_config.HumidityGreenMin), HumidityFraction(_config.HumidityGreenMax));
            SetMark(_humidityDryMark, HumidityFraction(_config.DryHumidity));
            SetMark(_humidityDrownMark, HumidityFraction(_config.DrownHumidity));
        }

        private float TempFraction(float value)
        {
            float span = _tempMax - _tempMin;
            return span > 0f ? Mathf.Clamp01((value - _tempMin) / span) : 0f;
        }

        // La lógica limita la humedad a 0..100 %.
        private static float HumidityFraction(float value) => Mathf.Clamp01(value / 100f);

        private static void SetSpan(RectTransform rect, float from, float to)
        {
            if (rect == null) return;
            rect.anchorMin = new Vector2(from, 0f);
            rect.anchorMax = new Vector2(to, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetMark(RectTransform rect, float at)
        {
            if (rect == null) return;
            rect.anchorMin = new Vector2(at, rect.anchorMin.y);
            rect.anchorMax = new Vector2(at, rect.anchorMax.y);
            rect.anchoredPosition = new Vector2(0f, rect.anchoredPosition.y);
        }

        private void RefreshSoup()
        {
            if (_session == null) return;
            SetMark(_tempNeedle, TempFraction(_session.Temperature));
            SetMark(_humidityNeedle, HumidityFraction(_session.Humidity));
            SetText(_tempText, Bootstrap.Texts.Format("minigame.compost.temp_value",
                _session.Temperature, _config.TempGreenMin, _config.TempGreenMax));
            SetText(_humidityText, Bootstrap.Texts.Format("minigame.compost.humidity_value",
                _session.Humidity, _config.HumidityGreenMin, _config.HumidityGreenMax));
            if (_tempText != null) _tempText.color = _session.IsTemperatureGreen ? Color.green : Color.white;
            if (_humidityText != null) _humidityText.color = _session.IsHumidityGreen ? Color.green : Color.white;

            if (_heatFill != null) _heatFill.fillAmount = _session.HeatLevel;
            SetText(_heatText, Bootstrap.Texts.Format("minigame.compost.heat_value", Mathf.RoundToInt(_session.HeatLevel * 100f)));
            if (_sprinklerLamp != null) _sprinklerLamp.color = _session.IsSprinklerOn ? _sprinklerOnColor : _sprinklerOffColor;
            SetText(_sprinklerText, Text(_session.IsSprinklerOn ? "minigame.compost.sprinkler_on" : "minigame.compost.sprinkler_off"));

            float cycle = _config.CycleSeconds;
            float inGreen = _session.TimeInGreen;
            if (_timeBar != null) _timeBar.fillAmount = Mathf.Clamp01(inGreen / cycle);
            SetText(_timeText, Bootstrap.Texts.Format("minigame.compost.time", inGreen, cycle, Mathf.Min(_elapsed, cycle)));
            SetText(_yieldText, Bootstrap.Texts.Format("minigame.compost.yield_preview",
                Mathf.RoundToInt(Mathf.Clamp01(inGreen / cycle) * _mixScore * 100f)));
            SetText(_warningText, Text(WarningKey()));
        }

        // Aviso corto de la IA; el primero que aplica gana (calor y ahogo son los más urgentes).
        private string WarningKey()
        {
            if (_session.IsFinished) return string.Empty;
            if (_session.Temperature > _config.TempGreenMax) return "minigame.compost.warn_hot";
            if (_session.IsDrowning) return "minigame.compost.warn_drown";
            if (_session.IsTooDry) return "minigame.compost.warn_dry";
            if (_session.Temperature < _config.TempGreenMin) return "minigame.compost.warn_cold";
            if (_session.Humidity < _config.HumidityGreenMin) return "minigame.compost.warn_humidity_low";
            if (_session.Humidity > _config.HumidityGreenMax) return "minigame.compost.warn_humidity_high";
            return "minigame.compost.warn_ok";
        }

        private void HandleSessionFinished(PreparationResult result)
        {
            _result = result;
            _hasResult = true;
            SetText(_resultText, result.Starts
                ? Bootstrap.Texts.Format("minigame.compost.result_ok", Mathf.RoundToInt(result.Yield * 100f))
                : Text(result.ReasonKey));
            if (_resultGroup != null) _resultGroup.SetActive(true);
        }

        private void OnContinueClicked()
        {
            if (!_hasResult) return;
            Finish(_result);
        }

        // ---------------- Utilidades ----------------

        private void ShowPhase(bool mix)
        {
            if (_mixPhase != null) _mixPhase.SetActive(mix);
            if (_soupPhase != null) _soupPhase.SetActive(!mix);
            if (!mix && _resultGroup != null) _resultGroup.SetActive(_hasResult);
        }

        private string Text(string key) => Bootstrap.Texts.Get(key);

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null) target.text = value;
        }
    }
}
