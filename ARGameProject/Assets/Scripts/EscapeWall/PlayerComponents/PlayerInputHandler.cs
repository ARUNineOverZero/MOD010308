using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace EscapeWall.PlayerComponents
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("UI Filtering")]
        [SerializeField] private bool _ignoreTouchesOverUI = true;

        public struct FingerData
        {
            public int fingerId;
            public Vector2 position;
            public bool isOverUI;
        }

        public event Action<FingerData> OnFingerDown;
        public event Action<FingerData> OnFingerUp;
        public event Action<FingerData> OnFingerMove;
        private readonly Dictionary<int, FingerData> _activeFingers = new();
        public IReadOnlyDictionary<int, FingerData> ActiveFingers => _activeFingers;
        public bool HasActiveFinger => _activeFingers.Count > 0;

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            RegisterActions();
        }

        private void OnDisable()
        {
            DeregisterActions();
            EnhancedTouchSupport.Disable();
            _activeFingers.Clear();
        }

        private void RegisterActions()
        {
            Touch.onFingerDown += HandleFingerDown;
            Touch.onFingerUp += HandleFingerUp;
            Touch.onFingerMove += HandleFingerMove;
        }

        private void DeregisterActions()
        {
            Touch.onFingerDown -= HandleFingerDown;
            Touch.onFingerUp -= HandleFingerUp;
            Touch.onFingerMove -= HandleFingerMove;
        }

        private void HandleFingerDown(Finger finger)
        {
            Touch touch = finger.currentTouch;
            bool overUI = IsPointerOverUI(touch.touchId, touch.screenPosition);

            FingerData data = new FingerData
            {
                fingerId = touch.touchId,
                position = touch.screenPosition,
                isOverUI = overUI
            };

            if (overUI && _ignoreTouchesOverUI) return;

            _activeFingers[touch.touchId] = data;
            OnFingerDown?.Invoke(data);
        }

        private void HandleFingerUp(Finger finger)
        {
            Touch touch = finger.currentTouch;
            if (!_activeFingers.ContainsKey(touch.touchId)) return;

            FingerData data = _activeFingers[touch.touchId];
            data.position = touch.screenPosition;

            _activeFingers.Remove(touch.touchId);
            OnFingerUp?.Invoke(data);
        }

        private void HandleFingerMove(Finger finger)
        {
            Touch touch = finger.currentTouch;
            if (!_activeFingers.TryGetValue(touch.touchId, out var data)) return;

            data.position = touch.screenPosition;
            _activeFingers[touch.touchId] = data;
            OnFingerMove?.Invoke(data);
        }

        private bool IsPointerOverUI(int pointerId, Vector2 screenPosition)
        {
            if (!_ignoreTouchesOverUI || EventSystem.current == null)
                return false;

            return EventSystem.current.IsPointerOverGameObject(pointerId);
        }
        public bool TryGetPrimaryFinger(out FingerData finger)
        {
            foreach (FingerData f in _activeFingers.Values)
            {
                finger = f;
                return true;
            }

            finger = default;
            return false;
        }
    }
}