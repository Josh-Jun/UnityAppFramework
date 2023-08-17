using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    [Serializable]
    public class ChatDialogData
    {
        public ChatDialogType type;
        public string msg;
        public Vector2 size;
    }

    public enum ChatDialogType
    {
        Time,
        AI,
        Person,
    }

    public class ChatDialogItem : MonoBehaviour
    {
        public int index;
        private Text text;
        private RectTransform rectText;
        private RectTransform rectBg;
        
        public RectTransform rect;
        public VerticalLayoutGroup verticalLayoutGroup;

        private float preferredWidth = 690f;

        public void AddChatDialog(ChatDialogType type, string connect)
        {
            GetChild(type);
            text.text = connect;
            if (type != ChatDialogType.Time)
            {
                var width = CaculateTextWidth(text, connect) < preferredWidth
                    ? CaculateTextWidth(text, connect)
                    : preferredWidth;
                rectText.sizeDelta = new Vector2(width, rectText.sizeDelta.y);
                rectBg.sizeDelta = new Vector2(width + 60, rectBg.sizeDelta.y);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }

        private void GetChild(ChatDialogType type)
        {
            foreach (Transform t in transform)
            {
                t.gameObject.SetActive(false);
            }
            text = transform.Find($"{type}/Text").GetComponent<Text>();
            rectBg = transform.Find($"{type}").GetComponent<RectTransform>();
            rectText = text.rectTransform;
            rectBg.gameObject.SetActive(true);
            switch (type)
            {
                case ChatDialogType.AI:
                    verticalLayoutGroup.childAlignment = TextAnchor.UpperRight;
                    break;
                case ChatDialogType.Time:
                case ChatDialogType.Person:
                    verticalLayoutGroup.childAlignment = TextAnchor.UpperLeft;
                    break;
                default:
                    break;
            }
        }

        private float CaculateTextWidth(Text text, string connect)
        {
            var extents = text.cachedTextGenerator.rectExtents.size * 0.5f;
            var settings = text.GetGenerationSettings(extents);
            var width = text.cachedTextGeneratorForLayout.GetPreferredWidth(connect, settings);
            return width;
        }

        public void Clear()
        {
            text.text = null;
        }
    }
}