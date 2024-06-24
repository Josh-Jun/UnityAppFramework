using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// UGUI Tab键切换InputField
/// </summary>
namespace AppFrame.Tools
{
    public class InputNavigator : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        private EventSystem system; //eventsystem组件  场景中只能有一个用来检测UI的鼠标或者触摸事件，也可以检测带有UI的event事件的其他GameObject
        private bool isSelect = false; //是否选中InputField
        public Direction direction = Direction.Vertical; //InputField的排列方向  水平或者垂直

        /// <summary>
        /// 自定义的排列方向Enum枚举
        /// </summary>
        public enum Direction
        {
            Vertical = 0,
            Horizontal = 1
        }

        void Start()
        {
            system = EventSystem.current; //初始化system
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab) && isSelect) //当按下Tab键并且是选中InputField
            {
                Selectable next = null;
                var current = system.currentSelectedGameObject.GetComponent<Selectable>(); //获取当前选中的InputField

                int mark = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
                    ? 1
                    : -1; //判断是否按下Shift键（按下Shift是反向切换InputField光标）
                Vector3 dir = direction == Direction.Horizontal
                    ? Vector3.left * mark
                    : Vector3.up * mark; //根据排列方向确定寻找下一个InputField的方向
                next = GetNextSelectable(current, dir); //根据当前选中的InputField和向量方向，获取下一个InputField

                if (next != null) //当下一个不为空的时候
                {
                    var inputField = next.GetComponent<InputField>();
                    if (inputField == null) return; //判断是否有InputField组件，没有就ruturn
                    StartCoroutine(Wait(next));
                }
            }
        }

        /// <summary>
        /// 寻找下一个Selectable
        /// </summary>
        /// <param name="current"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        private static Selectable GetNextSelectable(Selectable current, Vector3 dir)
        {
            Selectable next = current.FindSelectable(dir);
            if (next == null) //如果下一个为空就向上找到第一个
                next = current.FindLoopSelectable(-dir);
            return next;
        }

        /// <summary>
        /// 将下一个InputField转换成当前选中的，这个是eventsystem自带的方法，不能直接调用需要等待最后一帧结束才能执行，否则报null
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        IEnumerator Wait(Selectable next)
        {
            yield return new WaitForEndOfFrame();
            system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
        }

        /// <summary>
        /// 上面继承的两个接口方法，选中方法
        /// </summary>
        /// <param name="eventData"></param>
        public void OnSelect(BaseEventData eventData)
        {
            isSelect = true;
        }

        /// <summary>
        /// 上面继承的两个接口方法，未选中方法
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDeselect(BaseEventData eventData)
        {
            isSelect = false;
        }
    }
}