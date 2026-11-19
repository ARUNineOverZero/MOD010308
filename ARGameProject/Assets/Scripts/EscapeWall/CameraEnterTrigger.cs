using System.Collections.Generic;
using EscapeWall.PlayerComponents.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeWall
{
    public class CameraEnterTrigger : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Vector3 _boxSize = Vector3.one;

        [Header("Events")]
        public UnityEvent OnEnter;
        public UnityEvent OnExit;

        private readonly HashSet<IInteractable> _interactables = new();
        private bool _playerInside;
        private bool _wasInside;

        private void Start()
        {
            if (_playerTransform == null && GameManager.Instance != null)
                _playerTransform = GameManager.Instance.Player.transform;
        }

        private void Update()
        {
            // Fallback box check for the player (in case trigger doesn't catch them)
            bool playerInBox = IsInsideBox(_playerTransform);
            bool isInside = playerInBox || _playerInside || _interactables.Count > 0;

            if (isInside && !_wasInside)
                OnEnter?.Invoke();

            if (!isInside && _wasInside)
                OnExit?.Invoke();

            _wasInside = isInside;
        }

        private bool IsInsideBox(Transform target)
        {
            if (target == null) return false;

            Vector3 local = transform.InverseTransformPoint(target.position);
            Vector3 half = _boxSize * 0.5f;

            return Mathf.Abs(local.x) <= half.x &&
                   Mathf.Abs(local.y) <= half.y &&
                   Mathf.Abs(local.z) <= half.z;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, _boxSize);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Player entered via trigger
            if (IsPlayer(other.transform))
            {
                _playerInside = true;
                return;
            }

            // Interactable item entered
            var interactable = other.GetComponentInParent<IInteractable>();
            if (interactable != null)
                _interactables.Add(interactable);
        }

        private void OnTriggerExit(Collider other)
        {
            // Player exited via trigger
            if (IsPlayer(other.transform))
            {
                _playerInside = false;
                return;
            }

            // Interactable item exited
            var interactable = other.GetComponentInParent<IInteractable>();
            if (interactable != null)
                _interactables.Remove(interactable);
        }

        private bool IsPlayer(Transform t)
        {
            if (t == null || _playerTransform == null) return false;
            return t == _playerTransform || t.IsChildOf(_playerTransform);
        }
    }
}