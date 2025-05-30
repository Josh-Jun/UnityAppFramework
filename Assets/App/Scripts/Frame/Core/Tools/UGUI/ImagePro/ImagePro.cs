/* *
 * ===============================================
 * author      : Josh@macbook
 * e-mail      : shijun_z@163.com
 * create time : 2025年3月17 16:44
 * function    :
 * ===============================================
 * */

using System;

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
                if (!_shader)
                {
                    _shader = Shader.Find($"UI/RoundedRectangle");
                }

                if (!_material)
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