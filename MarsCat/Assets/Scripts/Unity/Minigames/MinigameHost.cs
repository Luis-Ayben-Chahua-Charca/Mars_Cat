// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using MarsCat.Logic.Preparation;
using MarsCat.Unity.UI;
using UnityEngine;

namespace MarsCat.Unity.Minigames
{
    /// <summary>
    /// Abre el minijuego de cada edificio cuando el panel de detalle pide "Preparar" (DetailPanel.PrepareRequested)
    /// y aplica su resultado con GameBootstrap.ApplyPreparation. Si la preparación falla, la IA dice el motivo.
    /// Un solo host en la escena; cada minijuego se registra en _entries con el id de su edificio.
    /// </summary>
    [DisallowMultipleComponent]
    public class MinigameHost : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            [Tooltip("Id del edificio: solar_panels, moxie, water_extractor, plant_farm, rat_farm u organic_recycler.")]
            public string BuildingId;

            [Tooltip("Minijuego de ese edificio.")]
            public MinigameView View;
        }

        [SerializeField] private GameBootstrap _bootstrap;
        [SerializeField] private DetailPanel _detailPanel;

        [Tooltip("Opcional: la IA dice el motivo cuando una preparación falla.")]
        [SerializeField] private AiCorner _aiCorner;

        [Tooltip("Pausa la simulación mientras hay un minijuego abierto (el minijuego avanza en tiempo real igual).")]
        [SerializeField] private bool _pauseWhileOpen = true;

        [Tooltip("Si un edificio no tiene minijuego registrado, se prepara con rendimiento 1 (para no trabar el tutorial).")]
        [SerializeField] private bool _fallbackAutoPrepare = true;

        [SerializeField] private Entry[] _entries = new Entry[0];

        private MinigameView _current;
        private bool _pausedByHost;

        public MinigameView Current => _current;

        /// <summary>Se abrió un minijuego (buildingId).</summary>
        public event Action<string> Opened;

        /// <summary>Se aplicó un resultado (buildingId, resultado).</summary>
        public event Action<string, PreparationResult> ResultApplied;

        private void OnEnable()
        {
            if (_detailPanel != null) _detailPanel.PrepareRequested += Open;
            foreach (Entry entry in _entries)
            {
                if (entry?.View == null) continue;
                entry.View.Finished += HandleFinished;
                entry.View.Closed += HandleClosed;
            }
        }

        private void OnDisable()
        {
            if (_detailPanel != null) _detailPanel.PrepareRequested -= Open;
            foreach (Entry entry in _entries)
            {
                if (entry?.View == null) continue;
                entry.View.Finished -= HandleFinished;
                entry.View.Closed -= HandleClosed;
            }
        }

        /// <summary>Abre el minijuego del edificio. Si no hay uno registrado, avisa y no hace nada.</summary>
        public void Open(string buildingId)
        {
            if (_bootstrap == null || !_bootstrap.IsReady) return;
            MinigameView view = Find(buildingId);
            if (view == null)
            {
                if (_fallbackAutoPrepare)
                {
                    // Mientras falte el minijuego de ese edificio, se prepara con rendimiento 1 para no trabar el tutorial.
                    _bootstrap.ApplyPreparation(buildingId, PreparationResult.Ok(1f));
                    Debug.Log($"MinigameHost: '{buildingId}' no tiene minijuego registrado; se preparó con rendimiento 1.", this);
                }
                else
                {
                    Debug.LogWarning($"MinigameHost: no hay minijuego registrado para '{buildingId}'.", this);
                }
                return;
            }
            if (_current != null && _current != view) _current.Close();
            _current = view;
            if (_pauseWhileOpen && view.PausesGame && !_bootstrap.IsPaused)
            {
                _bootstrap.SetPaused(true);
                _pausedByHost = true;
            }
            view.Open(_bootstrap, buildingId);
            Opened?.Invoke(buildingId);
        }

        private MinigameView Find(string buildingId)
        {
            foreach (Entry entry in _entries)
                if (entry != null && entry.View != null && entry.BuildingId == buildingId) return entry.View;
            return null;
        }

        private void HandleFinished(MinigameView view, PreparationResult result)
        {
            bool applied = _bootstrap.ApplyPreparation(view.BuildingId, result);
            if (applied) ResultApplied?.Invoke(view.BuildingId, result);
            if (!result.Starts && _aiCorner != null && !string.IsNullOrEmpty(result.ReasonKey))
                _aiCorner.Say(result.ReasonKey);
        }

        private void HandleClosed(MinigameView view)
        {
            if (view != _current) return;
            _current = null;
            if (_pausedByHost)
            {
                _pausedByHost = false;
                _bootstrap.SetPaused(false);
            }
        }
    }
}
