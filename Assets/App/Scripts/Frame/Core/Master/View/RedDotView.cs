/* *
 * ===============================================
 * author      : Josh@macbook
 * e-mail      : shijun_z@163.com
 * create time : 2025年2月19 10:4
 * function    : 
 * ===============================================
 * */

using System;
using App.Core.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace App.Core.Master
{
    public class RedDotView : MonoBehaviour
    {
        public bool ShowCount = true;
        public RedDotMold RedDotMold;
        public RedDotAnchor Anchor;
        public Vector2 Offset;
        public int Size = 30;

        public int _count = 0;
        
        private GameObject _redDot;
        private Text _redDotText;
        private RectTransform _redDotRect;
        private void Awake()
        {
            InitRedDot();
        }

        private void InitRedDot()
        {
            if(_redDot) return;
            var prefab = AssetsMaster.Instance.LoadAssetSync<GameObject>(AssetPath.RedDot);
            _redDot = Instantiate(prefab, transform, false);
            _redDotRect = _redDot.GetComponent<RectTransform>();
            _redDotText = _redDot.GetComponentInChildren<Text>();
            _redDot.SetActive(false);
            float x, y;
            switch (Anchor)
            {
                case RedDotAnchor.UpperLeft:
                    x = 0f;
                    y = 1f;
                    break;
                case RedDotAnchor.UpperCenter:
                    x = 0.5f;
                    y = 1f;
                    break;
                case RedDotAnchor.UpperRight:
                    x = 1f;
                    y = 1f;
                    break;
                case RedDotAnchor.MiddleLeft:
                    x = 0f;
                    y = 0.5f;
                    break;
                case RedDotAnchor.MiddleCenter:
                    x = 0.5f;
                    y = 0.5f;
                    break;
                case RedDotAnchor.MiddleRight:
                    x = 1f;
                    y = 0.5f;
                    break;
                case RedDotAnchor.LowerLeft:
                    x = 0f;
                    y = 0f;
                    break;
                case RedDotAnchor.LowerCenter:
                    x = 0.5f;
                    y = 0f;
                    break;
                case RedDotAnchor.LowerRight:
                    x = 1f;
                    y = 0f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _redDotRect.anchorMin = new Vector2(x, y);
            _redDotRect.anchorMax = new Vector2(x, y);
            _redDotRect.pivot = new Vector2(x, y);
            _redDotRect.anchoredPosition = Offset;
            _redDotRect.sizeDelta = Vector2.one * Size;
            _redDotText.fontSize = Size - 10;
        }
        
        public void Refresh(int count)
        {
            InitRedDot();
            _redDot.SetActive(count > 0);
            _redDotText.text = ShowCount ? $"{count}" : string.Empty;
            _count = count;
        }
    }
}
