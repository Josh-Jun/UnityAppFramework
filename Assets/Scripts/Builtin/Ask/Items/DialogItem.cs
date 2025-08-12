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
using App.Core.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace App.Modules
{
    public class DialogItem : AskBase
    {
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _connect;
        private Button _confirm;
        private Button _cancel;
        private TextMeshProUGUI _confirmText;
        private TextMeshProUGUI _cancelText;
        
        private void Awake()
        {
            _title = this.FindComponent<TextMeshProUGUI>("Title");
            _connect = this.FindComponent<TextMeshProUGUI>("Connect");
            _confirm = this.FindComponent<Button>("Btns/BtnConfirm");
            _cancel = this.FindComponent<Button>("Btns/BtnCancel");
            _confirmText = this.FindComponent<TextMeshProUGUI>("Btns/BtnConfirm/Text");
            _cancelText = this.FindComponent<TextMeshProUGUI>("Btns/BtnCancel/Text");
        }

        public override void Show(AskData data)
        {
            base.Show(data);
            _title.text = data.title;
            _connect.text = data.connect;
            _cancel.onClick.RemoveAllListeners();
            _confirm.onClick.RemoveAllListeners();
            if (data.Events.Count > 0)
            {
                if(!string.IsNullOrEmpty(data.Events[0].BtnName))
                    _confirmText.text = data.Events[0].BtnName;
                _confirm.onClick.AddListener(() => { data.Events[0].Event?.Invoke(); });
            }

            if (data.Events.Count > 1)
            {
                if(!string.IsNullOrEmpty(data.Events[1].BtnName))
                    _cancelText.text = data.Events[1].BtnName;
                _cancel.onClick.AddListener(() => { data.Events[1].Event?.Invoke(); });
            }
            _confirm.onClick.AddListener(Hide);
            _cancel.onClick.AddListener(Hide);
        }
    }
}
