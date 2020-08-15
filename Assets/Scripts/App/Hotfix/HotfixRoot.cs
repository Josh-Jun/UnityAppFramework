using EventController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hotfix
{
    public class HotfixRoot : SingletonEvent<HotfixRoot>, IRoot
    {
        private HotfixWin hotfixWin;
        public HotfixRoot()
        {
            string prefab_HotfixPath = "App/Hotfix/Windows/HotfixWin";
            hotfixWin = LoadHotfixWindow<HotfixWin>(prefab_HotfixPath);
        }

        public void Begin()
        {
            //SendEventMsg("HotfixWin", true);
            //hotfixWin.SetWindowActive(true);
            hotfixWin.StartHotfix();
        }

        /// <summary> 加载热更窗口 </summary>
        public T LoadHotfixWindow<T>(string path, bool state = false) where T : Component
        {
            GameObject go = Resources.Load<GameObject>(path);
            if (go != null)
            {
                go = Object.Instantiate(go, UIRoot.Instance.UIRectTransform);
                go.transform.localEulerAngles = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.name = go.name.Replace("(Clone)", "");
                T t = go.AddComponent<T>();
                EventDispatcher.TriggerEvent(go.name, state);
                return t;
            }
            return null;
        }

        public void Finish()
        {

        }
    }
}