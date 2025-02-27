using App.Core.Tools;

namespace UnityEngine.UI
{
    [ExecuteInEditMode]
    public class UIDepth : MonoBehaviour
    {
        public int order = 100;
        public bool isUI = false;
        /// <summary>
        /// 对于3D物体和粒子特效，材质需要设置为透明模式
        /// </summary>
        private void Start()
        {
            if (isUI)
            {
                Canvas canvas = this.GetOrAddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = order;
            }
            else
            {
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    renderer.sortingOrder = order;
                }
            }
        }
    }
}