using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AppFramework.Tools
{
    [ExecuteInEditMode]
    public class UIDepth : MonoBehaviour
    {
        public int order = 100;
        public bool isUI = false;

        private void Start()
        {
            if (isUI)
            {
                Canvas canvas = this.TryGetComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = order;
            }
            else
            {
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    renderer.sortingOrder = order;
                }
            }
        }
    }
}