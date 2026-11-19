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

        public void BeginInteract()
        {
            _rigidbody.isKinematic = true;
            transform.parent = GameManager.Instance.Player.transform;
        }

        public void EndInteract()
        {
            _rigidbody.isKinematic = false;
            transform.parent = null;
        }
    }
}
