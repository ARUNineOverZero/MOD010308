using System;
using EscapeWall.PlayerComponents.Interaction;
using UnityEngine;

namespace EscapeWall.Items
{
    public class Padlock : MonoBehaviour, IInteractable
    {
        [SerializeField] private PadlockLoop _padlockLoop;
        private bool _isLocked = true;

        public void BeginInteract()
        {
            PlayUnlock();
        }

        public void EndInteract()
        {
            Drop();
        }

        public void UseKey(Transform key)
        {
            

        }
        private void PlayIncorrectKey(Transform key)
        {
            
        }

        private void PlayCorrectKey(Transform key)
        {
            
        }

        private void PlayUnlock()
        {
            _padlockLoop.Play();
        }

        private void Drop()
        {
            _padlockLoop.Stop();
        }
    }

    public class PadlockLoop : MonoBehaviour
    {
        [SerializeField] public Padlock _Padlock; 
        public TransformAnimation animation;

        private Coroutine _playCoroutine;

        public void Play(Action callback = null)
        {
            if(_playCoroutine != null) return;

            _playCoroutine = StartCoroutine(animation.Play(transform, () => Stop(callback)));
        }

        public void Stop(Action callback = null)
        {
            StopCoroutine(_playCoroutine);
            _playCoroutine = null;
            callback?.Invoke();
        }
    }
}
