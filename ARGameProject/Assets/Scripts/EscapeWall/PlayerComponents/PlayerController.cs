using EscapeWall.PlayerComponents.Interaction;
using TMPro;
using UnityEngine;

namespace EscapeWall.PlayerComponents
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Self Reference")]
        [SerializeField] private PlayerInputHandler _inputHandler;
        [SerializeField] private Camera _camera;
        [SerializeField] private InteractionManager _interactionManager;

        [Header("Interaction Settings")]
        [SerializeField] private float _interactionDistance = 5f;
        [SerializeField] private LayerMask _layerMask;


        public void Awake()
        {
            if (_inputHandler == null)
                _inputHandler = GameManager.Instance.Player.InputHandler;

            if (_camera == null)
                _camera = GameManager.Instance.Player.Camera;

            if (_interactionManager == null)
                _interactionManager = GameManager.Instance.Player.InteractionManager;
        }

        private void OnEnable()
        {
            if (_inputHandler == null) return;

            RegisterActions();
        }

        private void OnDisable()
        {
            if (_inputHandler == null) return;

            DeregisterActions();
        }

        private void RegisterActions()
        {
            _inputHandler.OnFingerDown += HandleFingerDown;
            _inputHandler.OnFingerMove += HandleFingerMove;
            _inputHandler.OnFingerUp += HandleFingerUp;
        }

        private void DeregisterActions()
        {
            _inputHandler.OnFingerDown -= HandleFingerDown;
            _inputHandler.OnFingerMove -= HandleFingerMove;
            _inputHandler.OnFingerUp -= HandleFingerUp;
        }


        private bool TryIteract(Vector3 screenPosition, out IInteractable interactable)
        {
            var ray = _camera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _layerMask))
            {
                interactable = hit.transform.GetComponentInParent<IInteractable>();
                return interactable != null;
            }

            interactable = null;
            return false;
        }

        private void HandleFingerDown(PlayerInputHandler.FingerData data)
        {
            switch (data.fingerId)
            {
                case 1:
                    if(TryIteract(data.position, out IInteractable interactable))
                        _interactionManager.BeginInteraction(interactable);
                    break;
                default:
                    break;
            }
        }

        private void HandleFingerMove(PlayerInputHandler.FingerData data)
        {
            switch (data.fingerId)
            {
                case 1:
                    if(TryIteract(data.position, out IInteractable interactable))
                        _interactionManager.MoveInteraction();
                    break;
                default:
                    break;
            }
            
        }

        private void HandleFingerUp(PlayerInputHandler.FingerData data)
        {
            switch (data.fingerId)
            {
                case 1:
                    _interactionManager.EndInteraction();
                    break;
                default:
                    break;
            }
        }
    }
}