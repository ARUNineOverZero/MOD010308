using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
 
public class TapToPlace : MonoBehaviour
{
    [Tooltip("The AR Raycast Manager on the XR Origin.")]
    public ARRaycastManager raycastManager;
 
    [Tooltip("The model to place. It is hidden until you tap.")]
    public Transform model;
    
    private readonly List<ARRaycastHit> _hits = new List<ARRaycastHit>();

    void Start() => model.gameObject.SetActive(false);

    void Update()
    {
        if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame)
            return;
 
        Vector2 screenPosition = Pointer.current.position.ReadValue();
 
        if (raycastManager.Raycast(screenPosition, _hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = _hits[0].pose;
            model.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            model.gameObject.SetActive(true);
        }
    }
}
