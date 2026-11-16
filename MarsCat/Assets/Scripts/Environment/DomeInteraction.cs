// Asistencia de IA: escrito con Claude Code (Anthropic).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MarsCat.Unity.Environment
{
    /// <summary>
    /// Abre y cierra una cúpula (CapsuleDomeController) cuando el jugador está dentro de la zona y pulsa la tecla de interacción.
    /// Va en el mismo objeto que un Collider marcado como Trigger (la zona de la sala). Solo detecta: no bloquea el paso
    /// ni interviene en los colliders de la cabina.
    /// Se considera jugador cualquier CharacterController que entre en la zona (Nio_Player lo lleva); el ratón enemigo
    /// no tiene CharacterController ni Rigidbody, así que no la activa.
    /// La presencia se renueva con OnTriggerStay: si el jugador se teletransporta o se desactiva dentro de la zona
    /// (Unity no envía OnTriggerExit en ese caso), deja de contar al cabo de unos pasos de física.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DomeInteraction : MonoBehaviour
    {
        [SerializeField] private CapsuleDomeController _dome;
        [Tooltip("Tecla de interacción (Input System).")]
        [SerializeField] private Key _key = Key.E;
        [SerializeField] private UnityEvent _onPlayerEnter = new UnityEvent();
        [SerializeField] private UnityEvent _onPlayerExit = new UnityEvent();
        [SerializeField] private UnityEvent _onInteract = new UnityEvent();

        private const float StepsBeforeExit = 2.5f;
        private readonly Dictionary<Collider, float> _lastSeen = new Dictionary<Collider, float>();
        private readonly List<Collider> _expired = new List<Collider>();

        public CapsuleDomeController Dome => _dome;
        public Key InteractKey => _key;
        public bool PlayerInside => _lastSeen.Count > 0;
        public int Interactions { get; private set; }

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
            _dome = GetComponentInParent<CapsuleDomeController>();
        }

        private void Awake()
        {
            Collider zone = GetComponent<Collider>();
            if (!zone.isTrigger)
            {
                Debug.LogWarning("[DomeInteraction] El collider de " + name + " no era Trigger; se marca como Trigger para no bloquear el paso.", this);
                zone.isTrigger = true;
            }
        }

        private void OnDisable()
        {
            bool wasInside = PlayerInside;
            _lastSeen.Clear();
            if (wasInside) _onPlayerExit.Invoke();
        }

        private void Update()
        {
            PurgeExpired();
            if (!PlayerInside || Keyboard.current == null) return;
            if (Keyboard.current[_key].wasPressedThisFrame) Interact();
        }

        /// <summary>Alterna la cúpula (abrir/cerrar). La llama la tecla; también sirve para botones de UI o eventos.</summary>
        public void Interact()
        {
            if (_dome == null) return;
            _dome.Alternar();
            Interactions++;
            _onInteract.Invoke();
        }

        private void OnTriggerEnter(Collider other) => Seen(other);
        private void OnTriggerStay(Collider other) => Seen(other);

        private void OnTriggerExit(Collider other)
        {
            if (_lastSeen.Remove(other) && _lastSeen.Count == 0) _onPlayerExit.Invoke();
        }

        private void Seen(Collider other)
        {
            if (!(other is CharacterController)) return;
            bool first = _lastSeen.Count == 0;
            _lastSeen[other] = Time.fixedTime;
            if (first) _onPlayerEnter.Invoke();
        }

        private void PurgeExpired()
        {
            if (_lastSeen.Count == 0) return;
            float limit = Time.fixedTime - Time.fixedDeltaTime * StepsBeforeExit;
            _expired.Clear();
            foreach (KeyValuePair<Collider, float> entry in _lastSeen)
                if (entry.Key == null || !entry.Key.enabled || !entry.Key.gameObject.activeInHierarchy || entry.Value < limit)
                    _expired.Add(entry.Key);
            foreach (Collider c in _expired) _lastSeen.Remove(c);
            if (_expired.Count > 0 && _lastSeen.Count == 0) _onPlayerExit.Invoke();
        }
    }
}
