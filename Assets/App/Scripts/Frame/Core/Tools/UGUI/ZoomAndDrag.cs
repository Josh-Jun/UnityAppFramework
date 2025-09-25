/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年7月31 18:50
 * function    : 
 * ===============================================
 * */
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI{
    public class ZoomAndDrag : MonoBehaviour, IDragHandler, IScrollHandler{
        private RectTransform rawImageRect;
        private Vector3 initialScale;
        public RectTransform cropArea;

        public float zoomSpeed = 0.1f;
        public float maxZoom = 2f;
        public float minZoom = 1f;
        public float damping = 0.3f; // 阻尼系数

        private void Awake(){
            rawImageRect = GetComponent<RectTransform>();
            initialScale = rawImageRect.localScale;
        }

        public void OnDrag(PointerEventData eventData){
            Vector2 delta = eventData.delta;

            // 根据RawImage和CropArea的大小决定拖拽方向
            bool isWidthGreaterThanCropArea = rawImageRect.rect.width * rawImageRect.localScale.x > cropArea.rect.width;
            bool isHeightGreaterThanCropArea =
                rawImageRect.rect.height * rawImageRect.localScale.y > cropArea.rect.height;

            if (isWidthGreaterThanCropArea && isHeightGreaterThanCropArea){
                // 如果两者都大于限制范围，则允许双向拖拽
            }
            else if (isWidthGreaterThanCropArea){
                delta.y = 0; // 只允许横向拖拽
            }
            else if (isHeightGreaterThanCropArea){
                delta.x = 0; // 只允许纵向拖拽
            }
            else{
                return; // 如果两者一样大或者都小于限制范围，则不允许拖拽
            }

            rawImageRect.anchoredPosition += delta;
            ApplyDamping();
        }

        public void OnScroll(PointerEventData eventData){
            float delta = eventData.scrollDelta.y * zoomSpeed;
            Vector3 scaleChange = Vector3.one * delta;
            Vector3 newScale = rawImageRect.localScale + scaleChange;

            // 限制缩放范围并保持等比例
            float scale = Mathf.Clamp(newScale.x, initialScale.x * minZoom, initialScale.x * maxZoom);
            rawImageRect.localScale = new Vector3(scale, scale, 1f);
            ApplyDamping();

        }
        private void Update(){
            if (Input.touchCount == 2){
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                float difference = currentMagnitude - prevMagnitude;

                ScaleRawImage(difference * zoomSpeed);
            }
        }

        private void ScaleRawImage(float scaleFactor){
            Vector3 scaleChange = Vector3.one * scaleFactor;
            Vector3 newScale = rawImageRect.localScale + scaleChange;

            // 限制缩放范围
            newScale = new Vector3(
                Mathf.Clamp(newScale.x, initialScale.x * minZoom, initialScale.x * maxZoom),
                Mathf.Clamp(newScale.y, initialScale.y * minZoom, initialScale.y * maxZoom),
                1f);

            rawImageRect.localScale = newScale;
            ApplyDamping();
        }

        private void ApplyDamping(){
            Vector2 pos = rawImageRect.anchoredPosition;
            float maxX = Mathf.Max((rawImageRect.rect.width * rawImageRect.localScale.x - cropArea.rect.width) / 2, 0);
            float maxY = Mathf.Max((rawImageRect.rect.height * rawImageRect.localScale.y - cropArea.rect.height) / 2,
                0);
            Vector2 clampedPosition = new Vector2(
                Mathf.Clamp(pos.x, -maxX, maxX),
                Mathf.Clamp(pos.y, -maxY, maxY)
            );

            // 应用阻尼效果
            rawImageRect.anchoredPosition = Vector2.Lerp(pos, clampedPosition, damping);

            // 再次确保位置在限制范围内
            pos = rawImageRect.anchoredPosition;
            pos.x = Mathf.Clamp(pos.x, -maxX, maxX);
            pos.y = Mathf.Clamp(pos.y, -maxY, maxY);
            rawImageRect.anchoredPosition = pos;
        }
    }
}