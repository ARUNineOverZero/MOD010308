using System;
using UnityEngine;

namespace EscapeWall.Items
{
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
