using UnityEngine;

namespace EscapeWall.Items
{
    public class Rotator : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private Vector3 _axis = Vector3.up;
    [SerializeField] private float _rotationsPerSecond = 0.25f;

    [Header("Options")]
    [SerializeField] private bool _useLocalSpace = true;

    private void Update()
    {
        float degreesPerSecond = _rotationsPerSecond * 360f;
        transform.Rotate(_axis.normalized * degreesPerSecond * Time.deltaTime, _useLocalSpace ? Space.Self : Space.World);
    }
}
}