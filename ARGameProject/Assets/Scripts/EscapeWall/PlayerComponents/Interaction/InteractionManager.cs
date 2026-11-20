using System;
using UnityEngine;

namespace EscapeWall.PlayerComponents.Interaction
{
    public class InteractionManager : MonoBehaviour
    {
        private IInteractable _currentInteraction;
        public IInteractable CurrentInteraction => _currentInteraction;

        public void BeginInteraction(IInteractable interactable)
        {
            if(interactable == null) return;

            interactable.BeginInteract();
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