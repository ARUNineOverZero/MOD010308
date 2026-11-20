using System.Collections;
using EscapeWall.PlayerComponents.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeWall.Items
{
    public class Padlock : MonoBehaviour
    {
        [SerializeField] private int _keyId = -1;

        [SerializeField] private Transform _keyAnchor;

        [SerializeField] private TransformAnimation _incorrectKey;
        [SerializeField] private TransformAnimation _correctKey;
        [SerializeField] private TransformAnimation _baseUnlock;
        private bool usingKey = false;
        public UnityEvent OnUnlocked;

        [SerializeField] private Color _color = Color.white;

        [Tooltip("Renderer that uses the shared material")]
        [SerializeField] private Renderer _renderer;

        [Tooltip("Shader color property name")]
        [SerializeField] private string _colorProperty = "_BaseColor";

        private MaterialPropertyBlock _propertyBlock;
        private static readonly int ColorPropertyId = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponentInChildren<Renderer>();

            _propertyBlock = new MaterialPropertyBlock();
            ApplyColor();
        }

        private void OnValidate()
        {
            if (_renderer == null)
                _renderer = GetComponentInChildren<Renderer>();

            ApplyColor();
        }

        private void ApplyColor()
        {
            if (_renderer == null) return;

            if (_propertyBlock == null)
                _propertyBlock = new MaterialPropertyBlock();

            _renderer.GetPropertyBlock(_propertyBlock);

            int propertyId = Shader.PropertyToID(_colorProperty);
            if (_renderer.sharedMaterial != null && _renderer.sharedMaterial.HasProperty(propertyId))
                _propertyBlock.SetColor(propertyId, _color);
            else if (_renderer.sharedMaterial != null && _renderer.sharedMaterial.HasProperty("_BaseColor"))
                _propertyBlock.SetColor("_BaseColor", _color);
            else
                _propertyBlock.SetColor("_Color", _color);

            _renderer.SetPropertyBlock(_propertyBlock);
        }
        public void SetColor(Color newColor)
        {
            _color = newColor;
            ApplyColor();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (usingKey) return;

            Key key = other.gameObject.GetComponentInParent<Key>();

            if (key == null) return;

            UseKey(key);
        }

        public void UseKey(Key key)
        {
            key.EndInteraction();
            if (key.ID == _keyId)
                PlayCorrectKey(key);
            else
                PlayIncorrectKey(key);
        }
        private void PlayIncorrectKey(Key key)
        {
            usingKey = true;
            Transform t = key.transform;
            key.DisableRigidBody();
            t.parent = _keyAnchor;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;

            StartCoroutine(_incorrectKey.Play(null, () =>
            {
                var parent = GetComponentInParent<WallSide>();
                t.parent = parent?.transform;
                key.EnableRigidbody();
                StartCoroutine(RemoveKey());
            }));
        }

        private IEnumerator RemoveKey()
        {
            yield return new WaitForSeconds(1);
            usingKey = false;
        }

        private void PlayCorrectKey(Key key)
        {
            usingKey = true;
            Transform t = key.transform;
            Destroy(key.Rigidbody);
            Destroy(key);
            t.parent = _keyAnchor;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;

            StartCoroutine(_correctKey.Play(null, PlayUnlock));
        }

        private void PlayUnlock()
        {
            StartCoroutine(_baseUnlock.Play(null, Drop));
        }

        private void Drop()
        {
            PhysicalItem pi = gameObject.AddComponent<PhysicalItem>();
            pi.EnableRigidbody();
            OnUnlocked?.Invoke();

            Destroy(this);
        }
    }
}
