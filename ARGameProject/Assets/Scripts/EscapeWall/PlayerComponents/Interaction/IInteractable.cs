using UnityEngine;

namespace EscapeWall.PlayerComponents.Interaction
{
    public interface IInteractable
    {
        void BeginInteract();
        void EndInteract();
    }

    public class InteractionManager : MonoBehaviour
    {
        
    }
}