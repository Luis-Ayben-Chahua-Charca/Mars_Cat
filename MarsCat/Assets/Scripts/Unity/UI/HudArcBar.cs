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
            if (_source == Source.Electricity)
            {
                if (_bootstrap == null)
                {
                    Debug.LogWarning("HudArcBar: falta asignar el GameBootstrap.", this);
                    enabled = false;
                    return;
                }
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

            // #55: conectar BaseAtmosphere.OxygenFraction cuando exista
            return _hasExternalValue ? _external : _previewValue;
        }

        private void Apply()
        {
            if (_fill != null)
            {
                _fill.fillAmount = _shown;
                if (_colors != null) _fill.color = _colors.Evaluate(_shown);
            }
            if (_label != null)
                _label.text = Mathf.RoundToInt(_shown * 100f) + "%";
        }
    }
}
