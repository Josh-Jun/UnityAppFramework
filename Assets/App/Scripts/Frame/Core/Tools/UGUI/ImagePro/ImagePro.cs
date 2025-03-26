/* *
 * ===============================================
 * author      : Josh@macbook
 * e-mail      : shijun_z@163.com
 * create time : 2025年3月17 16:44
 * function    : 
 * ===============================================
 * */

namespace UnityEngine.UI
{
    public class ImagePro : Image
    {
        [SerializeField] [Range(0, 1)] private float _Radius;

        public float Radius
        {
            get => _Radius;
            set
            {
                _Radius = value;
                Refresh();
            }
        }
        
        private static readonly int SizeID = Shader.PropertyToID("_Size");
        private static readonly int RadiusID = Shader.PropertyToID("_Radius");

        private Shader _shader = null;
        private Material _material = null;

        private Material Material
        {
            get
            {
                if (_shader == null)
                {
                    _shader = Shader.Find($"Shader Graphs/RoundedRectangle");
                }

                if (_material == null)
                {
                    _material = new Material(_shader)
                    {
                        name = "Rounded Rectangle"
                    };
                }

                return _material;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            Refresh();
        }

        protected override void UpdateMaterial()
        {
            Refresh();
            if (sprite == null)
            {
                canvasRenderer.materialCount = 1;
                canvasRenderer.SetMaterial(material, 0);
                canvasRenderer.SetTexture(Texture2D.whiteTexture);
                return;
            }
            base.UpdateMaterial();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            Refresh();
        }

        private void Refresh()
        {
            if (material != Material)
            {
                material = Material;
            }
            var maxRadius = Mathf.Min(rectTransform.rect.size.x, rectTransform.rect.size.y) / 2;
            material.SetFloat(RadiusID, Radius * maxRadius);
            material.SetVector(SizeID, rectTransform.rect.size);
        }
    }
}