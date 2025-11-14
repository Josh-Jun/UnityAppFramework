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
        private Button _background;
        private void Awake()
        {
            _prefab = this.FindGameObject("Cancel");
            _cancel = _prefab.GetComponent<Button>();
            _cancel.onClick.RemoveAllListeners();
            _cancel.onClick.AddListener(Hide);
            var background = transform.parent.Find("Background");
            _background = background.GetOrAddComponent<Button>();
            _background.transition = Selectable.Transition.None;
            _background.onClick.AddListener(Hide);
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
                text.color = item.Color.ToColor();
                var button = go.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Hide);
                button.onClick.AddListener(() => { item.Event?.Invoke(); });
            }
        }
        
        protected override void Hide()
        {
            _background.onClick.RemoveAllListeners();
            Destroy(_background);
            base.Hide();
        }
    }
}
