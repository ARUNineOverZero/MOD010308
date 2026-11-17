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
        private SquareArea _area;
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
            GetFirstPoint = 0,
            GetSecondPoint = 1,
            StageSet = 2
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

            _stage = PlacementStage.GetFirstPoint;
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

        private void UpdateFirstPointStage()
        {
            if (TryGetRaycastHitPosition(out Vector3 hitPosition))
            {
                _area.pointA = hitPosition;
                SpawnMarker(_area.pointA);
                _stage = PlacementStage.GetSecondPoint;
            }
        }

        private void UpdateSecondPointStage()
        {
            var cameraPosition = _arCamera.transform.position;
            cameraPosition.y = _area.pointA.y;
            _area.pointB = cameraPosition;
            UpdatePreviewPlane();

            if (Pointer.current != null & Pointer.current.press.wasPressedThisFrame)
            {
                UpdatePreviewPlane();
                _stage = PlacementStage.StageSet;
            }
        }

        private void UpdatePreviewPlane()
        {
            if (!_area.IsValid)
                return;

            if (_previewPlane == null)
            {
                _previewPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                _previewPlane.name = "AreaPreview";
                Destroy(_previewPlane.GetComponent<Collider>());
            }

            _previewPlane.transform.position = _area.Center;
            _previewPlane.transform.rotation = _area.Rotation;
            _previewPlane.transform.localScale = _area.PreviewScale;

            Color color = _area.IsTooSmall(MinSize) ? Color.red : Color.green;
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

            Vector3 center = _area.Center;
            Quaternion rotation = Quaternion.LookRotation(_area.Forwards, Vector3.up);

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

    public struct SquareArea
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public Vector3 Diagonal
        {
            get
            {
                Vector3 d = pointA - pointB;
                d.y = 0f;
                return d;
            }
        }

        public float DiagonalLength => Diagonal.magnitude;

        public float SideLength => DiagonalLength / Mathf.Sqrt(2f);

        public Vector3 Center
        {
            get
            {
                Vector3 c = (pointA + pointB) * 0.5f;
                c.y = pointA.y + 0.01f;
                return c;
            }
        }

        public Vector3 Forwards
        {
            get
            {
                if (DiagonalLength < 0.001f)
                    return Vector3.forward;

                Vector3 forwards = Quaternion.Euler(0f, 45f, 0f) * Diagonal.normalized;
                forwards.y = 0f;
                return forwards.normalized;
            }
        }

        public Quaternion Rotation => Quaternion.LookRotation(Forwards, Vector3.up);

        public Vector3 PreviewScale => new Vector3(SideLength / 10f, 1f, SideLength / 10f);

        public bool IsTooSmall(float minSize) => SideLength < minSize;

        public bool IsValid => DiagonalLength >= 0.001f;
    }
}