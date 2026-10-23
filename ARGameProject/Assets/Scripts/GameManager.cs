using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

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

    [SerializeField] private  ARSession _arSession;
    public ARSession ArSession => _arSession;
    [SerializeField] private  XROrigin _xrOrigin;
     public XROrigin XrOrigin => _xrOrigin;
    [SerializeField] private  Camera _arCamera;
     public Camera ArCamera => _arCamera;
    [SerializeField] private  ARPlaneManager _planeManager;
     public ARPlaneManager PlaneManager => _planeManager;
    [SerializeField] private  ARRaycastManager _raycastManager;
     public ARRaycastManager RaycastManager => _raycastManager;
    [SerializeField] private  ARAnchorManager _anchorManager;
     public ARAnchorManager AnchorManager => _anchorManager;

    [SerializeField] private GameObject _worldRoot;
     public GameObject WorldRoot => _worldRoot;

     private Side _cameraSideOfWall = Side.None;
     private Side CameraSideOfWall => _cameraSideOfWall;

    private void Awake()
    {
        StartInstance(this);
    }

    public void Update()
    {
        CheckSideOfWall();
    }

    private void CheckSideOfWall()
    {
        var side = GetSideOfTheWall(_arCamera.transform.position);

        if(side == _cameraSideOfWall) return;

        _cameraSideOfWall = side;
        Debug.Log($"Side of wall has changed: {side}");
    }

    public Side GetSideOfTheWall(Vector3 worldPosition)
    {
        if(!_worldRoot.activeSelf) return Side.None;

        var t = _worldRoot.transform;
        var localPosition = t.InverseTransformPoint(worldPosition);
        return localPosition.z == 0 
            ? Side.None
            : localPosition.z > 0
                ? Side.Front
                : Side.Back;
    }
}

public enum Side
{
    None,
    Back,
    Front
}
