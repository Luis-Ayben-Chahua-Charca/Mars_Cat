// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace MarsCat.Unity.View
{
    /// <summary>
    /// Selección de edificios con el ratón: clic izquierdo con Raycast desde la cámara.
    /// Un solo objeto en la escena. Requiere un EventSystem con InputSystemUIInputModule para ignorar clics sobre la UI.
    /// </summary>
    public class SelectionController : MonoBehaviour
    {
        [Tooltip("Cámara de la escena (la del rig).")]
        [SerializeField] private Camera _camera;

        [Tooltip("Capas que se pueden seleccionar (los edificios con Collider).")]
        [SerializeField] private LayerMask _selectableLayers = ~0;

        [Tooltip("Distancia máxima del Raycast.")]
        [SerializeField] private float _maxDistance = 500f;

        [Tooltip("Collide: el rayo también acierta colliders trigger (el depósito y la esclusa usan trigger para no bloquear a los gatos). " +
                 "Limitar _selectableLayers a la capa de edificios para no acertar los triggers de puertas y cúpulas.")]
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Collide;

        public BuildingView Selected { get; private set; }

        public event Action<BuildingView> SelectionChanged;

        public void Select(BuildingView view)
        {
            if (view == Selected) return;
            if (Selected != null) Selected.SetSelected(false);
            Selected = view;
            if (Selected != null) Selected.SetSelected(true);
            SelectionChanged?.Invoke(Selected);
        }

        public void Clear() => Select(null);

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Clear();
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null || _camera == null || !mouse.leftButton.wasPressedThisFrame) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = _camera.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, _maxDistance, _selectableLayers, _triggerInteraction))
                Select(hit.collider.GetComponentInParent<BuildingView>());
            else
                Clear();
        }

        private void OnDisable()
        {
            if (Selected != null) Clear();
        }
    }
}
