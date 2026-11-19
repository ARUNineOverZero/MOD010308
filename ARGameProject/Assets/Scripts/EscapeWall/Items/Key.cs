using EscapeWall.PlayerComponents.Interaction;
using UnityEngine;

namespace EscapeWall.Items
{
    public class Key : MonoBehaviour, IInteractable
    {
        [SerializeField] private Rigidbody _rigidbody;
        public Rigidbody Rigidbody => _rigidbody;
        [SerializeField] private int _id = -1;

        public int ID => _id;
        private InteractionState _state = InteractionState.None;
        public InteractionState State => _state;

        public void BeginInteract()
        {
            _state = InteractionState.Began;
            _rigidbody.isKinematic = true;
            _rigidbody.interpolation = RigidbodyInterpolation.None;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            transform.parent = GameManager.Instance.Player.transform;

        }

        public void EndInteract()
        {
            _state = InteractionState.Ended;
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = false;
                _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            transform.parent = null;
        }

        public void DisableRigidBody()
        {
            _rigidbody.interpolation = RigidbodyInterpolation.None;
            _rigidbody.isKinematic = true;
            _rigidbody.detectCollisions = false;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        public void EnableRigidbody()
        {
            _rigidbody.isKinematic = false;
            _rigidbody.detectCollisions = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidbody.WakeUp();
        }
    }
}
