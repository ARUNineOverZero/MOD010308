using System.Collections;
using UnityEngine;

namespace EscapeWall
{
    public class WallHatch : MonoBehaviour, ITapable
    {
        [SerializeField] private Transform _hatch;
        [SerializeField] private float _speed = 1f;
        [SerializeField] private float _maxRotation = 90f;

        private bool _isOpen;
        private float _currentPosition;
        private Coroutine _openCloseCoroutine;

        private void Start()
        {
            ApplyRotation();
        }

        private void ToggleOpen()
        {
            _isOpen = !_isOpen;

            if (_openCloseCoroutine != null)
                return;

            _openCloseCoroutine = StartCoroutine(OpenCloseHatch());
        }

        private IEnumerator OpenCloseHatch()
        {
            float target = _isOpen ? 1f : 0f;

            while (!Mathf.Approximately(_currentPosition, target))
            {
                _currentPosition = Mathf.MoveTowards(_currentPosition, target, _speed * Time.deltaTime);
                ApplyRotation();
                yield return null;
            }

            _currentPosition = target;
            ApplyRotation();
            _openCloseCoroutine = null;
        }

        private void ApplyRotation()
        {
            if (_hatch == null)
                return;

            Vector3 euler = _hatch.localEulerAngles;
            euler.x = _currentPosition * _maxRotation;
            _hatch.localRotation = Quaternion.Euler(euler);
        }

        public void Tap() => ToggleOpen();
    }
}
