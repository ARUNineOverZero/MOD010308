using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

namespace EscapeWall
{
    public class GameManager : MonoBehaviour
    {
        #region Static 

        private static GameManager _instance;
        public static GameManager Instance => _instance;

        public static void StartInstance(GameManager instance)
        {
            if (_instance != null)
                Destroy(instance.gameObject);
            else
                _instance = instance;

        }

        #endregion Static

        [SerializeField] private ARSession _arSession;
        public ARSession ArSession => _arSession;
        [SerializeField] private XROrigin _xrOrigin;
        public XROrigin XrOrigin => _xrOrigin;
        [SerializeField] private Camera _arCamera;
        [SerializeField] private ARCameraInsideLock _arCameraInsideLock;
        public Camera ArCamera => _arCamera;
        [SerializeField] private ARPlaneManager _planeManager;
        public ARPlaneManager PlaneManager => _planeManager;
        [SerializeField] private ARRaycastManager _raycastManager;
        public ARRaycastManager RaycastManager => _raycastManager;
        [SerializeField] private ARAnchorManager _anchorManager;
        public ARAnchorManager AnchorManager => _anchorManager;

        [SerializeField] private ARTrackedImageManager _trackedImageManager;
        public ARTrackedImageManager TrackedImageManager => _trackedImageManager;

        [SerializeField] private InteractionManager _interactionManager;
        public InteractionManager InteractionManager => _interactionManager;

        [SerializeField] private GameObject _worldRoot;
        public GameObject WorldRoot => _worldRoot;
        public bool isCheckingSideOfWall = true;
        private Side _cameraSideOfWall = Side.None;
        public Side CameraSideOfWall => _cameraSideOfWall;
        public UnityEvent<string> onSideOfWallChanged;

        private void Awake() => StartInstance(this);

        public void Update() => CheckSideOfWall();

        private void CheckSideOfWall()
        {
            if (!isCheckingSideOfWall) return;
            var side = GetSideOfTheWall(_arCamera.transform.position);

            if (side == _cameraSideOfWall) return;

            _cameraSideOfWall = side;
            onSideOfWallChanged?.Invoke(_cameraSideOfWall.ToString());
        }

        public Side GetSideOfTheWall(Vector3 worldPosition)
        {
            if (!_worldRoot.activeSelf) return Side.None;

            var t = _worldRoot.transform;
            var localPosition = t.InverseTransformPoint(worldPosition);
            return localPosition.z == 0
                ? Side.None
                : localPosition.z > 0
                    ? Side.Front
                    : Side.Back;
        }

        public void SetPlaneVisibility(bool visible)
        {
            _planeManager.enabled = visible;

            foreach (var plane in _planeManager.trackables)
                plane.gameObject.SetActive(visible);
        }

        public void SetCameraLockPosition(Vector3 lockPosition) => _arCameraInsideLock.SetLockPosition(lockPosition);
    }

    public enum Side
    {
        None,
        Back,
        Front
    }
}