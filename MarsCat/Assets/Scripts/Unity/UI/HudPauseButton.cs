// Asistencia de IA: escrito con Claude Code (Anthropic).
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MarsCat.Unity.UI
{
    /// <summary>Botón de pausa del HUD. También alterna con Espacio si está activo.</summary>
    public class HudPauseButton : MonoBehaviour
    {
        [Tooltip("Puente con la lógica (el GameBootstrap de la escena).")]
        [SerializeField] private GameBootstrap _bootstrap;

        [Tooltip("Botón de pausa.")]
        [SerializeField] private Button _button;

        [Tooltip("Texto que muestra \"Pausa\" mientras el juego está en pausa (opcional).")]
        [SerializeField] private TMP_Text _label;

        [Tooltip("La tecla Espacio alterna la pausa.")]
        [SerializeField] private bool _spaceToggles = true;

        private bool _subscribed;

        private void OnEnable()
        {
            if (_button != null) _button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            if (_button != null) _button.onClick.RemoveListener(HandleClick);
        }

        private void Start()
        {
            if (_bootstrap == null)
            {
                Debug.LogWarning("HudPauseButton: falta asignar el GameBootstrap.", this);
                return;
            }
            _bootstrap.WhenReady(Connect);
        }

        private void Connect()
        {
            if (this == null || _subscribed) return;
            _bootstrap.PauseChanged += HandlePauseChanged;
            _subscribed = true;
            HandlePauseChanged(_bootstrap.IsPaused);
        }

        private void OnDestroy()
        {
            if (_subscribed && _bootstrap != null) _bootstrap.PauseChanged -= HandlePauseChanged;
            _subscribed = false;
        }

        private void Update()
        {
            if (!_spaceToggles || _bootstrap == null || !_bootstrap.IsReady) return;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
                _bootstrap.TogglePause();
        }

        private void HandleClick()
        {
            if (_bootstrap != null && _bootstrap.IsReady) _bootstrap.TogglePause();
        }

        private void HandlePauseChanged(bool paused)
        {
            if (_label != null)
                _label.text = paused ? _bootstrap.Texts.Get("ui.pause") : string.Empty;
        }
    }
}
