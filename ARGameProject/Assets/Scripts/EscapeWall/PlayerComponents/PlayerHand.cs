using EscapeWall.PlayerComponents.Interaction;
using UnityEngine;

namespace EscapeWall.PlayerComponents
{
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
        public float minGrabDistance = 1f;
        public float maxGrabDistance = 1.5f;
        public float scrollSensitivity = .1f;

        [SerializeField] private IInteractable _inHand;
        [SerializeField] private Rigidbody _rigidbody;

        private Quaternion _targetRotation;

        public bool IsHoldingItem => _rigidbody != null;

        private void FixedUpdate() => HandleHoveringPhysical();

        public bool SetInHand(IInteractable item)
        {
            if (_inHand != null)
                _inHand.EndInteraction();

            Rigidbody rb = ((Component)item)?.gameObject.GetComponent<Rigidbody>();

            if (rb == null)
                return false;

            _inHand = item;
            _rigidbody = rb;

            //_rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            //Quaternion relativeRotation = Quaternion.Inverse(transform.rotation) * _rigidbody.rotation;
            //float relativeY = relativeRotation.eulerAngles.y;
            //float snappedY = Mathf.Round(relativeY / 90f) * 90f;
            //_targetRotation = Quaternion.Euler(0, snappedY, 0);

            grabDistance = Vector3.Distance(_rigidbody.transform.position, transform.position);
            grabDistance = Mathf.Clamp(grabDistance, minGrabDistance, maxGrabDistance);

            return true;
        }

        public void Throw()
        {
            if (_rigidbody == null) return;

            Rigidbody rb = _rigidbody;
            Drop();

            if (_camera != null)
                rb.AddForce(_camera.transform.forward * throwForce, ForceMode.Impulse);
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

            Vector3 targetPosition = _camera.transform.position + (_camera.transform.forward * grabDistance);
            Vector3 directionToTarget = targetPosition - _rigidbody.transform.position;
            _rigidbody.linearVelocity = directionToTarget * hoverSpeed;

            Quaternion rotationDifference = Quaternion.Euler(0, transform.eulerAngles.y, 0) * _targetRotation ;
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
    }
}