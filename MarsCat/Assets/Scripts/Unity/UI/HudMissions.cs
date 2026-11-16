// Asistencia de IA: escrito con Claude Code (Anthropic).
using MarsCat.Logic.Tutorial;
using TMPro;
using UnityEngine;

namespace MarsCat.Unity.UI
{
    /// <summary>Tablero de misiones del HUD (§5): muestra la misión del paso actual del tutorial.</summary>
    public class HudMissions : MonoBehaviour
    {
        private const string DoneKey = "tutorial.done.mission";

        [Tooltip("Puente con la lógica (el GameBootstrap de la escena).")]
        [SerializeField] private GameBootstrap _bootstrap;

        [Tooltip("Título del tablero (\"Misiones\").")]
        [SerializeField] private TMP_Text _title;

        [Tooltip("Texto de la misión actual.")]
        [SerializeField] private TMP_Text _currentMission;

        private TutorialSequence _tutorial;
        private bool _subscribed;

        private void Start()
        {
            if (_bootstrap == null)
            {
                Debug.LogWarning("HudMissions: falta asignar el GameBootstrap.", this);
                return;
            }
            _bootstrap.WhenReady(Connect);
        }

        private void Connect()
        {
            if (this == null || _subscribed) return;
            _tutorial = _bootstrap.Tutorial;
            if (_tutorial == null) return;
            _tutorial.OnStepStarted += HandleStepStarted;
            _subscribed = true;
            if (_title != null) _title.text = _bootstrap.Texts.Get("ui.missions.title");
            Refresh();
        }

        private void OnDestroy()
        {
            if (_subscribed && _tutorial != null) _tutorial.OnStepStarted -= HandleStepStarted;
            _subscribed = false;
        }

        private void HandleStepStarted(TutorialStep step) => Refresh();

        private void Refresh()
        {
            if (_currentMission == null || _tutorial == null) return;
            string key = _tutorial.CurrentStep == TutorialStep.Done ? DoneKey : _tutorial.MissionKey;
            _currentMission.text = _bootstrap.Texts.Get(key);
        }
    }
}
