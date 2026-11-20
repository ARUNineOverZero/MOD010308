namespace EscapeWall.PlayerComponents.Interaction
{
    public interface IInteractable 
    {
        public InteractionState State {get;}
        void BeginInteract();
        void EndInteraction();
    }

    public enum InteractionState
    {
        None, 
        Began,
        Ended
    }
}