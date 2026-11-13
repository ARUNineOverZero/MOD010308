using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;


namespace EscapeWall.PaperMarker
{
    public class PaperMarkerCapture : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager _trackedImageManager;
    [SerializeField] private ARCameraManager _cameraManager;
    [SerializeField] private float _paperWidthInMeters = 0.15f;

    private MutableRuntimeReferenceImageLibrary mutableLibrary;

    private void Awake() => _trackedImageManager.enabled = false;

    private void Start()
    {
        var runtimeLib = _trackedImageManager.CreateRuntimeLibrary();
        mutableLibrary = runtimeLib as MutableRuntimeReferenceImageLibrary;
        _trackedImageManager.referenceLibrary = mutableLibrary;

        _trackedImageManager.enabled = true;
        Debug.Log("AR Tracked Image Manager enabled with mutable library.");
    }

    public void CaptureAndAddMarker()
    {
        StartCoroutine(CaptureCoroutine());
    }

    private IEnumerator CaptureCoroutine()
    {
        if (!_cameraManager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage))
        {
            Debug.LogWarning("Could not acquire camera image.");
            yield break;
        }

        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),
            outputDimensions = new Vector2Int(cpuImage.width, cpuImage.height),
            outputFormat = TextureFormat.RGBA32,
            transformation = XRCpuImage.Transformation.MirrorY
        };

        int size = cpuImage.GetConvertedDataSize(conversionParams);
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        cpuImage.Convert(conversionParams, buffer);
        cpuImage.Dispose();

        var texture = new Texture2D(conversionParams.outputDimensions.x, conversionParams.outputDimensions.y, TextureFormat.RGBA32, false);
        texture.LoadRawTextureData(buffer);
        texture.Apply();
        buffer.Dispose();

        if (mutableLibrary != null)
        {
            mutableLibrary.ScheduleAddImageWithValidationJob(
                texture,
                "PlayerDrawnMarker",
                _paperWidthInMeters
            );
            Debug.Log("Marker added. Point the camera at the paper to track it.");
        }

        Destroy(texture, 2f);
    }
}
}
