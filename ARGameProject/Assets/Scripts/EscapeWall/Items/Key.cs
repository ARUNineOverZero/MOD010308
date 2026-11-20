using EscapeWall.PlayerComponents.Interaction;
using UnityEngine;

namespace EscapeWall.Items
{
    public class Key : PhysicalItem
    {
        [SerializeField] private int _id = -1;
        public int ID => _id;
    }

    public class PhysicalItem : MonoBehaviour, IInteractable
    {
        [SerializeField] protected Rigidbody _rigidbody;
        public Rigidbody Rigidbody => _rigidbody;
        protected InteractionState _state = InteractionState.None;
        public InteractionState State => _state;

        public void Awake()
        {
            if(_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();
        }

        public void BeginInteract()
        {
            _state = InteractionState.Began;
            GameManager.Instance.Player.PlayerHand.SetInHand(this);
        }

        public void EndInteraction()
        {
            if(_state == InteractionState.None || _state == InteractionState.Ended)
                return;

            _state = InteractionState.None;
            GameManager.Instance.Player.PlayerHand.Drop(this);
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
