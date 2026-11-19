namespace EscapeWall.PlayerComponents.Interaction
{
    public interface IInteractable
    {
        public InteractionState State {get;}
        void BeginInteract();
        void EndInteract();
    }

    public enum InteractionState
    {
        None, 
        Began,
        Ended
    }
}