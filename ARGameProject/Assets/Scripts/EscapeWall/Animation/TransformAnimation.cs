using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeWall.Items
{
    [System.Serializable]
    public class TransformAnimation
    {
        [System.Serializable]
        public struct KeyPoint
        {
            public Vector3 localPosition;
            public Quaternion localRotation;
            public float duration;

            public KeyPoint(Vector3 pos, Quaternion rot, float dur = 1f)
            {
                localPosition = pos;
                localRotation = rot;
                duration = dur;
            }
        }

        public Transform target;

        public List<KeyPoint> keys = new();
        public bool loop;
        public bool pingPong;

        public IEnumerator Play(Transform target = null, Action callback = null)
        {
            if(target == null)
            {
                target = this.target;
                if(target == null)
                    yield break;
            }

            if (keys == null || keys.Count < 2)
                yield break;

            do
            {
                for (int i = 0; i < keys.Count - 1; i++)
                    yield return TweenBetween(target, keys[i], keys[i + 1]);


                if (pingPong && keys.Count > 2)
                    for (int i = keys.Count - 2; i >= 0; i--)
                        yield return TweenBetween(target, keys[i + 1], keys[i]);
            }
            while (loop);

            callback?.Invoke();
        }

        private IEnumerator TweenBetween(Transform target, KeyPoint from, KeyPoint to)
        {
            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, to.duration);

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                t = Mathf.Lerp(0f, 1f, t);

                target.localPosition = Vector3.Lerp(from.localPosition, to.localPosition, t);
                target.localRotation = Quaternion.Slerp(from.localRotation, to.localRotation, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            target.localPosition = to.localPosition;
            target.localRotation = to.localRotation;
        }
    }
}
