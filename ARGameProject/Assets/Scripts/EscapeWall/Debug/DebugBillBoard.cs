using EscapeWall;
using TMPro;
using UnityEngine;

namespace EscapeWallDebug
{
    public class DebugBillBoard : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private bool _yRotationOnly = true;
        [SerializeField] private TextMeshProUGUI _text;

        private void Awake()
        {
            if (_camera == null)
                _camera = GameManager.Instance.ArCamera;
        }

        public void Update()
        {
            if (_camera == null) return;

            Vector3 cameraPosition = _camera.transform.position;
            Transform t = transform;
            Vector3 direction = cameraPosition - t.position;
            var rotation = Quaternion.LookRotation(direction);
            if (_yRotationOnly)
            {
                float yRotation = Quaternion.LookRotation(direction).eulerAngles.y;
                t.eulerAngles = new Vector3(0f, yRotation, 0f);
            }
            else
                t.rotation = rotation;

        }

        public void UpdateText(string text) => _text.text = text;

    }
}