using System.Collections;
using System.Data;
using Unity.VisualScripting.InputSystem;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace EscapeWall
{
    public interface ITapable
    {
        void Tap();
    }

    public class TapToMove : MonoBehaviour, ITapable
    {
        public float speed;
        
        private Coroutine _moveRoutine;

        public void Tap()
        {
            if (_moveRoutine == null)
                StartCoroutine(Move(-transform.localPosition.z));
        }

        private IEnumerator Move(float target)
        {
            float current = 0;
            float start = transform.localPosition.z;
            var position = transform.localPosition;

            while (current <= 1)
            {
                current += speed * Time.deltaTime;

                if (current > 1)
                    current = 1;

                position.z = Mathf.Lerp(start, target, current);
                transform.localPosition = position;
                yield return null;
            }
        }
    }
}
