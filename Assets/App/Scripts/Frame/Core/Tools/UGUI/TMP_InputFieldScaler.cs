/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年6月11 13:33
 * function    : 
 * ===============================================
 * */
using TMPro;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(TMP_InputField))]
    public class TMP_InputFieldScaler : MonoBehaviour, ILayoutElement
    {
        private TMP_Text TextComponent => this.GetComponent<TMP_InputField>().textComponent;

        private RectTransform _viewRect;

        private RectTransform ViewRect => InputField.textViewport;

        private RectTransform _mRect;

        private RectTransform RectTransform
        {
            get
            {
                if (!_mRect)
                    _mRect = GetComponent<RectTransform>();
                return _mRect;
            }
        }

        private void OnValueChanged(string v)
        {
            if (!fixedWidth)
            {
                RectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)0, LayoutUtility.GetPreferredWidth(_mRect));
            }

            RectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)1, LayoutUtility.GetPreferredHeight(_mRect));
        }

        private void OnEnable()
        {
            this.InputField.onValueChanged.AddListener(OnValueChanged);
        }
        
        private float _originalWidth;
        // private float _originalHeight;
        private TMP_InputField _inputField;

        private TMP_InputField InputField => _inputField ??= this.GetComponent<TMP_InputField>();

        protected void Awake()
        {
            TextComponent.fontSize = fontSize;
            InputField.placeholder.GetComponent<TMP_Text>().fontSize = fontSize;
            this._originalWidth = this.GetComponent<RectTransform>().sizeDelta.x - Mathf.Abs(ViewRect.offsetMax.x) - Mathf.Abs(ViewRect.offsetMin.x);
            // this._originalHeight = this.GetComponent<RectTransform>().sizeDelta.y - Mathf.Abs(ViewRect.offsetMax.y) - Mathf.Abs(ViewRect.offsetMin.y);
            InputField.lineType = fixedWidth ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;
            RectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)1, LayoutUtility.GetPreferredHeight(_mRect));
            Debug.LogWarning(ViewRect.offsetMax);
            Debug.LogWarning(ViewRect.offsetMin);
        }

        private string Text => this.GetComponent<TMP_InputField>().text;

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
                    return this._originalWidth;
                }

                var offset = Mathf.Abs(ViewRect.offsetMax.x) + Mathf.Abs(ViewRect.offsetMin.x);
                return keepInitWidthSize ? Mathf.Max(_originalWidth, TextComponent.GetPreferredValues(Text).x) + offset : TextComponent.GetPreferredValues(Text).x + offset;
            }
        }

        public virtual void CalculateLayoutInputHorizontal()
        {
        }

        public virtual void CalculateLayoutInputVertical()
        {
        }

        public virtual float minWidth => -1;

        public virtual float flexibleWidth => -1;

        public virtual float minHeight => -1;

        public virtual float preferredHeight
        {
            get
            {
                if (string.IsNullOrEmpty(Text))
                {
                    return RectTransform.sizeDelta.y;
                }
                var offset = Mathf.Abs(ViewRect.offsetMax.y) + Mathf.Abs(ViewRect.offsetMin.y);
                if (fixedWidth)
                {
                    return fixedWidth ? 
                        TextComponent.GetPreferredValues(Text, _originalWidth, TextComponent.preferredHeight).y + offset : 
                        TextComponent.GetPreferredValues(Text, TextComponent.preferredWidth, TextComponent.preferredHeight).y + offset;
                }
                return TextComponent.preferredHeight + offset;
            }
        }

        public virtual float flexibleHeight => -1;

        //[Tooltip("输入框的字体大小，InputField的大小会随字体大小改变高度")]
        [HideInInspector] public int fontSize = 36;

        //[Tooltip("是否保持InputField的宽度不变")]
        [HideInInspector] public bool fixedWidth = true;

        //[Tooltip("是否不限制InputField的宽度")]
        [HideInInspector] public bool keepInitWidthSize = true;

        public virtual int layoutPriority => 1;
    }
}