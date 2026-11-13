using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace EscapeWall
{
    public class WallPlacer : MonoBehaviour
    {
        private ARRaycastManager _raycastManager;
        public Camera _arCamera;

        [SerializeField] private GameObject _worldRoot;

        private readonly List<ARRaycastHit> _hits = new List<ARRaycastHit>();

        private void Awake()
        {
            _worldRoot.gameObject.SetActive(false);
            _raycastManager = GameManager.Instance.RaycastManager;
            _arCamera = GameManager.Instance.ArCamera;
        }

        public void Update() => PlaceWorldRoot();


        public void PlaceWorldRoot()
        {
            if (_worldRoot.activeSelf) return;

            if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame)
                return;

            Vector2 screenPosition = Pointer.current.position.ReadValue();

            if (_raycastManager.Raycast(screenPosition, _hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = _hits[0].pose;
                var position = hitPose.position;
                var t = _worldRoot.transform;

                t.SetPositionAndRotation(position, hitPose.rotation);

                t.LookAt(_arCamera.transform.position);

                var localEularAngles = t.localEulerAngles;

                t.localEulerAngles = new Vector3(0, localEularAngles.y, 0);

                _worldRoot.SetActive(true);

                GameManager.Instance.SetPlaneVisibility(false);
            }
        }
    }
}
