using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;



namespace EscapeWall.PaperMarker
{
    public class PaperObjectController : MonoBehaviour
    {
        [SerializeField] private ARTrackedImageManager _trackedImageManager;
        [SerializeField] private Transform _controlledObject;

        private void OnEnable()
        {
            _trackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
        }

        private void OnDisable()
        {
            _trackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
        }

        private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
        {
            foreach (var trackedImage in args.added)
                HandleTrackedImage(trackedImage);

            foreach (var trackedImage in args.updated)
                HandleTrackedImage(trackedImage);

        }

        private void HandleTrackedImage(ARTrackedImage trackedImage)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                _controlledObject.SetParent(trackedImage.transform, false);
                _controlledObject.localPosition = Vector3.zero;
                _controlledObject.localRotation = Quaternion.identity;
                _controlledObject.gameObject.SetActive(true);
            }
            else
                _controlledObject.gameObject.SetActive(false);

        }
    }
}
