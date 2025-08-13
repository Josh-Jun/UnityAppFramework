/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年8月12 15:32
 * function    : 
 * ===============================================
 * */
using System.Collections;
using System.Collections.Generic;
using App.Core.Master;
using App.Core.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace App.Modules
{
    public class ActionbarItem : AskBase
    {
        private GameObject _prefab;
        private Button _cancel;
        private void Awake()
        {
            _prefab = this.FindGameObject("Cancel");
            _cancel = _prefab.GetComponent<Button>();
            _cancel.onClick.RemoveAllListeners();
            _cancel.onClick.AddListener(Hide);
        }

        public override void Show(AskData data)
        {
            base.Show(data);
            transform.RectTransform().anchoredPosition = Vector2.up * ViewMaster.Instance.UISafeArea2D.offsetMin.y;
            foreach (var item in data.Events)
            {
                var go = Instantiate(_prefab, transform);
                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                text.text = item.BtnName;
                var button = go.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Hide);
                button.onClick.AddListener(() => { item.Event?.Invoke(); });
            }
        }
    }
}
