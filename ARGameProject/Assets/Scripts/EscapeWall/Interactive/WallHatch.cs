using System.Collections;
using EscapeWall.PlayerComponents.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeWall.Interactive
{
    public class WallHatch : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform _hatch;
        [SerializeField] private float _speed = 1f;
        [SerializeField] private float _maxRotation = 90f;
        private bool _allowOpen = false;
        private bool _allowClose = true;
        private float _target;

        private bool _isOpen;
        private float _currentPosition;
        private Coroutine _openCloseCoroutine;

        [Header("Events")]
        public UnityEvent OnOpen;
        public UnityEvent OnClose;
        public UnityEvent OnClosed;

        public InteractionState State => InteractionState.None;

        private void Start()
        {
            ApplyRotation();
        }

        public void Open()
        {
            if (!_allowOpen || _isOpen)
                return;

            _isOpen = true;
            _target = 1;

            OnOpen?.Invoke();

            if (_openCloseCoroutine != null)
                return;

            _openCloseCoroutine = StartCoroutine(OpenCloseHatch());
        }

        public void Close()
        {
            if (!_allowClose) return;

            if (!_isOpen)
                return;

            _isOpen = false;
            _target = 0f;

            OnClose?.Invoke();

            if (_openCloseCoroutine != null)
                return;

            _openCloseCoroutine = StartCoroutine(OpenCloseHatch());
        }

        private void ToggleOpen()
        {
            if (_isOpen)
                Close();
            else
                Open();
        }

        public void Unlock()
        {
            _allowOpen = true;
        }

        private IEnumerator OpenCloseHatch()
        {
            while (!Mathf.Approximately(_currentPosition, _target))
            {
                _currentPosition = Mathf.MoveTowards(_currentPosition, _target, _speed * Time.deltaTime);
                ApplyRotation();
                yield return null;
            }

            _currentPosition = _target;
            ApplyRotation();
            if (!_isOpen)
                OnClosed?.Invoke();
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

        public void PreventClose()
        {
            _allowClose = false;
            if (_currentPosition != 0)
                Open();
        }

        public void AllowClose()
        {
            _allowClose = true;
        }

        public void BeginInteract() => ToggleOpen();

        public void EndInteraction()
        {

        }
    }
}
