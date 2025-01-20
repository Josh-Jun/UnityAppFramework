using System;


namespace UnityEngine.UI
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class ScrollViewEx : ScrollView
    {
        protected override void Awake()
        {
            base.Awake();
            onValueChanged.AddListener(OnValueChanged);
        }

        [SerializeField]
        private int m_pageSize = 50;

        public int pageSize => m_pageSize;

        private int startOffset = 0;

        private Func<int> realItemCountFunc;

        private bool canNextPage = false;

        private void Update()
        {
            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonDown(0))
                canNextPage = true;
        }

        public override void SetUpdateFunc(Action<int, RectTransform> func)
        {
            if (func != null)
            {
                var f = func;
                func = (index, rect) =>
                {
                    f(index + startOffset, rect);
                };
            }
            base.SetUpdateFunc(func);
        }

        public override void SetItemSizeFunc(Func<int, Vector2> func)
        {
            if (func != null)
            {
                var f = func;
                func = (index) =>
                {
                    return f(index + startOffset);
                };
            }
            base.SetItemSizeFunc(func);
        }

        public override void SetItemCountFunc(Func<int> func)
        {
            realItemCountFunc = func;
            if (func != null)
            {
                var f = func;
                func = () => Mathf.Min(f(), pageSize);
            }
            base.SetItemCountFunc(func);
        }

        protected override void InternalScrollTo(int index)
        {
            int count = 0;
            if (realItemCountFunc != null)
            {
                count = realItemCountFunc();
            }
            index = Mathf.Clamp(index, 0, count - 1);
            startOffset = Mathf.Clamp(index - pageSize / 2, 0, count - itemCountFunc());
            UpdateData(true);

            base.InternalScrollTo(index - startOffset);
        }

        bool reloadFlag = false;


        private void OnValueChanged(Vector2 position)
        {
            if (reloadFlag)
            {
                UpdateData(true);
                reloadFlag = false;
            }
            if (Input.GetMouseButton(0) && !canNextPage) return;

            int toShow;
            int critical;
            bool downward;
            int pin;
            if (((int)layoutType & flagScrollDirection) == 1)
            {
                // 垂直滚动 只计算y向
                if (velocity.y > 0)
                {
                    // 向上
                    toShow = criticalItemIndex[CriticalItemType.DownToShow];
                    critical = pageSize - 1;
                    if (toShow < critical)
                    {
                        return;
                    }
                    pin = critical - 1;
                    downward = false;
                }
                else
                {
                    // 向下
                    toShow = criticalItemIndex[CriticalItemType.UpToShow];
                    critical = 0;
                    if (toShow > critical)
                    {
                        return;
                    }
                    pin = critical + 1;
                    downward = true;
                }
            }
            else // = 0
            {
                // 水平滚动 只计算x向
                if (velocity.x > 0)
                {
                    // 向右
                    toShow = criticalItemIndex[CriticalItemType.UpToShow];
                    critical = 0;
                    if (toShow > critical)
                    {
                        return;
                    }
                    pin = critical + 1;
                    downward = true;
                }
                else
                {
                    // 向左
                    toShow = criticalItemIndex[CriticalItemType.DownToShow];
                    critical = pageSize - 1;
                    if (toShow < critical)
                    {
                        return;
                    }
                    pin = critical - 1;
                    downward = false;
                }
            }

            // 翻页
            int old = startOffset;
            if (downward)
            {
                startOffset -= pageSize / 2;
            }
            else
            {
                startOffset += pageSize / 2;
            }
            canNextPage = false;


            int realDataCount = 0;
            if (realItemCountFunc != null)
            {
                realDataCount = realItemCountFunc();
            }
            startOffset = Mathf.Clamp(startOffset, 0, Mathf.Max(realDataCount - pageSize, 0));

            if (old != startOffset)
            {
                reloadFlag = true;

                // 计算 pin元素的世界坐标
                Rect rect = GetItemLocalRect(pin);
                Vector2 oldWorld = content.TransformPoint(rect.position);
                UpdateData(true);
                int dataCount = 0;
                if (itemCountFunc != null)
                {
                    dataCount = itemCountFunc();
                }
                if (dataCount > 0)
                {
                    EnsureItemRect(0);
                    if (dataCount > 1)
                    {
                        EnsureItemRect(dataCount - 1);
                    }
                }

                // 根据 pin元素的世界坐标 计算出content的position
                int pin2 = pin + old - startOffset;
                Rect rect2 = GetItemLocalRect(pin2);
                Vector2 newWorld = content.TransformPoint(rect2.position);
                Vector2 deltaWorld = newWorld - oldWorld;

                Vector2 deltaLocal = content.InverseTransformVector(deltaWorld);
                SetContentAnchoredPosition(content.anchoredPosition - deltaLocal);

                UpdateData(true);

                // 减速
                velocity /= 50f;
            }

        }
    }
}
