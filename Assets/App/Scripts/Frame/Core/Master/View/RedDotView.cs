/* *
 * ===============================================
 * author      : Josh@macbook
 * e-mail      : shijun_z@163.com
 * create time : 2025年2月19 10:4
 * function    : 
 * ===============================================
 * */
using System.Collections;
using System.Collections.Generic;
using App.Core.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace App.Core.Master
{
    public class RedDotView : MonoBehaviour
    {
        public bool ShowCount = true;
        public RedDotMold RedDotMold;
        
        private GameObject _redDot;
        private Text _redDotText;
        private void Awake()
        {
            var prefab = AssetsMaster.Instance.LoadAsset<GameObject>(AssetPath.RedDot);
            _redDot = Instantiate(prefab, transform, false);
            _redDotText = _redDot.GetComponentInChildren<Text>();
            _redDot.SetActive(false);
        }
        public void Refresh(int count)
        {
            _redDot.SetActive(count > 0);
            _redDotText.text = ShowCount ? $"{count}" : string.Empty;
        }
    }
}
