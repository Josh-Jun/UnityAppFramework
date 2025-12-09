#define ENABLE_TEST

using System;
using System.Collections;
using System.Collections.Generic;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace App.Modules
{
    public enum AskMold
    {
        Toast,
        Dialog,
        Actionbar,
        Snackbar,
    }
    [Serializable]
    public class AskData
    {
        // 类型
        public AskMold mold;
        // 标题
        public string title;
        // 内容
        public string connect;
        // Toast图标
        public string iconUrl;
        // 显示时间（只有Toast和Snackbar使用）
        public float time;
        // 按钮事件 （按钮显示名称，按钮名称字体颜色，按钮事件）
        public List<(string BtnName, string Color, Action Event)> Events = new();
        // 上（1），中（0），下（-1）
        public int pos;
    }

    [LogicOf("Ask", AssetPath.Global)]
    public class AskLogic : EventBase, ILogic
    {
        private AskView View => ViewMaster.Instance.GetView<AskView>();

        public AskLogic()
        {
            AddEventMsg<object>("OpenAskView", OpenAskView);
            AddEventMsg("CloseAskView", CloseAskView);
        }

        #region Life Cycle

#if ENABLE_TEST
        private int _index = 0;
#endif

        public void Begin()
        {
            View.BackgroundRectTransform.SetGameObjectActive(false);

#if ENABLE_TEST
            TimeUpdateMaster.Instance.StartTimer((time) =>
            {
                if (Input.GetKeyDown(KeyCode.U))
                {
                    _index++;
                    View.OpenView(new AskData()
                    {
                        mold = AskMold.Toast,
                        connect = $"Toast测试{_index}",
                        pos = -1,
                        iconUrl = "https://img.keaitupian.cn/newupload/06/1686641149755068.jpg"
                    });
                }

                if (Input.GetKeyDown(KeyCode.I))
                {
                    View.OpenView(new AskData()
                    {
                        mold = AskMold.Snackbar,
                        title = "Snackbar标题",
                        connect = $"Snackbar测试内容",
                        time = 2,
                    });
                }

                if (Input.GetKeyDown(KeyCode.O))
                {
                    View.OpenView(new AskData()
                    {
                        mold = AskMold.Dialog,
                        title = "Dialog标题",
                        connect = $"Dialog测试内容",
                        Events = new List<(string BtnName, string Color, Action Event)>()
                        {
                            ("确认1", Color.red.ToHex(), () => { Log.I("确认点击事件"); }),
                            ("取消2", Color.blue.ToHex(), () => { Log.I("取消点击事件"); }),
                        }
                    });
                }

                if (Input.GetKeyDown(KeyCode.P))
                {
                    View.OpenView(new AskData()
                    {
                        mold = AskMold.Actionbar,
                        Events = new List<(string BtnName, string Color, Action Event)>()
                        {
                            ("事件1", "", () => { Log.I("事件1点击事件"); }),
                            ("事件2", "", () => { Log.I("事件2点击事件"); }),
                            ("事件3", "", () => { Log.I("事件3点击事件"); }),
                            ("事件4", "", () => { Log.I("事件4点击事件"); }),
                            ("事件5", "#FF6363", () => { Log.I("事件5点击事件"); }),
                        }
                    });
                }
            });
#endif
        }

        public void End()
        {
        }

        public void AppPause(bool pause)
        {
        }

        public void AppFocus(bool focus)
        {
        }

        public void AppQuit()
        {
        }

        #endregion

        #region Logic

        private const string BasePath = "Assets/Bundles/Builtin/Prefabs/UI/Items/Ask";

        private void OpenAskView(AskData data)
        {
            View.BackgroundRectTransform.SetGameObjectActive();
            var prefab = AssetsMaster.Instance.LoadAssetSync<GameObject>($"{BasePath}/{data.mold}Item.prefab");
            var item = Object.Instantiate(prefab, View.transform);
            var ask = item.GetComponent<AskBase>();
            ask.Show(data);
        }

        #endregion

        #region View Logic

        private void OpenAskView(object obj)
        {
            if (obj is AskData data)
            {
                OpenAskView(data);
            }
            else
            {
                Log.W("OpenAskView obj is not AskData");
            }
        }

        private void CloseAskView()
        {
            View.BackgroundRectTransform.SetGameObjectActive(false);
        }

        #endregion
    }
}