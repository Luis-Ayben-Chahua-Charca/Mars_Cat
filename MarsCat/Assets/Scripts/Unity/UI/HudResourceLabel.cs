// Asistencia de IA: escrito con Claude Code (Anthropic).
using System.Globalization;
using MarsCat.Logic.Resources;
using TMPro;
using UnityEngine;

namespace MarsCat.Unity.UI
{
    /// <summary>
    /// Muestra la cantidad de un recurso del ResourcePool (comida, agua, metal, escombros, abono...).
    /// Un componente por recurso.
    /// </summary>
    public class HudResourceLabel : MonoBehaviour
    {
        [Tooltip("Puente con la lógica (el GameBootstrap de la escena).")]
        [SerializeField] private GameBootstrap _bootstrap;

        [Tooltip("Recurso que muestra este rótulo.")]
        [SerializeField] private ResourceId _resource;

        [Tooltip("Texto donde se escribe la cantidad.")]
        [SerializeField] private TMP_Text _value;

        [Tooltip("Mostrar también la capacidad (\"12 / 20\").")]
        [SerializeField] private bool _showCapacity;

        private ResourcePool _pool;
        private bool _subscribed;

        private void Start()
        {
            if (_bootstrap == null)
            {
                Debug.LogWarning("HudResourceLabel: falta asignar el GameBootstrap.", this);
                return;
            }
            _bootstrap.WhenReady(Connect);
        }

        private void Connect()
        {
            if (this == null || _subscribed) return;
            _pool = _bootstrap.Resources;
            if (_pool == null) return;
            _pool.OnChanged += HandleChanged;
            _subscribed = true;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_subscribed && _pool != null) _pool.OnChanged -= HandleChanged;
            _subscribed = false;
        }

        private void HandleChanged(ResourceId id, float amount)
        {
            if (id == _resource) Refresh();
        }

        private void Refresh()
        {
            if (_value == null || _pool == null) return;
            string text = Format(_pool.Get(_resource));
            if (_showCapacity && !_pool.IsInfinite(_resource))
                text += " / " + Format(_pool.Capacity(_resource));
            _value.text = text;
        }

        // Enteros desde 10; un decimal por debajo.
        private static string Format(float amount)
        {
            return amount >= 10f
                ? Mathf.FloorToInt(amount).ToString(CultureInfo.InvariantCulture)
                : amount.ToString("0.0", CultureInfo.InvariantCulture);
        }
    }
}
