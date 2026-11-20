using UnityEngine;
using UnityEngine.Events;

namespace EscapeWall
{
    public class UnhideObjectAfterEntering : MonoBehaviour
    {
        [SerializeField] private GameObject _object;
        [SerializeField] private Transform _arCamera;

        [Header("Look Direction")]
        [SerializeField, Range(0f, 1f)] private float _forwardDotThreshold = 0.65f;

        public UnityEvent OnPassLine;
        public UnityEvent OnHide;

        private void Update()
        {
            if (_arCamera == null || _object == null)
                return;

            Vector3 localPosition = transform.InverseTransformPoint(_arCamera.position);

            bool inZone = Mathf.Abs(localPosition.x) < 1.25f && localPosition.z >= 0f;
            if(inZone)
                OnPassLine?.Invoke();

            bool lookingSameDirection = Vector3.Dot(_arCamera.forward, transform.forward) >= _forwardDotThreshold;

            if (inZone && lookingSameDirection)
            {
                OnHide?.Invoke();
                _object.SetActive(true);
                Destroy(gameObject);
            }
        }
    }
}