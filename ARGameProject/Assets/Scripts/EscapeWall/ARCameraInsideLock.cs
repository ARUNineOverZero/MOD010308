using System.Linq;
using UnityEngine;

namespace EscapeWall
{
    public class ARCameraInsideLock : MonoBehaviour
    {
        [Header("References")]
        public Camera arCamera;
        [SerializeField] private GameObject _worldRoot;
        [SerializeField] private Transform _arrow;
        [SerializeField] private Transform _ball;

        [Header("Layers")]
        [SerializeField] private string _hiddenLayerName = "WorldHidden";
        [SerializeField] private LayerMask _layersToIgnore; 

        private int _hiddenLayer;
        private int _originalLayer;

        [Header("Sphere Check")]
        public float sphereRadius = 0.05f;

        [Header("Ball Screen Size")]
        [SerializeField] private float _minScreenSize = 40f;  
        [SerializeField] private float _maxScreenSize = 200f; 
        [SerializeField] private float _referenceDistance = 1.5f;
        [SerializeField] private float _baseBallScale = 0.1f;

        [Header("Filtering")]
        public LayerMask blockingLayers = ~0;

        [Header("Arrow")]
        [SerializeField] private float _arrowDistance = 0.4f;   

        private Vector3 _lockPosition;
        private bool _isLocked;
        private Vector3 _previousPosition;

        private void Start()
        {
            if (arCamera == null)
                arCamera = Camera.main;

            _previousPosition = arCamera.transform.position;

            if (_arrow != null)
                _arrow.gameObject.SetActive(false);

            if (_ball != null)
                _ball.gameObject.SetActive(false);

            _hiddenLayer = LayerMask.NameToLayer(_hiddenLayerName);
            if (_worldRoot != null)
                _originalLayer = _worldRoot.layer;
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
                UpdateIndicators();

            _previousPosition = currentPos;
        }

        private void UpdateIndicators()
        {
            bool ballOnScreen = false;

            if (_ball != null)
            {
                _ball.gameObject.SetActive(true);
                _ball.position = _lockPosition;

                Vector3 screenPoint = arCamera.WorldToScreenPoint(_lockPosition);

                if (screenPoint.z > 0f)
                {
                    ballOnScreen = screenPoint.x > 0f && screenPoint.x < Screen.width
                                   && screenPoint.y > 0f && screenPoint.y < Screen.height;

                    float distance = Vector3.Distance(arCamera.transform.position, _lockPosition);
                    float t = Mathf.Clamp01(_referenceDistance / Mathf.Max(distance, 0.01f));
                    float targetScreenSize = Mathf.Lerp(_minScreenSize, _maxScreenSize, t);

                    float worldScale = targetScreenSize * distance / (arCamera.pixelHeight * 0.5f) * Mathf.Tan(arCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                    worldScale = Mathf.Max(worldScale, 0.01f);

                    _ball.localScale = Vector3.one * worldScale;
                }
                else
                    _ball.localScale = Vector3.one * _baseBallScale;
                
            }

            if (_arrow != null)
            {
                _arrow.gameObject.SetActive(!ballOnScreen);

                if (!ballOnScreen)
                {
                    _arrow.position = arCamera.transform.position + arCamera.transform.forward * _arrowDistance;

                    Vector3 lookDir = _lockPosition - _arrow.position;
                    if (lookDir.sqrMagnitude > 0.001f)
                        _arrow.rotation = Quaternion.LookRotation(lookDir);
                }
            }
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

            if (_worldRoot != null)
                SetLayerRecursive(_worldRoot, locked ? _hiddenLayer : _originalLayer);


            if (!locked)
            {
                if (_arrow != null)
                    _arrow.gameObject.SetActive(false);

                if (_ball != null)
                    _ball.gameObject.SetActive(false);
            }

            Debug.Log(locked ? "Screen Lock" : "Screen Unlock");
        }

        private void SetLayerRecursive(GameObject obj, int layer)
        {
            if ((_layersToIgnore.value & (1 << obj.layer)) == 0)
                obj.layer = layer;

            foreach (Transform child in obj.transform)
                SetLayerRecursive(child.gameObject, layer);
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