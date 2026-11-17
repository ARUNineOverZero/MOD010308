using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace EscapeWall
{
    public class TwoPointWallPlacer : MonoBehaviour
    {
        private float _groundHeight;
        private Vector3 _firstPoint;
        private Vector3 _secondPoint;

        private ARRaycastManager _raycastManager;
        [SerializeField] private Camera _arCamera;
        [SerializeField] private GameObject _worldRoot;

        [Header("Prefabs")]
        [SerializeField] private GameObject _pointMarkerPrefab;

        private readonly List<ARRaycastHit> _hits = new();
        private readonly List<GameObject> _tempMarkers = new();

        private PlacementStage _stage = PlacementStage.NotSet;
        private Coroutine _placementRoutine;

        private GameObject _previewPlane;
        private const float MinSize = 3f;

        private enum PlacementStage
        {
            NotSet = -1,
            GetGround = 0,
            GetFirstPoint = 1,
            GetSecondPoint = 2,
            StageSet = 3
        }

        private void Awake()
        {
            if (_raycastManager == null)
                _raycastManager = GameManager.Instance.RaycastManager;

            if (_arCamera == null)
                _arCamera = GameManager.Instance.ArCamera;

            if (_worldRoot != null)
                _worldRoot.SetActive(false);

            BeginPlacement();
        }

        public void BeginPlacement()
        {
            CancelPlacement();

            _stage = PlacementStage.GetGround;
            if (_worldRoot != null)
                _worldRoot.SetActive(false);

            ClearMarkers();
            DestroyPreview();
            GameManager.Instance.SetPlaneVisibility(true);
            _placementRoutine = StartCoroutine(PlacementRoutine());
        }

        public void CancelPlacement()
        {
            if (_placementRoutine != null)
            {
                StopCoroutine(_placementRoutine);
                _placementRoutine = null;
            }

            _stage = PlacementStage.NotSet;
            ClearMarkers();
            DestroyPreview();
        }

        private IEnumerator PlacementRoutine()
        {
            while (_stage != PlacementStage.StageSet && _stage != PlacementStage.NotSet)
            {
                switch (_stage)
                {
                    case PlacementStage.GetGround:
                        UpdateGroundStage();
                        break;
                    case PlacementStage.GetFirstPoint:
                        UpdateFirstPointStage();
                        break;
                    case PlacementStage.GetSecondPoint:
                        UpdateSecondPointStage();
                        break;
                }

                yield return null;
            }

            if (_stage == PlacementStage.StageSet)
                FinalizePlacement();

            _placementRoutine = null;
        }

        private bool TryGetRaycastHitPosition(out Vector3 worldPosition)
        {
            worldPosition = default;

            if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame)
                return false;

            if (_raycastManager == null)
                return false;

            Vector2 screenPosition = Pointer.current.position.ReadValue();

            if (_raycastManager.Raycast(screenPosition, _hits, TrackableType.PlaneWithinPolygon))
            {
                worldPosition = _hits[0].pose.position;
                return true;
            }

            return false;
        }

        private bool TryGetCurrentPointerHit(out Vector3 worldPosition)
        {
            worldPosition = default;

            if (Pointer.current == null || _raycastManager == null)
                return false;

            Vector2 screenPosition = Pointer.current.position.ReadValue();

            if (_raycastManager.Raycast(screenPosition, _hits, TrackableType.PlaneWithinPolygon))
            {
                worldPosition = _hits[0].pose.position;
                return true;
            }

            return false;
        }

        private void UpdateGroundStage()
        {
            if (TryGetRaycastHitPosition(out Vector3 hitPosition))
            {
                _groundHeight = hitPosition.y;
                _stage = PlacementStage.GetFirstPoint;
            }
        }

        private void UpdateFirstPointStage()
        {
            if (TryGetRaycastHitPosition(out Vector3 hitPosition))
            {
                hitPosition.y = _groundHeight;
                _firstPoint = hitPosition;
                SpawnMarker(_firstPoint);
                _stage = PlacementStage.GetSecondPoint;
            }
        }

        private void UpdateSecondPointStage()
        {
            if (TryGetCurrentPointerHit(out Vector3 currentHit))
            {
                currentHit.y = _groundHeight;
                UpdatePreviewPlane(_firstPoint, currentHit);
            }

            if (TryGetRaycastHitPosition(out Vector3 hitPosition))
            {
                hitPosition.y = _groundHeight;
                _secondPoint = hitPosition;
                SpawnMarker(_secondPoint);
                UpdatePreviewPlane(_firstPoint, _secondPoint);
                _stage = PlacementStage.StageSet;
            }
        }

        private void UpdatePreviewPlane(Vector3 pointA, Vector3 pointB)
        {
            if (_previewPlane == null)
            {
                _previewPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                _previewPlane.name = "AreaPreview";
                Destroy(_previewPlane.GetComponent<Collider>());
            }

            float width = Mathf.Abs(pointA.x - pointB.x);
            float length = Mathf.Abs(pointA.z - pointB.z);

            Vector3 center = new Vector3(
                (pointA.x + pointB.x) * 0.5f,
                _groundHeight + 0.01f,
                (pointA.z + pointB.z) * 0.5f
            );

            _previewPlane.transform.position = center;
            _previewPlane.transform.rotation = Quaternion.identity;
            _previewPlane.transform.localScale = new Vector3(width / 10f, 1f, length / 10f);

            bool tooSmall = width < MinSize || length < MinSize;
            Color color = tooSmall ? Color.red : Color.green;
            color.a = 0.45f;

            var renderer = _previewPlane.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.material.color = color;
        }

        private void FinalizePlacement()
        {
            ClearMarkers();
            DestroyPreview();

            if (_worldRoot == null)
                return;

            float width = Mathf.Abs(_firstPoint.x - _secondPoint.x);
            float length = Mathf.Abs(_firstPoint.z - _secondPoint.z);

            Vector3 center = new Vector3(
                (_firstPoint.x + _secondPoint.x) * 0.5f,
                _groundHeight,
                (_firstPoint.z + _secondPoint.z) * 0.5f
            );

            Vector3 faceDirection = _firstPoint - center;
            faceDirection.y = 0f;
            if (faceDirection.sqrMagnitude < 0.001f)
                faceDirection = Vector3.forward;

            Quaternion rotation = Quaternion.LookRotation(faceDirection.normalized, Vector3.up);

            _worldRoot.transform.position = center;
            _worldRoot.transform.rotation = rotation;
            _worldRoot.SetActive(true);
            GameManager.Instance.SetPlaneVisibility(false);
        }

        private void SpawnMarker(Vector3 position)
        {
            if (_pointMarkerPrefab == null)
                return;

            GameObject marker = Instantiate(_pointMarkerPrefab, position, Quaternion.identity);
            _tempMarkers.Add(marker);
        }

        private void ClearMarkers()
        {
            foreach (GameObject marker in _tempMarkers)
                if (marker != null)
                    Destroy(marker);

            _tempMarkers.Clear();
        }

        private void DestroyPreview()
        {
            if (_previewPlane != null)
            {
                Destroy(_previewPlane);
                _previewPlane = null;
            }
        }
    }
}