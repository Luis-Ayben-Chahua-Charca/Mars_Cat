// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using MarsCat.Logic.Preparation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarsCat.Unity.Minigames
{
    /// <summary>
    /// Base de todos los minijuegos de preparación (Lote 3). Cada minijuego es un panel de UI que dibuja una sesión
    /// de MarsCat.Logic y le pasa la entrada del jugador. Lo abre MinigameHost cuando el panel de detalle pide
    /// "Preparar"; al terminar, el minijuego llama a Finish(resultado) y el host lo aplica al edificio.
    /// Esc cierra el minijuego sin resultado (salvo que la subclase lo impida).
    /// </summary>
    public abstract class MinigameView : MonoBehaviour
    {
        [Tooltip("Panel visible del minijuego (hijo de este objeto). Se activa al abrir y se oculta al cerrar.")]
        [SerializeField] private GameObject _root;

        [Tooltip("Esc cierra el minijuego.")]
        [SerializeField] private bool _closeOnEscape = true;

        /// <summary>Edificio que se está preparando.</summary>
        public string BuildingId { get; private set; }

        public bool IsOpen { get; private set; }

        protected GameBootstrap Bootstrap { get; private set; }

        /// <summary>
        /// true (por defecto): el host pausa la simulación mientras está abierto. La Redada devuelve false porque
        /// necesita que los gatos se muevan mientras el jugador marca ratas.
        /// </summary>
        public virtual bool PausesGame => true;

        /// <summary>El minijuego terminó con un resultado para el edificio.</summary>
        public event Action<MinigameView, PreparationResult> Finished;

        /// <summary>El minijuego se cerró (con o sin resultado).</summary>
        public event Action<MinigameView> Closed;

        protected virtual void Awake()
        {
            if (_root != null && !IsOpen) _root.SetActive(false);
        }

        public void Open(GameBootstrap bootstrap, string buildingId)
        {
            if (bootstrap == null) throw new ArgumentNullException(nameof(bootstrap));
            if (IsOpen) Close();
            Bootstrap = bootstrap;
            BuildingId = buildingId;
            IsOpen = true;
            if (_root != null) _root.SetActive(true);
            OnOpened();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            OnClosed();
            if (_root != null) _root.SetActive(false);
            Closed?.Invoke(this);
        }

        /// <summary>Prepara la sesión y la UI. Bootstrap y BuildingId ya están asignados.</summary>
        protected abstract void OnOpened();

        /// <summary>Limpieza al cerrar (desuscribir eventos de la sesión, etc.).</summary>
        protected virtual void OnClosed() { }

        /// <summary>Avance por frame mientras está abierto (segundos reales, también en pausa).</summary>
        protected virtual void Tick(float realDt) { }

        /// <summary>Entrega el resultado al host. Por defecto cierra el minijuego después.</summary>
        protected void Finish(PreparationResult result, bool close = true)
        {
            Finished?.Invoke(this, result);
            if (close) Close();
        }

        protected virtual void Update()
        {
            if (!IsOpen) return;
            Keyboard keyboard = Keyboard.current;
            if (_closeOnEscape && keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Close();
                return;
            }
            Tick(Time.unscaledDeltaTime);
        }

        protected virtual void OnDisable()
        {
            if (IsOpen) Close();
        }
    }
}
