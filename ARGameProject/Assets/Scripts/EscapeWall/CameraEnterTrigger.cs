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

        private bool _wasInside;

        private void Start()
        {
            if (_playerTransform == null)
                _playerTransform = GameManager.Instance.Player.transform;
        }

        private void Update()
        {
            if (_playerTransform == null)
                return;

            bool isInside = IsInsideBox(_playerTransform.position);

            if (isInside && !_wasInside)
                OnEnter?.Invoke();

            if (!isInside && _wasInside)
                OnExit?.Invoke();

            _wasInside = isInside;
        }

        private bool IsInsideBox(Vector3 position)
        {
            Vector3 local = transform.InverseTransformPoint(position);
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
    }
}