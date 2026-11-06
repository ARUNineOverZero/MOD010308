using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace EscapeWall
{
    public class ARCameraInsideLock : MonoBehaviour
    {
        [Header("References")]
        public Camera arCamera;
        public GameObject blackScreen;

        [Header("Sphere Check")]
        public float sphereRadius = 0.15f;

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
        }

        private void Update()
        {
            Vector3 currentPos = arCamera.transform.position;

            bool sphereOverlapping = IsSphereOverlapping(currentPos, out Collider[] currentHits) && !CollidersContainARCameraInsideLockIgnore(currentHits);
            bool wasOverlapping    = IsSphereOverlapping(_previousPosition, out Collider[] previousHits) && !CollidersContainARCameraInsideLockIgnore(previousHits);

            if (!_isLocked && sphereOverlapping && !wasOverlapping)
            {
                _lockPosition = _previousPosition;
                SetLock(true);
            }

            if (_isLocked && !sphereOverlapping && HasLineOfSight(currentPos))
                SetLock(false);
        

            _previousPosition = currentPos;
        }

        private bool IsSphereOverlapping(Vector3 center, out Collider[] hits)
        {
            hits = Physics.OverlapSphere(center, sphereRadius, blockingLayers);
            return hits.Length > 0; 
        }

        private bool CollidersContainARCameraInsideLockIgnore(Collider[] hits)
        {
            return hits.Any(x =>
            {
                var arCameraIgnore = x.GetComponent<ARCameraInsideLockIgnore>();
                return arCameraIgnore != null;
            });
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

            Debug.Log(locked ? "Screen Lock" : "Screen Unlock");
        }

        private void OnDrawGizmosSelected()
        {
            if (arCamera == null) return;
            Gizmos.color = _isLocked ? Color.red : Color.green;
            Gizmos.DrawWireSphere(arCamera.transform.position, sphereRadius);
        }
    }
}