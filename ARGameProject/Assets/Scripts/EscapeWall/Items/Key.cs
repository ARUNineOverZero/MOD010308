namespace EscapeWall.Items
{
    using UnityEngine;

    public class Key : PhysicalItem
    {
        [SerializeField] private int _id = -1;
        public int ID => _id;
        [SerializeField] private Color _color = Color.white;

        [Tooltip("Renderer that uses the shared material")]
        [SerializeField] private Renderer _renderer;

        [Tooltip("Shader color property name")]
        [SerializeField] private string _colorProperty = "_BaseColor";

        private MaterialPropertyBlock _propertyBlock;
        private static readonly int ColorPropertyId = Shader.PropertyToID("_BaseColor");

        protected override void Awake()
        {
            base.Awake();
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
    }
}
