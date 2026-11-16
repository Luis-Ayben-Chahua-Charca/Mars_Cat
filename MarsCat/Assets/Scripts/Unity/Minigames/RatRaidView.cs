// Asistencia de IA: escrito con Claude Code (Anthropic).
using MarsCat.Logic.Buildings;
using MarsCat.Logic.Food;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MarsCat.Unity.Minigames
{
    /// <summary>
    /// La Redada (#60), preparación de la granja de ratones. Panel pequeño arriba al centro que no tapa la escena
    /// ni pausa el juego: el jugador hace clic sobre las ratas sueltas (LooseRatView) y un gato libre va a buscarlas.
    /// Esta vista solo pide la captura (GameBootstrap.RequestRatCapture) y muestra el estado; la preparación de
    /// la granja la aplica el bootstrap cuando supera el pie de cría, así que no llama a Finish.
    /// </summary>
    public class RatRaidView : MinigameView
    {
        [Tooltip("Instrucciones fijas del panel.")]
        [SerializeField] private TMP_Text _instructionsText;

        [Tooltip("Ratas sueltas que quedan en la estación.")]
        [SerializeField] private TMP_Text _looseText;

        [Tooltip("Ratas en la granja / pie de cría necesario.")]
        [SerializeField] private TMP_Text _farmText;

        [Tooltip("Estado: un gato va por la rata, la granja ya funciona, etc.")]
        [SerializeField] private TMP_Text _statusText;

        [Tooltip("Botón Terminar: cierra el panel (las capturas en curso siguen).")]
        [SerializeField] private Button _doneButton;

        [Tooltip("Texto del botón Terminar (para traducirlo).")]
        [SerializeField] private TMP_Text _doneLabel;

        [Tooltip("Cámara de la escena (la del rig), para el raycast del clic.")]
        [SerializeField] private Camera _camera;

        [Tooltip("Capas donde están las ratas sueltas.")]
        [SerializeField] private LayerMask _ratLayers = ~0;

        [SerializeField] private float _maxDistance = 500f;

        [Tooltip("Collide: acierta también colliders trigger.")]
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Collide;

        private int _pending;
        private int _minBreeders;
        private bool _subscribed;
        private bool _lastRejected;

        public override bool PausesGame => false;

        protected override void OnOpened()
        {
            _lastRejected = false;
            _pending = 0;
            RatFarmConfig config = Bootstrap.Balance != null ? RatFarmConfig.FromBalance(Bootstrap.Balance) : null;
            _minBreeders = config != null ? config.MinBreeders : 0;

            // Marcadas de antes que siguen en pie (ids de 0 hasta el total inicial de ratas sueltas).
            if (Bootstrap.RatRaid != null && Bootstrap.LooseRats != null)
            {
                int max = Bootstrap.LooseRats.Count + 64;
                for (int i = 0; i < max; i++)
                    if (Bootstrap.LooseRats.IsLoose(i) && Bootstrap.RatRaid.IsMarked(i)) _pending++;
            }

            Bootstrap.RatMarked += HandleMarked;
            Bootstrap.RatCaptured += HandleCaptured;
            if (Bootstrap.RatFarm != null) Bootstrap.RatFarm.OnRatsChanged += HandleRatsChanged;
            if (Bootstrap.LooseRats != null) Bootstrap.LooseRats.OnTaken += HandleTaken;
            _subscribed = true;
            if (_doneButton != null) _doneButton.onClick.AddListener(Close);

            if (_instructionsText != null) _instructionsText.text = T("minigame.raid.instructions");
            if (_doneLabel != null) _doneLabel.text = T("minigame.raid.done");
            Refresh();
        }

        protected override void OnClosed()
        {
            if (_doneButton != null) _doneButton.onClick.RemoveListener(Close);
            Unsubscribe();
        }

        private void OnDestroy() => Unsubscribe();

        private void Unsubscribe()
        {
            if (!_subscribed || Bootstrap == null) return;
            _subscribed = false;
            Bootstrap.RatMarked -= HandleMarked;
            Bootstrap.RatCaptured -= HandleCaptured;
            if (Bootstrap.RatFarm != null) Bootstrap.RatFarm.OnRatsChanged -= HandleRatsChanged;
            if (Bootstrap.LooseRats != null) Bootstrap.LooseRats.OnTaken -= HandleTaken;
        }

        protected override void Tick(float realDt)
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || _camera == null || !mouse.leftButton.wasPressedThisFrame) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = _camera.ScreenPointToRay(mouse.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, _maxDistance, _ratLayers, _triggerInteraction)) return;
            LooseRatView rat = hit.collider.GetComponentInParent<LooseRatView>();
            if (rat == null) return;

            _lastRejected = !Bootstrap.RequestRatCapture(rat.RatId);
            Refresh();
        }

        private void HandleMarked(int ratId)
        {
            _pending++;
            _lastRejected = false;
            Refresh();
        }

        private void HandleCaptured(int ratId, bool captured)
        {
            if (_pending > 0) _pending--;
            Refresh();
        }

        private void HandleRatsChanged(int rats) => Refresh();

        private void HandleTaken(int ratId) => Refresh();

        private void Refresh()
        {
            if (!IsOpen || Bootstrap == null) return;
            int loose = Bootstrap.LooseRats != null ? Bootstrap.LooseRats.Count : 0;
            int rats = Bootstrap.RatFarm != null ? Bootstrap.RatFarm.Rats : 0;
            if (_looseText != null) _looseText.text = Bootstrap.Texts.Format("minigame.raid.loose", loose);
            if (_farmText != null) _farmText.text = Bootstrap.Texts.Format("minigame.raid.farm", rats, _minBreeders + 1);

            Building farm = Bootstrap.GetBuilding(GameBootstrap.RatFarmId);
            bool running = farm != null && farm.State == BuildingState.Running;
            string key;
            if (running) key = "minigame.raid.running";
            else if (_lastRejected) key = "minigame.raid.rejected";
            else if (_pending > 0) key = "minigame.raid.going";
            else if (loose <= 0) key = "minigame.raid.none";
            else key = "minigame.raid.idle";
            if (_statusText != null) _statusText.text = T(key);
        }

        private string T(string key) => Bootstrap != null && Bootstrap.Texts != null ? Bootstrap.Texts.Get(key) : key;
    }
}
