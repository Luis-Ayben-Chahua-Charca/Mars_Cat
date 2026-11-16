// Asistencia de IA: escrito con Claude Code (Anthropic).
using MarsCat.Logic.Clock;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsCat.Unity.UI
{
    /// <summary>Reloj semicircular del HUD (§4): sol actual, día/noche y avance de la fase.</summary>
    public class HudClock : MonoBehaviour
    {
        [Tooltip("Puente con la lógica (el GameBootstrap de la escena).")]
        [SerializeField] private GameBootstrap _bootstrap;

        [Tooltip("Texto \"Sol N\".")]
        [SerializeField] private TMP_Text _solLabel;

        [Tooltip("Texto \"Día\" / \"Noche\" (opcional).")]
        [SerializeField] private TMP_Text _phaseLabel;

        [Tooltip("Aguja que gira de -90° a +90° durante la fase actual (opcional).")]
        [SerializeField] private RectTransform _needle;

        [Tooltip("Imagen tipo Filled que se llena con el avance de la fase actual (opcional).")]
        [SerializeField] private Image _dayNightFill;

        [Tooltip("Color del relleno de día.")]
        [SerializeField] private Color _dayColor = new Color(1f, 0.8f, 0.3f, 1f);

        [Tooltip("Color del relleno de noche.")]
        [SerializeField] private Color _nightColor = new Color(0.3f, 0.4f, 0.8f, 1f);

        private DayNightClock _clock;
        private int _shownSol = -1;
        private int _shownNight = -1;
        private float _dayFraction = 0.5f; // parte del sol (en tiempo de juego) que ocupa el día

        private void Start()
        {
            if (_bootstrap == null)
            {
                Debug.LogWarning("HudClock: falta asignar el GameBootstrap.", this);
                enabled = false;
                return;
            }
            _bootstrap.WhenReady(Connect);
        }

        private void Connect()
        {
            if (this == null) return;
            _clock = _bootstrap.Clock;
            DayNightConfig config = DayNightConfig.FromBalance(_bootstrap.Balance);
            float total = config.DaySeconds + config.NightSeconds * config.NightTimeScale;
            if (total > 0f) _dayFraction = Mathf.Clamp(config.DaySeconds / total, 0.01f, 0.99f);
        }

        private void Update()
        {
            if (_clock == null) return;

            int sol = _clock.CurrentSol;
            bool night = _clock.IsNight;
            int nightFlag = night ? 1 : 0;

            if (sol != _shownSol && _solLabel != null)
                _solLabel.text = _bootstrap.Texts.Format("ui.clock.sol", sol);
            if (nightFlag != _shownNight)
            {
                if (_phaseLabel != null)
                    _phaseLabel.text = _bootstrap.Texts.Get(night ? "ui.clock.night" : "ui.clock.day");
                if (_dayNightFill != null)
                    _dayNightFill.color = night ? _nightColor : _dayColor;
            }
            _shownSol = sol;
            _shownNight = nightFlag;

            // DayProgress cubre el sol entero (día + noche); se convierte en avance de la fase actual.
            float progress = Mathf.Clamp01(_clock.DayProgress);
            float phase = night
                ? Mathf.Clamp01((progress - _dayFraction) / (1f - _dayFraction))
                : Mathf.Clamp01(progress / _dayFraction);

            if (_needle != null)
                _needle.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(90f, -90f, phase));
            if (_dayNightFill != null)
                _dayNightFill.fillAmount = phase;
        }
    }
}
