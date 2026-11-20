using UnityEngine;

namespace EscapeWall.PlayerComponents.Interaction
{
    public interface IInteractable 
    {
        public InteractionState State {get;}
        void BeginInteract();
        void BeginInteract(Vector3 screenPosition, float radius);
        void EndInteraction();
    }

    public enum InteractionState
    {
        None, 
        Began,
        Ended
    }
}