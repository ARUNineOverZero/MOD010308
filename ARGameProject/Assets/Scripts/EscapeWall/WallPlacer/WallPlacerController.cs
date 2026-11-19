using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EscapeWall.PlayerComponents;

namespace EscapeWall.WallPlacer
{
    public class WallPlacerController : MonoBehaviour
    {
        private SquareArea _area;
        private ARRaycastManager _raycastManager;
        [SerializeField] private Camera _arCamera;
        [SerializeField] private GameObject _worldRoot;
        [SerializeField] private PlayerInputHandler _inputHandler;

        [Header("Prefabs")]
        [SerializeField] private GameObject _pointMarkerPrefab;
        [SerializeField] private Material _planeMaterial;
        private Material _arrowMaterial;
        private readonly List<ARRaycastHit> _hits = new();
        private readonly List<GameObject> _tempMarkers = new();
        private PlacementStage _stage = PlacementStage.NotSet;
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

            if (_pointMarkerPrefab != null && _pointMarkerPrefab.TryGetComponent(out MeshRenderer mr))
                _arrowMaterial = mr.sharedMaterial;
        }

        private void OnEnable()
        {
            if (_inputHandler == null) return;

            _inputHandler.OnFingerDown += HandleFingerDown;
            _inputHandler.OnFingerMove += HandleFingerMove;
        }

        private void OnDisable()
        {
            if (_inputHandler == null) return;

            _inputHandler.OnFingerDown -= HandleFingerDown;
            _inputHandler.OnFingerMove -= HandleFingerMove;
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
        }

        public void CancelPlacement()
        {
            _stage = PlacementStage.NotSet;
            ClearMarkers();
            DestroyPreview();
        }

        private void HandleFingerDown(PlayerInputHandler.FingerData finger)
        {
            if (_stage == PlacementStage.GetFirstPoint)
            {
                if (TryGetRaycastHitPosition(finger.position, out Vector3 hitPosition))
                {
                    _area.pointA = hitPosition;
                    SpawnMarker(_area.pointA);
                    _stage = PlacementStage.GetSecondPoint;
                    SpawnMarker(_area.pointA);
                    GameManager.Instance.SetPlaneVisibility(false);
                }
            }
            else if (_stage == PlacementStage.GetSecondPoint)
            {
                UpdatePreviewPlane();
                _stage = PlacementStage.StageSet;
                FinalizePlacement();
            }
        }

        private void HandleFingerMove(PlayerInputHandler.FingerData finger)
        {
            if (_stage != PlacementStage.GetSecondPoint) return;

            var cameraPosition = _arCamera.transform.position;
            cameraPosition.y = _area.pointA.y;
            _area.pointB = cameraPosition;

            if (_tempMarkers.Count > 1)
                _tempMarkers[^1].transform.position = _area.pointB + (_area.pointB - _area.pointA).normalized * 0.5f;

            UpdatePreviewPlane();
        }

        private bool TryGetRaycastHitPosition(Vector2 screenPosition, out Vector3 worldPosition)
        {
            worldPosition = default;

            if (_raycastManager == null)
                return false;

            if (_raycastManager.Raycast(screenPosition, _hits, TrackableType.PlaneWithinPolygon))
            {
                worldPosition = _hits[0].pose.position;
                return true;
            }

            return false;
        }

        private void UpdatePreviewPlane()
        {
            if (!_area.IsValid)
                return;

            if (_previewPlane == null)
            {
                _previewPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                _previewPlane.name = "AreaPreview";
                var meshRenderer = _previewPlane.GetComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = _planeMaterial;
                Destroy(_previewPlane.GetComponent<Collider>());
            }

            _previewPlane.transform.position = _area.Center;
            _previewPlane.transform.rotation = _area.Rotation;
            _previewPlane.transform.localScale = _area.PreviewScale;

            Color color;
            if (_area.IsTooSmall(MinSize))
            {
                color = Color.red;
                color.a = 0f;
            }
            else
            {
                color = Color.green;
                color.a = 0.8f;
            }

            if (_planeMaterial != null)
                _planeMaterial.color = color;

            if (_arrowMaterial != null)
                _arrowMaterial.color = color;
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
            _stage = PlacementStage.NotSet;
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
