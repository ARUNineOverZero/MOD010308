using UnityEngine;
using UnityEngine.InputSystem;

namespace EscapeWall
{
    public class InteractionManager : MonoBehaviour
    {
        private Camera _arCamera;
        private void Awake()
        {
            _arCamera = GameManager.Instance.ArCamera;
        }

        private void Update()
        {
             if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame)
                return;

            Vector2 screenPosition = Pointer.current.position.ReadValue();

            var ray = _arCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1f) && hit.transform.TryGetComponent(out ITapable t))
                t.Tap();
        }
    }
}
