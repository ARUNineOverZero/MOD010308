using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeWall
{
    public class ARCameraInsideLock : MonoBehaviour
    {
        [Header("References")]
        public Camera arCamera;
        public GameObject blackScreen;

        [Header("Circle Indicator")]
        [SerializeField] private RectTransform _circle;
        [SerializeField] private float _minCircleScale = 0.2f;
        [SerializeField] private float _maxCircleScale = 2.5f;
        [SerializeField] private float _referenceDistance = 1.5f;

        [Header("Sphere Check")]
        public float sphereRadius = 0.05f;

        [Header("Filtering")]
        public LayerMask blockingLayers = ~0;

        private Vector3 _lockPosition;
        private bool _isLocked;
        private Vector3 _previousPosition;

        private void Start()
        {
            if (arCamera == null)
                arCamera = Camera.main;

            _previousPosition = arCamera.transform.position;

            if (blackScreen != null)
                blackScreen.SetActive(false);

            if (_circle != null)
                _circle.gameObject.SetActive(false);
        }

        private void Update()
        {
            Vector3 currentPos = arCamera.transform.position;

            bool sphereOverlapping = IsSphereOverlapping(currentPos, out Collider[] currentHits) && !CollidersContainARCameraInsideLockIgnore(currentHits);
            bool wasOverlapping = IsSphereOverlapping(_previousPosition, out Collider[] previousHits) && !CollidersContainARCameraInsideLockIgnore(previousHits);

            if (!_isLocked && sphereOverlapping && !wasOverlapping)
            {
                _lockPosition = _previousPosition;
                SetLock(true);
            }

            if (_isLocked && !sphereOverlapping && HasLineOfSight(currentPos))
                SetLock(false);

            if (_isLocked)
                UpdateCircle();

            _previousPosition = currentPos;
        }

        private void UpdateCircle()
        {
            if (_circle == null || arCamera == null)
                return;

            Vector3 screenPoint = arCamera.WorldToScreenPoint(_lockPosition);

            if (screenPoint.z <= 0f)
            {
                _circle.gameObject.SetActive(false);
                return;
            }

            _circle.gameObject.SetActive(true);
            _circle.position = screenPoint;

            float distance = Vector3.Distance(arCamera.transform.position, _lockPosition);
            float t = Mathf.Clamp01(_referenceDistance / Mathf.Max(distance, 0.01f));
            float scale = Mathf.Lerp(_minCircleScale, _maxCircleScale, t);

            _circle.localScale = Vector3.one * scale;
        }

        private bool IsSphereOverlapping(Vector3 center, out Collider[] hits)
        {
            hits = Physics.OverlapSphere(center, sphereRadius, blockingLayers);
            return hits.Length > 0;
        }

        private bool CollidersContainARCameraInsideLockIgnore(Collider[] hits)
        {
            return hits.Any(x => x.gameObject.tag == "ARCameraLockIgnore");
        }

        private bool HasLineOfSight(Vector3 from)
        {
            Vector3 dir = _lockPosition - from;
            float dist = dir.magnitude;

            if (dist < 0.01f)
                return true;

            return !Physics.Raycast(from, dir.normalized, dist, blockingLayers);
        }

        private void SetLock(bool locked)
        {
            _isLocked = locked;

            if (blackScreen != null)
                blackScreen.SetActive(locked);

            if (_circle != null)
                _circle.gameObject.SetActive(locked);

            Debug.Log(locked ? "Screen Lock" : "Screen Unlock");
        }

        public void SetLockPosition(Vector3 position)
        {
            _lockPosition = position;
            SetLock(true);
        }

        [ContextMenu("Unlock")]
        private void Unlock() => SetLock(false);

        private void OnDrawGizmosSelected()
        {
            if (arCamera == null) return;
            Gizmos.color = _isLocked ? Color.red : Color.green;
            Gizmos.DrawWireSphere(arCamera.transform.position, sphereRadius);
        }
    }
}