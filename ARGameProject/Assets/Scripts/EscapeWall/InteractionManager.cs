using UnityEngine;
using UnityEngine.InputSystem;

namespace EscapeWall
{
    public class InteractionManager : MonoBehaviour
    {
        private Camera _arCamera;
        public float interactionDistance = 5f;
        private void Awake()
        {
            _arCamera = GameManager.Instance.Player.Camera;
        }

        private void Update()
        {
             if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame)
                return;

            Vector2 screenPosition = Pointer.current.position.ReadValue();

            var ray = _arCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
            {
                ITapable tappable = hit.transform.GetComponentInParent<ITapable>();
                tappable?.Tap();
            }
        }
    }
}
