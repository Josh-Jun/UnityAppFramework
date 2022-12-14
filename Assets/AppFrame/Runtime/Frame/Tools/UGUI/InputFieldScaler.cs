using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AppFrame.Tools
{
    [RequireComponent(typeof(InputField))]
    public class InputFieldScaler : MonoBehaviour, ILayoutElement
    {
        private Text textComponent
        {
            get { return this.GetComponent<InputField>().textComponent; }
        }

        public TextGenerationSettings GetTextGenerationSettings(Vector2 extents)
        {
            var settings = textComponent.GetGenerationSettings(extents);
            settings.generateOutOfBounds = true;
            return settings;
        }

        private RectTransform m_Rect;

        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        public void OnValueChanged(string v)
        {
            if (!fixedWidth)
            {
                rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)0, LayoutUtility.GetPreferredWidth(m_Rect));
            }

            rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)1, LayoutUtility.GetPreferredHeight(m_Rect));
        }

        void OnEnable()
        {
            this.inputField.onValueChanged.AddListener(OnValueChanged);
        }

        void OnDisable()
        {

        }

        private Vector2 originalSize;
        private InputField _inputField;

        public InputField inputField
        {
            get { return _inputField ?? (_inputField = this.GetComponent<InputField>()); }
        }

        private float _offsetHeight;

        public float offsetHeight
        {
            get
            {
                if (_offsetHeight == 0)
                    _offsetHeight =
                        generatorForLayout.GetPreferredHeight(text, GetTextGenerationSettings(Vector2.zero)) /
                        textComponent.pixelsPerUnit;
                return _offsetHeight;
            }
        }

        private float _offsetTextComponentLeftRingt;

        public float offsetTextComponentLeftRingt
        {
            get
            {
                if (_offsetTextComponentLeftRingt == 0)
                    _offsetTextComponentLeftRingt =
                        Mathf.Abs(rectTransform.rect.xMin - textComponent.rectTransform.rect.xMin) +
                        Mathf.Abs(rectTransform.rect.xMax - textComponent.rectTransform.rect.xMax);
                return _offsetTextComponentLeftRingt;
            }
        }

        protected void Awake()
        {
            textComponent.fontSize = fontSize;
            inputField.placeholder.GetComponent<Text>().fontSize = fontSize;
            this.originalSize = this.GetComponent<RectTransform>().sizeDelta;
            inputField.lineType = fixedWidth ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
            rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)1, LayoutUtility.GetPreferredHeight(m_Rect));
        }

        private string text
        {
            get { return this.GetComponent<InputField>().text; }
        }

        private TextGenerator _generatorForLayout;

        public TextGenerator generatorForLayout
        {
            get { return _generatorForLayout ?? (_generatorForLayout = new TextGenerator()); }
        }

        //[Tooltip("测试用")]
        //public float Width;
        //[Tooltip("测试用")]
        //public float Height;
        public void Update()
        {
            //this.Width = this.preferredWidth;
            //this.Height = this.preferredHeight;
        }

        public float preferredWidth
        {
            get
            {
                if (fixedWidth)
                {
                    return this.originalSize.x;
                    //return Mathf.Max(this.originalSize.x, generatorForLayout.GetPreferredWidth(text, GetTextGenerationSettings(Vector2.zero)) / textComponent.pixelsPerUnit + 20);
                }
                else
                {
                    if (keepInitWidthSize)
                    {
                        return Mathf.Max(this.originalSize.x,
                            generatorForLayout.GetPreferredWidth(text, GetTextGenerationSettings(Vector2.zero)) /
                            textComponent.pixelsPerUnit + offsetTextComponentLeftRingt);
                        //return generatorForLayout.GetPreferredWidth(text, GetTextGenerationSettings(Vector2.zero)) / textComponent.pixelsPerUnit + 20;
                    }
                    else
                    {
                        return generatorForLayout.GetPreferredWidth(text, GetTextGenerationSettings(Vector2.zero)) /
                            textComponent.pixelsPerUnit + offsetTextComponentLeftRingt;
                    }
                }
            }
        }

        public virtual void CalculateLayoutInputHorizontal()
        {
        }

        public virtual void CalculateLayoutInputVertical()
        {
        }

        public virtual float minWidth
        {
            get { return -1; }
        }

        public virtual float flexibleWidth
        {
            get { return -1; }
        }

        public virtual float minHeight
        {
            get { return -1; }
        }

        public virtual float preferredHeight
        {
            get
            {
                if (fixedWidth)
                {
                    return generatorForLayout.GetPreferredHeight(text,
                        GetTextGenerationSettings(new Vector2(this.textComponent.GetPixelAdjustedRect().size.x,
                            0.0f))) / textComponent.pixelsPerUnit + offsetHeight;
                    //return Mathf.Max(this.originalSize.y, generatorForLayout.GetPreferredHeight(text, GetTextGenerationSettings(new Vector2(this.textComponent.GetPixelAdjustedRect().size.x, 0.0f))) / textComponent.pixelsPerUnit);
                }
                else
                {
                    return generatorForLayout.GetPreferredHeight(text,
                        GetTextGenerationSettings(new Vector2(this.textComponent.GetPixelAdjustedRect().size.x,
                            0.0f))) / textComponent.pixelsPerUnit + offsetHeight;
                }

            }
        }

        public virtual float flexibleHeight
        {
            get { return -1; }
        }

        //[Tooltip("输入框的字体大小，InputField的大小会随字体大小改变高度")]
        [HideInInspector] public int fontSize = 20;

        //[Tooltip("是否保持InputField的宽度不变")]
        [HideInInspector] public bool fixedWidth = true;

        //[Tooltip("是否不限制InputField的宽度")]
        [HideInInspector] public bool keepInitWidthSize = true;

        //[SerializeField]
        //[Tooltip("提高Layout计算优先级，要比InputField大 这里设为1")]
        private int priority = 1;

        public virtual int layoutPriority
        {
            get { return priority; }
        }
    }
}