using System.Collections.Generic;
using EscapeWall.Items;
using UnityEditor;
using UnityEngine;

namespace EscapeWallEditor.Animation
{
    public static class TransformAnimationPreview
    {
        private class State
        {
            public Transform transform;
            public TransformAnimation.KeyPoint[] keys;
            public bool loop;
            public bool pingPong;
            public int segment;
            public float elapsed;
            public bool reversing;
        }

        private static readonly Dictionary<string, State> States = new();
        private static bool Hooked;

        public static bool IsPlaying(string path) => States.ContainsKey(path);

        public static void Play(string path, Transform transform, TransformAnimation.KeyPoint[] keys, bool loop, bool pingPong)
        {
            if (keys == null || keys.Length < 2 || transform == null) return;

            States[path] = new State
            {
                transform = transform,
                keys = keys,
                loop = loop,
                pingPong = pingPong,
                segment = 0,
                elapsed = 0f,
                reversing = false
            };

            if (!Hooked)
            {
                EditorApplication.update += Tick;
                Hooked = true;
            }
        }

        public static void Stop(string path)
        {
            States.Remove(path);
            if (States.Count == 0 && Hooked)
            {
                EditorApplication.update -= Tick;
                Hooked = false;
            }
        }

        private static void Tick()
        {
            if (States.Count == 0) return;

            var toRemove = new List<string>();

            foreach (var pair in States)
            {
                var path = pair.Key;
                var s = pair.Value;

                if (s.transform == null)
                {
                    toRemove.Add(path);
                    continue;
                }

                var from = s.reversing ? s.keys[s.segment + 1] : s.keys[s.segment];
                var to = s.reversing ? s.keys[s.segment] : s.keys[s.segment + 1];

                float duration = Mathf.Max(0.01f, to.duration);
                s.elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(s.elapsed / duration);
                t = Mathf.SmoothStep(0f, 1f, t);

                s.transform.localPosition = Vector3.Lerp(from.localPosition, to.localPosition, t);
                s.transform.localRotation = Quaternion.Slerp(from.localRotation, to.localRotation, t);

                if (s.elapsed >= duration)
                {
                    s.elapsed = 0f;

                    if (!s.reversing)
                    {
                        s.segment++;
                        if (s.segment >= s.keys.Length - 1)
                        {
                            if (s.pingPong)
                            {
                                s.reversing = true;
                                s.segment = s.keys.Length - 2;
                            }
                            else if (s.loop)
                            {
                                s.segment = 0;
                            }
                            else
                            {
                                toRemove.Add(path);
                            }
                        }
                    }
                    else
                    {
                        s.segment--;
                        if (s.segment < 0)
                        {
                            if (s.loop)
                            {
                                s.reversing = false;
                                s.segment = 0;
                            }
                            else
                            {
                                toRemove.Add(path);
                            }
                        }
                    }
                }
            }

            foreach (var path in toRemove)
                States.Remove(path);

            if (States.Count == 0 && Hooked)
            {
                EditorApplication.update -= Tick;
                Hooked = false;
            }

            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.Repaint();
        }
    }
}