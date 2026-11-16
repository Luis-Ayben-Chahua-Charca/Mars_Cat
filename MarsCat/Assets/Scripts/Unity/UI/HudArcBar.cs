// Asistencia de IA: escrito con Claude Code (Anthropic).
using MarsCat.Logic.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsCat.Unity.UI
{
    /// <summary>Arco de 1/4 de círculo del HUD (§7 oxígeno, §8 energía).</summary>
    public class HudArcBar : MonoBehaviour
    {
        public enum Source
        {
            Electricity,
            Oxygen
        }

        [Tooltip("Puente con la lógica (el GameBootstrap de la escena). Necesario para Electricity.")]
        [SerializeField] private GameBootstrap _bootstrap;

        [Tooltip("De dónde sale el valor: la red eléctrica o el oxígeno del aire.")]
        [SerializeField] private Source _source = Source.Electricity;

        [Tooltip("Imagen tipo Filled, Radial 90: el script controla su fillAmount.")]
        [SerializeField] private Image _fill;

        [Tooltip("Texto con el porcentaje (opcional).")]
        [SerializeField] private TMP_Text _label;

        [Tooltip("Color del arco según la fracción (0 vacío, 1 lleno). Opcional.")]
        [SerializeField] private Gradient _colors;

        [Tooltip("Suavizado: segundos aproximados en recorrer el arco entero. 0 = sin suavizar.")]
        [Min(0f)]
        [SerializeField] private float _smoothing = 0.25f;

        [Tooltip("Fracción mostrada en Oxygen mientras nadie llame a SetValue (para capturas).")]
        [Range(0f, 1f)]
        [SerializeField] private float _previewValue = 1f;

        private float _external;
        private float _shown;
        private bool _hasExternalValue;
        private bool _ready;

        /// <summary>Fija la fracción (0..1). Para Oxygen la llama quien conozca el aire de la base.</summary>
        public void SetValue(float fraction01)
        {
            _external = Mathf.Clamp01(fraction01);
            _hasExternalValue = true;
        }

        private void Start()
        {
            if (_bootstrap == null)
            {
                // Sin bootstrap, el oxígeno muestra _previewValue (útil en escenas de prueba); la energía no puede.
                if (_source == Source.Electricity)
                {
                    Debug.LogWarning("HudArcBar: falta asignar el GameBootstrap.", this);
                    enabled = false;
                    return;
                }
            }
            else
            {
                _bootstrap.WhenReady(() => _ready = true);
            }
            _shown = CurrentTarget();
            Apply();
        }

        private void Update()
        {
            float target = CurrentTarget();
            _shown = _smoothing > 0f
                ? Mathf.MoveTowards(_shown, target, Time.unscaledDeltaTime / _smoothing)
                : target;
            Apply();
        }

        private float CurrentTarget()
        {
            if (_source == Source.Electricity)
            {
                if (!_ready || _bootstrap.Resources == null) return 0f;
                ResourcePool pool = _bootstrap.Resources;
                float capacity = pool.Capacity(ResourceId.Electricity);
                if (capacity <= 0f) return 0f;
                return Mathf.Clamp01(pool.Get(ResourceId.Electricity) / capacity);
            }

            if (_hasExternalValue) return _external;
            // #55: el arco va lleno con el % inicial de O₂ (0,21) y vacío en 0.
            if (_ready && _bootstrap.Atmosphere != null && _bootstrap.AtmosphereConfig.InitialFraction > 0f)
                return Mathf.Clamp01(_bootstrap.Atmosphere.OxygenFraction / _bootstrap.AtmosphereConfig.InitialFraction);
            return _previewValue;
        }

        // Etiqueta: para el oxígeno muestra el % real de O₂ en el aire (21,0 %); para la energía, el % de la batería.
        private string LabelText()
        {
            if (_source == Source.Oxygen && !_hasExternalValue && _ready && _bootstrap.Atmosphere != null)
                return (_bootstrap.Atmosphere.OxygenFraction * 100f).ToString("0.0") + "%";
            return Mathf.RoundToInt(_shown * 100f) + "%";
        }

        private void Apply()
        {
            if (_fill != null)
            {
                _fill.fillAmount = _shown;
                if (_colors != null) _fill.color = _colors.Evaluate(_shown);
            }
            if (_label != null)
                _label.text = LabelText();
        }
    }
}
