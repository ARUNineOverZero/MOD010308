using System.Collections;
using UnityEngine;

namespace EscapeWall.Interactive
{
    public class SlidingDoor : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private Vector3 _closePosition;
        [SerializeField] private Vector3 _openPosition;
        [SerializeField] private float _speed = 4f;

        [Header("State")]
        [SerializeField] private bool _isOpen;
        public bool IsOpen => _isOpen;

        private Vector3 _targetPosition;
        private Coroutine _moveCoroutine;

        private void Awake()
        {
            // Snap to the correct starting position
            _targetPosition = _isOpen ? _openPosition : _closePosition;
            transform.localPosition = _targetPosition;
        }

        public void Open()
        {
            if (_isOpen) return;

            _isOpen = true;
            _targetPosition = _openPosition;
            StartMove();
        }

        public void Close()
        {
            if (!_isOpen) return;

            _isOpen = false;
            _targetPosition = _closePosition; 
            StartMove();
        }

        public void Toggle()
        {
            if (_isOpen)
                Close();
            else
                Open();
        }

        private void StartMove()
        {
            if (_moveCoroutine != null)
                return;

            _moveCoroutine = StartCoroutine(MoveDoor());
        }

        private IEnumerator MoveDoor()
        {
            while (Vector3.Distance(transform.localPosition, _targetPosition) > 0.001f)
            {
                transform.localPosition = Vector3.MoveTowards(
                    transform.localPosition,
                    _targetPosition,
                    _speed * Time.deltaTime
                );
                yield return null;
            }

            transform.localPosition = _targetPosition;
            _moveCoroutine = null;
        }

        public void SetOpenPosition(Vector3 localPosition) => _openPosition = localPosition;
        public void SetSpeed(float newSpeed) => _speed = Mathf.Max(0.01f, newSpeed);

        [ContextMenu("Set Closed Position")]
        private void SetClosedPosition() => _closePosition = transform.localPosition;

        [ContextMenu("Set Open Position")]
        private void SetOpenPosition() => _openPosition = transform.localPosition;

        [ContextMenu("Go To Closed")]
        private void GotoClosed() => transform.localPosition = _closePosition;

        [ContextMenu("Go To Open")]
        private void GotoOpen() => transform.localPosition = _openPosition;
    }
}
