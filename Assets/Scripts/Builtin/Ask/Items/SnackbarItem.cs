/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年8月12 15:31
 * function    : 
 * ===============================================
 * */
using System.Collections;
using System.Collections.Generic;
using App.Core.Master;
using App.Core.Tools;
using TMPro;
using UnityEngine;

namespace App.Modules
{
    public class SnackbarItem : AskBase
    {
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _connect;

        private void Awake()
        {
            _title = this.FindComponent<TextMeshProUGUI>("Title");
            _connect = this.FindComponent<TextMeshProUGUI>("Connect");
        }

        public override void Show(AskData data)
        {
            base.Show(data);
            var time = 2f;
            _title.text = data.title;
            _connect.text = data.connect;

            if (data.time > 0)
            {
                time = data.time;
            }
            TimeTaskMaster.Instance.AddTimeTask(Hide, time);
        }
    }
}
