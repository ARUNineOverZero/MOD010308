using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace EscapeWall.PlayerComponents
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("UI Filtering")]
        [SerializeField] private bool _ignoreTouchesOverUI = true;

#if UNITY_EDITOR
        private const int MouseFingerId = 0;
        private const int MouseTouchId = 0;
        private bool _mouseWasDown;
#endif

        public struct FingerData
        {
            public int fingerId;
            public int touchId;
            public Vector2 position;
            public float radius;
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

#if UNITY_EDITOR

        private void Update()
        {
            HandleMouse();
        }


        private void HandleMouse()
        {
            if (Mouse.current == null) return;

            bool isDown = Mouse.current.leftButton.isPressed;
            Vector2 position = Mouse.current.position.ReadValue();

            if (isDown && !_mouseWasDown)
            {
                bool overUI = IsPointerOverUI(MouseFingerId, position);
                if (overUI && _ignoreTouchesOverUI)
                {
                    _mouseWasDown = true;
                    return;
                }

                FingerData data = new FingerData
                {
                    fingerId = MouseFingerId,
                    touchId = MouseTouchId,
                    position = position,
                    radius = 8f * Screen.dpi / 25.4f,
                    isOverUI = overUI
                };

                _activeFingers[MouseFingerId] = data;
                OnFingerDown?.Invoke(data);
            }
            else if (!isDown && _mouseWasDown)
            {
                if (_activeFingers.TryGetValue(MouseFingerId, out FingerData data))
                {
                    data.position = position;
                    _activeFingers.Remove(MouseFingerId);
                    OnFingerUp?.Invoke(data);
                }
            }
            else if (isDown && _activeFingers.ContainsKey(MouseFingerId))
            {
                FingerData data = _activeFingers[MouseFingerId];
                if (data.position != position)
                {
                    data.position = position;
                    _activeFingers[MouseFingerId] = data;
                    OnFingerMove?.Invoke(data);
                }
            }

            _mouseWasDown = isDown;
        }
#endif

        private void HandleFingerDown(Finger finger)
        {
            Touch touch = finger.currentTouch;
            bool overUI = IsPointerOverUI(touch.touchId, touch.screenPosition);

            var fingerId = _activeFingers.Count == 0
                            ? 0
                            : ActiveFingers.Values.Max(x => x.fingerId) + 1;


            FingerData data = new FingerData
            {
                fingerId = fingerId,
                touchId = touch.touchId,
                position = touch.screenPosition,
                radius = 25f,
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
            if (!_activeFingers.TryGetValue(touch.touchId, out FingerData data)) return;

            data.radius = GetTouchRadius(touch);

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

        private float GetTouchRadius(Touch touch)
        {
            float reported = Mathf.Max(touch.radius.x, touch.radius.y);
            if (reported > 1f)
                return reported;

            return 8f * Screen.dpi / 25.4f;
        }
    }
}