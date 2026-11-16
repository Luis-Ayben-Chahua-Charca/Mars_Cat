using MarsCat.Logic.Tutorial;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsCat.Unity.UI
{
    /// <summary>Esquina de la IA de la base (§1): dice la misión de cada paso con efecto de máquina de escribir.</summary>
    public class AiCorner : MonoBehaviour
    {
        private const string DoneKey = "tutorial.done.mission";

        [Tooltip("Puente con la lógica (el GameBootstrap de la escena).")]
        [SerializeField] private GameBootstrap _bootstrap;

        [Tooltip("Retrato de la IA (opcional).")]
        [SerializeField] private Image _portrait;

        [Tooltip("Nombre de la IA (clave ui.ai.name).")]
        [SerializeField] private TMP_Text _name;

        [Tooltip("Texto que dice la IA.")]
        [SerializeField] private TMP_Text _speech;

        [Tooltip("Velocidad de la máquina de escribir (caracteres por segundo). 0 = todo de golpe.")]
        [Min(0f)]
        [SerializeField] private float _charsPerSecond = 45f;

        [Tooltip("CanvasGroup para ocultar o mostrar la esquina (opcional).")]
        [SerializeField] private CanvasGroup _group;

        private TutorialSequence _tutorial;
        private bool _subscribed;
        private float _typed;
        private bool _typing;

        /// <summary>Muestra u oculta la esquina completa.</summary>
        public void SetVisible(bool visible)
        {
            if (_group == null) return;
            _group.alpha = visible ? 1f : 0f;
            _group.interactable = visible;
            _group.blocksRaycasts = visible;
        }

        /// <summary>La IA dice el texto de la clave dada.</summary>
        public void Say(string key)
        {
            if (_speech == null || _bootstrap == null || _bootstrap.Texts == null) return;
            _speech.text = _bootstrap.Texts.Get(key);
            _typed = 0f;
            if (_charsPerSecond <= 0f)
            {
                FinishTyping();
                return;
            }
            _speech.maxVisibleCharacters = 0;
            _typing = true;
        }

        private void Start()
        {
            if (_bootstrap == null)
            {
                Debug.LogWarning("AiCorner: falta asignar el GameBootstrap.", this);
                return;
            }
            _bootstrap.WhenReady(Connect);
        }

        private void Connect()
        {
            if (this == null || _subscribed) return;
            _tutorial = _bootstrap.Tutorial;
            if (_name != null) _name.text = _bootstrap.Texts.Get("ui.ai.name");
            if (_tutorial == null) return;
            _tutorial.OnStepStarted += HandleStepStarted;
            _subscribed = true;
            SayCurrentMission();
        }

        private void OnDestroy()
        {
            if (_subscribed && _tutorial != null) _tutorial.OnStepStarted -= HandleStepStarted;
            _subscribed = false;
        }

        private void HandleStepStarted(TutorialStep step) => SayCurrentMission();

        private void SayCurrentMission()
        {
            Say(_tutorial.CurrentStep == TutorialStep.Done ? DoneKey : _tutorial.MissionKey);
        }

        private void Update()
        {
            if (!_typing || _speech == null) return;
            _typed += _charsPerSecond * Time.unscaledDeltaTime;
            _speech.maxVisibleCharacters = Mathf.FloorToInt(_typed);
            if (_typed >= _speech.text.Length) FinishTyping();
        }

        private void FinishTyping()
        {
            _typing = false;
            if (_speech != null) _speech.maxVisibleCharacters = int.MaxValue;
        }
    }
}
