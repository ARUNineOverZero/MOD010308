using EscapeWall.PlayerComponents.Interaction;
using UnityEngine;

namespace EscapeWall.PlayerComponents
{
    using System;
    using UnityEngine;

    public class PlayerHand : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private Camera _camera;

        [Header("Grab Settings")]
        public float grabDistance = 2.0f;
        public float hoverSpeed = 10f;
        public float rotationMagnetSpeed = 10f;

        [Header("Throw Settings")]
        public float throwForce = 15f;

        [Header("Grab Distance Settings")]
        public float minGrabDistance = .25f;
        public float maxGrabDistance = .5f;
        public float scrollSensitivity = .1f;

        [SerializeField] private IInteractable _inHand;
        [SerializeField] private Rigidbody _rigidbody;
        private float _radius;
        private Vector3 _screenPosition;
        public bool HasItem => _rigidbody != null && _inHand != null;
        private Quaternion _targetRotation;

        public bool IsHoldingItem => _rigidbody != null;

        private void FixedUpdate() => HandleHoveringPhysical();

        public bool SetInHand(IInteractable item, Vector3 screenPosition, float radius)
        {
            if (_inHand != null)
                _inHand.EndInteraction();

            _screenPosition = screenPosition;
             _radius = radius;

            Rigidbody rb = ((Component)item)?.gameObject.GetComponent<Rigidbody>();

            if (rb == null)
                return false;

            _inHand = item;
            _rigidbody = rb;

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif

            grabDistance = Vector3.Distance(_rigidbody.transform.position, transform.position);
            grabDistance = Mathf.Clamp(grabDistance, minGrabDistance, maxGrabDistance);

            return true;
        }

        public void Drop(IInteractable item = null)
        {
            if (_rigidbody == null || (item != null && item != _inHand)) return;

            //Debug.Log($"{_rigidbody.gameObject.name} dropped!");

            //_rigidbody.interpolation = RigidbodyInterpolation.None;

            if (_inHand != null)
                _inHand.EndInteraction();

            _inHand = null;
            _rigidbody = null;
        }

        private void HandleHoveringPhysical()
        {
            if (_rigidbody == null || _camera == null) return;
            
            var ray = _camera.ScreenPointToRay(new Vector2(_screenPosition.x, _screenPosition.y + _radius * 2));
            Vector3 targetPosition = _camera.transform.position + (ray.direction * grabDistance);
            Vector3 directionToTarget = targetPosition - _rigidbody.transform.position;
            _rigidbody.linearVelocity = directionToTarget * hoverSpeed;

            Quaternion rotationDifference = Quaternion.Euler(0, transform.eulerAngles.y, 0) * _targetRotation;
            rotationDifference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

            if (angleInDegrees > 180f)
                angleInDegrees -= 360f;

            if (Mathf.Abs(angleInDegrees) > 0.5f)
                _rigidbody.angularVelocity = rotationAxis * (angleInDegrees * Mathf.Deg2Rad * rotationMagnetSpeed);
            else
                _rigidbody.angularVelocity = Vector3.Lerp(_rigidbody.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 15f);
        }

        public void ChangeGrabDistance(float scrollDelta)
        {
            if (!IsHoldingItem) return;

            float direction = Mathf.Sign(scrollDelta);
            grabDistance += direction * scrollSensitivity;
            grabDistance = Mathf.Clamp(grabDistance, minGrabDistance, maxGrabDistance);
        }

        public void UpdateScreenPosition(Vector2 screenPosition, float radius)
        {
            _screenPosition = screenPosition;
            _radius = radius;
        }
    }
}