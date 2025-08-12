/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年8月11 18:19
 * function    :
 * ===============================================
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using App.Core.Master;
using App.Core.Tools;
using App.Modules;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace App.Modules
{
    public class ToastItem : AskBase
    {
        private Image _image;
        private TextMeshProUGUI _text;

        private void Awake()
        {
            _image = this.FindComponent<Image>("Image");
            _text = this.FindComponent<TextMeshProUGUI>("Text");
        }

        public override void Show(AskData data)
        {
            base.Show(data);
            var time = 2f;
            transform.localPosition = Vector3.up * Screen.height / 4 * data.pos;
            _image.SetGameObjectActive(!string.IsNullOrEmpty(data.iconUrl));
            if (!string.IsNullOrEmpty(data.iconUrl))
            {
                if (data.iconUrl.StartsWith("https://"))
                {
                    HttpsMaster.Instance.DownloadSprite(data.iconUrl, sprite => { _image.sprite = sprite; });
                }
                else
                {
                    var sprite = AssetsMaster.Instance.LoadAssetSync<Sprite>(data.iconUrl);
                    _image.sprite = sprite;
                }
            }

            if (data.time > 0)
            {
                time = data.time;
            }

            _text.text = data.connect;
            TimeTaskMaster.Instance.AddTimeTask(Hide, time);
        }
    }
}