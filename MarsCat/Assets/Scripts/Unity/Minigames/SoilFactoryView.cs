// Asistencia de IA: escrito con Claude Code (Anthropic).
using System.Collections.Generic;
using System.Globalization;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsCat.Unity.Minigames
{
    /// <summary>
    /// Minijuego "Fábrica de Suelo" (granja de plantas, plant_farm): lavar el regolito (litros de agua) y mezclar
    /// (abono y bacterias). Adaptador: crea WashStep, MixStep y MultiStepPreparation con las configs del GameBootstrap,
    /// les pasa los sliders y dibuja lo que dicen Evaluate / YieldFor. No recalcula reglas.
    /// </summary>
    public class SoilFactoryView : MinigameView
    {
        [Header("Lavar")]
        [SerializeField] private TMP_Text _washHeader;
        [SerializeField] private TMP_Text _waterLabel;
        [SerializeField] private Slider _waterSlider;
        [SerializeField] private TMP_Text _waterValueText;
        [Tooltip("Zona verde (rango aceptado) sobre la barra del agua. Se coloca con anclas normalizadas.")]
        [SerializeField] private RectTransform _waterZone;
        [Tooltip("Marca del valor guía de la IA sobre la barra del agua.")]
        [SerializeField] private RectTransform _waterGuideMark;
        [SerializeField] private TMP_Text _waterGuideText;
        [SerializeField] private TMP_Text _perchlorateLabel;
        [SerializeField] private Image _perchlorateFill;
        [Tooltip("Marca del umbral de perclorato sobre su medidor.")]
        [SerializeField] private RectTransform _perchlorateThresholdMark;
        [SerializeField] private TMP_Text _perchlorateText;
        [SerializeField] private TMP_Text _washStatusText;

        [Header("Mezclar")]
        [SerializeField] private TMP_Text _mixHeader;
        [SerializeField] private TMP_Text _compostLabel;
        [SerializeField] private Slider _compostSlider;
        [SerializeField] private TMP_Text _compostValueText;
        [SerializeField] private RectTransform _compostZone;
        [SerializeField] private RectTransform _compostGuideMark;
        [SerializeField] private TMP_Text _compostGuideText;
        [SerializeField] private TMP_Text _compostStatusText;
        [SerializeField] private TMP_Text _bacteriaLabel;
        [SerializeField] private Slider _bacteriaSlider;
        [SerializeField] private TMP_Text _bacteriaValueText;
        [SerializeField] private RectTransform _bacteriaZone;
        [SerializeField] private RectTransform _bacteriaGuideMark;
        [SerializeField] private TMP_Text _bacteriaGuideText;
        [SerializeField] private TMP_Text _bacteriaStatusText;

        [Header("Resumen")]
        [SerializeField] private TMP_Text _yieldText;
        [SerializeField] private TMP_Text _costText;
        [Tooltip("Aviso del último intento de preparar (motivo del fallo) o pista de mejora.")]
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private TMP_Text _confirmLabel;

        [Header("Ajustes")]
        [Tooltip("Margen visual de la barra del agua a cada lado del rango, como fracción del ancho del rango.")]
        [SerializeField] private float _waterMarginFraction = 0.5f;
        [Tooltip("Los sliders de abono y bacterias llegan hasta el máximo aceptado por este factor (para poder pasarse).")]
        [SerializeField] private float _overshootFactor = 1.25f;
        [Tooltip("Paso del slider de agua, en litros.")]
        [SerializeField] private float _waterStep = 0.05f;
        [Tooltip("Paso de los sliders de abono y bacterias, en puntos porcentuales.")]
        [SerializeField] private float _percentStep = 0.5f;
        [SerializeField] private Color _okColor = new Color(0.3f, 0.9f, 0.4f, 1f);
        [SerializeField] private Color _badColor = new Color(0.95f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color _neutralColor = new Color32(0xF2, 0xEC, 0xFF, 0xFF);

        private WashStep _wash;
        private MixStep _mix;
        private MultiStepPreparation _preparation;
        private GuideRange _compostRange;
        private GuideRange _bacteriaRange;
        private Image _waterFillImage;
        private Image _compostFillImage;
        private Image _bacteriaFillImage;
        private string _lastMessageKey = string.Empty;
        private bool _listening;

        protected override void OnOpened()
        {
            GameBootstrap bootstrap = Bootstrap;
            if (bootstrap == null || bootstrap.SoilWashConfig == null || bootstrap.SoilMixConfig == null ||
                bootstrap.SoilYieldConfig == null || bootstrap.Resources == null || bootstrap.Texts == null)
            {
                Debug.LogWarning("SoilFactoryView: faltan las configs de suelo, los recursos o los textos en el GameBootstrap.");
                Close();
                return;
            }

            WashConfig wash = bootstrap.SoilWashConfig;
            MixConfig mix = bootstrap.SoilMixConfig;
            _wash = new WashStep(wash, bootstrap.Resources);
            _mix = new MixStep(mix, bootstrap.Resources);
            _preparation = new MultiStepPreparation(new List<IPreparationStep> { _wash, _mix }, bootstrap.SoilYieldConfig);
            // Mismos rangos que usa MixStep, solo para colorear y explicar cada slider por separado.
            _compostRange = new GuideRange(MixStep.StepId + ".compost", mix.CompostGuide, mix.CompostMin, mix.CompostMax, mix.Plateau);
            _bacteriaRange = new GuideRange(MixStep.StepId + ".bacteria", mix.BacteriaGuide, mix.BacteriaMin, mix.BacteriaMax, mix.Plateau);

            _waterFillImage = FillOf(_waterSlider);
            _compostFillImage = FillOf(_compostSlider);
            _bacteriaFillImage = FillOf(_bacteriaSlider);

            ApplyStaticTexts(wash, mix);
            ConfigureWaterSlider(wash);
            ConfigurePercentSlider(_compostSlider, _compostZone, _compostGuideMark, mix.CompostMin, mix.CompostGuide, mix.CompostMax);
            ConfigurePercentSlider(_bacteriaSlider, _bacteriaZone, _bacteriaGuideMark, mix.BacteriaMin, mix.BacteriaGuide, mix.BacteriaMax);
            PlaceMark(_perchlorateThresholdMark, wash.InitialPercent > 0f ? wash.ThresholdPercent / wash.InitialPercent : 0f);

            if (_waterSlider != null) _waterSlider.onValueChanged.AddListener(OnSliderChanged);
            if (_compostSlider != null) _compostSlider.onValueChanged.AddListener(OnSliderChanged);
            if (_bacteriaSlider != null) _bacteriaSlider.onValueChanged.AddListener(OnSliderChanged);
            if (_confirmButton != null) _confirmButton.onClick.AddListener(OnConfirm);
            _listening = true;

            _lastMessageKey = string.Empty;
            ApplyInputs();
            Refresh();
        }

        protected override void OnClosed()
        {
            if (_listening)
            {
                if (_waterSlider != null) _waterSlider.onValueChanged.RemoveListener(OnSliderChanged);
                if (_compostSlider != null) _compostSlider.onValueChanged.RemoveListener(OnSliderChanged);
                if (_bacteriaSlider != null) _bacteriaSlider.onValueChanged.RemoveListener(OnSliderChanged);
                if (_confirmButton != null) _confirmButton.onClick.RemoveListener(OnConfirm);
                _listening = false;
            }
            _wash = null;
            _mix = null;
            _preparation = null;
        }

        // ---------------- Configuración de la UI ----------------

        private void ApplyStaticTexts(WashConfig wash, MixConfig mix)
        {
            SetText(_washHeader, T("minigame.soil.wash.header"));
            SetText(_mixHeader, T("minigame.soil.mix.header"));
            SetText(_waterLabel, T("minigame.soil.water.label"));
            SetText(_perchlorateLabel, T("minigame.soil.perchlorate.label"));
            SetText(_compostLabel, T("minigame.soil.compost.label"));
            SetText(_bacteriaLabel, T("minigame.soil.bacteria.label"));
            SetText(_confirmLabel, T("minigame.soil.confirm"));
            SetText(_waterGuideText, Guide(Liters(wash.GuideLiters), Liters(wash.MinLiters), Liters(wash.MaxLiters)));
            SetText(_compostGuideText, Guide(Percent(mix.CompostGuide), Percent(mix.CompostMin), Percent(mix.CompostMax)));
            SetText(_bacteriaGuideText, Guide(Percent(mix.BacteriaGuide), Percent(mix.BacteriaMin), Percent(mix.BacteriaMax)));
        }

        private void ConfigureWaterSlider(WashConfig wash)
        {
            if (_waterSlider == null) return;
            float span = wash.MaxLiters - wash.MinLiters;
            float margin = (span > 0f ? span : Mathf.Max(wash.GuideLiters, 1f)) * _waterMarginFraction;
            float sliderMin = Mathf.Max(0f, wash.MinLiters - margin);
            float sliderMax = wash.MaxLiters + margin;
            _waterSlider.wholeNumbers = false;
            _waterSlider.minValue = sliderMin;
            _waterSlider.maxValue = sliderMax;
            // Empieza en el extremo bajo: el jugador tiene que lavar.
            _waterSlider.SetValueWithoutNotify(sliderMin);
            PlaceZone(_waterZone, Normalize(wash.MinLiters, sliderMin, sliderMax), Normalize(wash.MaxLiters, sliderMin, sliderMax));
            PlaceMark(_waterGuideMark, Normalize(wash.GuideLiters, sliderMin, sliderMax));
        }

        private void ConfigurePercentSlider(Slider slider, RectTransform zone, RectTransform guideMark,
            float min, float guide, float max)
        {
            if (slider == null) return;
            float sliderMax = max * Mathf.Max(1f, _overshootFactor);
            slider.wholeNumbers = false;
            slider.minValue = 0f;
            slider.maxValue = sliderMax;
            slider.SetValueWithoutNotify(0f);
            PlaceZone(zone, Normalize(min, 0f, sliderMax), Normalize(max, 0f, sliderMax));
            PlaceMark(guideMark, Normalize(guide, 0f, sliderMax));
        }

        // ---------------- Entrada ----------------

        private void OnSliderChanged(float value)
        {
            _lastMessageKey = string.Empty;
            ApplyInputs();
            Refresh();
        }

        // Pasa los valores de los sliders (con su paso) a los pasos de la lógica.
        private void ApplyInputs()
        {
            if (_wash == null || _mix == null) return;
            if (_waterSlider != null)
            {
                float liters = Snap(_waterSlider.value, _waterStep);
                _waterSlider.SetValueWithoutNotify(liters);
                _wash.SetWater(liters);
            }
            if (_compostSlider != null)
            {
                float percent = Snap(_compostSlider.value, _percentStep);
                _compostSlider.SetValueWithoutNotify(percent);
                _mix.SetCompost(percent);
            }
            if (_bacteriaSlider != null)
            {
                float dose = Snap(_bacteriaSlider.value, _percentStep);
                _bacteriaSlider.SetValueWithoutNotify(dose);
                _mix.SetBacteria(dose);
            }
        }

        private void OnConfirm()
        {
            if (_preparation == null) return;
            PreparationResult result = _preparation.Confirm();
            if (result.Starts)
            {
                Finish(result);
                return;
            }
            _lastMessageKey = result.ReasonKey;
            Refresh();
        }

        // ---------------- Dibujo ----------------

        private void Refresh()
        {
            if (_wash == null || _mix == null || _preparation == null) return;
            WashConfig wash = Bootstrap.SoilWashConfig;
            StepScore washScore = _wash.Evaluate();
            Color washColor = washScore.InRange ? _okColor : _badColor;

            // Lavar
            SetText(_waterValueText, Liters(_wash.Liters), washColor);
            SetGraphicColor(_waterFillImage, washColor);
            SetText(_perchlorateText, Bootstrap.Texts.Format("minigame.soil.perchlorate.value",
                Number(_wash.PerchloratePercent, "0.###"), Number(wash.ThresholdPercent, "0.###")), washColor);
            if (_perchlorateFill != null)
            {
                _perchlorateFill.fillAmount = wash.InitialPercent > 0f
                    ? Mathf.Clamp01(_wash.PerchloratePercent / wash.InitialPercent) : 0f;
                _perchlorateFill.color = washColor;
            }
            SetText(_washStatusText, washScore.InRange ? T("minigame.soil.ok") : T(washScore.ReasonKey), washColor);

            // Mezclar
            StepScore compostScore = _compostRange.Evaluate(_mix.CompostPercent);
            StepScore bacteriaScore = _bacteriaRange.Evaluate(_mix.BacteriaDose);
            Color compostColor = compostScore.InRange ? _okColor : _badColor;
            Color bacteriaColor = bacteriaScore.InRange ? _okColor : _badColor;
            SetText(_compostValueText, Percent(_mix.CompostPercent), compostColor);
            SetGraphicColor(_compostFillImage, compostColor);
            SetText(_compostStatusText, compostScore.InRange ? T("minigame.soil.ok") : BurnOrReason(compostScore), compostColor);
            SetText(_bacteriaValueText, Percent(_mix.BacteriaDose), bacteriaColor);
            SetGraphicColor(_bacteriaFillImage, bacteriaColor);
            SetText(_bacteriaStatusText, bacteriaScore.InRange ? T("minigame.soil.ok") : T(bacteriaScore.ReasonKey), bacteriaColor);

            // Resumen
            bool allInRange = washScore.InRange && compostScore.InRange && bacteriaScore.InRange;
            if (allInRange)
                SetText(_yieldText, Bootstrap.Texts.Format("minigame.soil.yield",
                    Number(_preparation.YieldFor(_preparation.Score) * 100f, "0")), _okColor);
            else
                SetText(_yieldText, T("minigame.soil.yield.none"), _badColor);

            ResourcePool pool = Bootstrap.Resources;
            bool canPay = _wash.CanCharge() && _mix.CanCharge();
            SetText(_costText, Bootstrap.Texts.Format("minigame.soil.cost",
                Number(_wash.Liters, "0.##"), Number(pool.Get(ResourceId.Water), "0.#"),
                Number(_mix.CompostKg, "0.##"), Number(pool.Get(ResourceId.Compost), "0.#")),
                canPay ? _neutralColor : _badColor);

            if (!string.IsNullOrEmpty(_lastMessageKey))
                SetText(_messageText, T(_lastMessageKey), _badColor);
            else if (allInRange && !string.IsNullOrEmpty(_preparation.ImproveKey))
                SetText(_messageText, T(_preparation.ImproveKey), _neutralColor);
            else
                SetText(_messageText, string.Empty);
        }

        private string BurnOrReason(StepScore score)
        {
            return _mix.CompostPercent > Bootstrap.SoilMixConfig.CompostMax ? T("minigame.soil.burn") : T(score.ReasonKey);
        }

        // ---------------- Utilidades ----------------

        private string T(string key) => Bootstrap != null && Bootstrap.Texts != null ? Bootstrap.Texts.Get(key) : key;

        private string Guide(string guide, string min, string max) =>
            Bootstrap.Texts.Format("minigame.soil.guide", guide, min, max);

        private string Liters(float value) => Bootstrap.Texts.Format("minigame.soil.liters", Number(value, "0.##"));

        private string Percent(float value) => Bootstrap.Texts.Format("minigame.soil.percent", Number(value, "0.#"));

        private static string Number(float value, string format) => value.ToString(format, CultureInfo.InvariantCulture);

        private static float Snap(float value, float step) => step > 0f ? Mathf.Round(value / step) * step : value;

        private static float Normalize(float value, float min, float max) =>
            max > min ? Mathf.Clamp01((value - min) / (max - min)) : 0f;

        private static Image FillOf(Slider slider) =>
            slider != null && slider.fillRect != null ? slider.fillRect.GetComponent<Image>() : null;

        private static void PlaceMark(RectTransform mark, float normalized)
        {
            if (mark == null) return;
            mark.anchorMin = new Vector2(normalized, 0f);
            mark.anchorMax = new Vector2(normalized, 1f);
            mark.anchoredPosition = new Vector2(0f, mark.anchoredPosition.y);
        }

        private static void PlaceZone(RectTransform zone, float from, float to)
        {
            if (zone == null) return;
            zone.anchorMin = new Vector2(from, 0f);
            zone.anchorMax = new Vector2(to, 1f);
            zone.offsetMin = Vector2.zero;
            zone.offsetMax = Vector2.zero;
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null) text.text = value;
        }

        private static void SetText(TMP_Text text, string value, Color color)
        {
            if (text == null) return;
            text.text = value;
            text.color = color;
        }

        private static void SetGraphicColor(Graphic graphic, Color color)
        {
            if (graphic != null) graphic.color = color;
        }
    }
}
