using System;
using UnityEngine;

namespace EscapeWall.PlayerComponents.Interaction
{
    public class InteractionManager : MonoBehaviour
    {
        private IInteractable _currentInteraction;
        public IInteractable CurrentInteraction => _currentInteraction;

        public void BeginInteraction(IInteractable interactable, Vector3 screenPosition, float radius)
        {
            if(interactable == null) return;

            interactable.BeginInteract(screenPosition, radius);
            _currentInteraction = interactable;
        }

        public void EndInteraction()
        {
            if(_currentInteraction == null || _currentInteraction.State == InteractionState.Ended) return;
            
            _currentInteraction.EndInteraction();
            _currentInteraction = null;
        }

        public void MoveInteraction()
        {
            
        }
    }
}