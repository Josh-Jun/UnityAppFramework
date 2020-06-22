using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Super Image")]
    public class SuperImage : BaseImage
    {
        private UIVertex uivertex;

        private List<Vector3> innerVertices = new List<Vector3>();
        private List<Vector3> outterVertices = new List<Vector3>();

        [Tooltip("圆角顶点个数")]
        [Range(1, 100)]
        public int segements = 20;
        [Tooltip("圆角大小")]
        [Range(0, 1)]
        public float round = 1f;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            innerVertices.Clear();
            outterVertices.Clear();

            Vector4 uv = overrideSprite != null ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;

            Rect pixelAdjustedRect = GetPixelAdjustedRect();
            float w = pixelAdjustedRect.width;
            float h = pixelAdjustedRect.height;

            float ASPECT_X = w > h ? h / w : 1;
            float ASPECT_Y = h > w ? w / h : 1;
            #region 圆角矩形
            Vector2 u;
            //uivertex = new UIVertex();
            //uivertex.color = color;
            //uivertex.position = Vector2.zero;
            //uivertex.uv0 = Vector2.zero;
            //vh.AddVert(uivertex);

            #region 左下角
            for (int i = 0; i < segements + 1; i++)
            {
                uivertex = new UIVertex();
                uivertex.color = color;
                if (i == 0)
                {
                    //半径*
                    uivertex.uv0 = new Vector2(uv.x + (round * ASPECT_X / 2) * (uv.z - uv.x) * (1 - Mathf.Sin(i * 90 / segements)), uv.y + (round * ASPECT_Y / 2) * (uv.w - uv.y) * (1 - Mathf.Cos(i * 90 / segements)));
                    u = new Vector2((round * ASPECT_X / 2) * (1 - Mathf.Sin(i * 90 / segements)), (round * ASPECT_Y / 2) * (1 - Mathf.Cos(i * 90 / segements)));
                }
                else
                {
                    uivertex.uv0 = new Vector2(uv.x + (round * ASPECT_X / 2) * (uv.z - uv.x) * (1 - Mathf.Sin(Mathf.PI / (180f / (i * 90 / segements)))), uv.y + (round * ASPECT_Y / 2) * (uv.w - uv.y) * (1 - Mathf.Cos(Mathf.PI / (180f / (i * 90 / segements)))));
                    u = new Vector2((round * ASPECT_X / 2) * (1 - Mathf.Sin(Mathf.PI / (180f / (i * 90 / segements)))), (round * ASPECT_Y / 2) * (1 - Mathf.Cos(Mathf.PI / (180f / (i * 90 / segements)))));
                }
                uivertex.position = new Vector3(w * u.x - w / 2, h * u.y - h / 2);
                vh.AddVert(uivertex);
                outterVertices.Add(new Vector3(w * u.x - w / 2, h * u.y - h / 2));
            }
            #endregion

            #region 左上角
            //2
            for (int i = 0; i < segements + 1; i++)
            {
                uivertex = new UIVertex();
                uivertex.color = color;
                if (i == 0)
                {
                    uivertex.uv0 = new Vector2(uv.x + (uv.z - uv.x) * (round * ASPECT_X / 2) * (1 - Mathf.Cos(i * 90 / segements)), uv.y + (uv.w - uv.y) * ((1 - round * ASPECT_Y / 2) + (round * ASPECT_Y / 2) * Mathf.Sin(i * 90 / segements)));
                    u = new Vector2((round * ASPECT_X / 2) - (round * ASPECT_X / 2) * Mathf.Cos(i * 90 / segements), (1 - round * ASPECT_Y / 2) + (round * ASPECT_Y / 2) * Mathf.Sin(i * 90 / segements));
                }
                else
                {
                    uivertex.uv0 = new Vector2(uv.x + (uv.z - uv.x) * (round * ASPECT_X / 2) * (1 - Mathf.Cos(Mathf.PI / (180f / (i * 90 / segements)))), uv.y + (uv.w - uv.y) * ((1 - round * ASPECT_Y / 2) + (round * ASPECT_Y / 2) * Mathf.Sin(Mathf.PI / (180f / (i * 90 / segements)))));
                    u = new Vector2((round * ASPECT_X / 2) * (1 - Mathf.Cos(Mathf.PI / (180f / (i * 90 / segements)))), (1 - round * ASPECT_Y / 2) + (round * ASPECT_Y / 2) * Mathf.Sin(Mathf.PI / (180f / (i * 90 / segements))));
                }
                uivertex.position = new Vector3(w * u.x - w / 2, h * u.y - h / 2);
                vh.AddVert(uivertex);
                outterVertices.Add(new Vector3(w * u.x - w / 2, h * u.y - h / 2));
            }
            #endregion

            #region 右上角
            //3
            for (int i = 0; i < segements + 1; i++)
            {
                uivertex = new UIVertex();
                uivertex.color = color;
                if (i == 0)
                {
                    uivertex.uv0 = new Vector2(uv.x + (uv.z - uv.x) * ((1 - round * ASPECT_X / 2) + (round * ASPECT_X / 2) * Mathf.Sin(i * 90 / segements)), uv.y + (uv.w - uv.y) * ((1 - round * ASPECT_Y / 2) + (round * ASPECT_Y / 2) * Mathf.Cos(i * 90 / segements)));
                    u = new Vector2((1 - round * ASPECT_X / 2) + (round * ASPECT_X / 2) * Mathf.Sin(i * 90 / segements), (1 - round * ASPECT_Y / 2) + (round * ASPECT_Y / 2) * Mathf.Cos(i * 90 / segements));
                }
                else
                {
                    uivertex.uv0 = new Vector2(uv.x + (uv.z - uv.x) * ((1 - round * ASPECT_X / 2) + (round * ASPECT_X / 2) * Mathf.Sin(Mathf.PI / (180f / (i * 90 / segements)))), uv.y + (uv.w - uv.y) * ((1 - round * ASPECT_Y / 2) + (round * ASPECT_Y / 2) * Mathf.Cos(Mathf.PI / (180f / (i * 90 / segements)))));
                    u = new Vector2((1 - round * ASPECT_X / 2) + (round * ASPECT_X / 2) * Mathf.Sin(Mathf.PI / (180f / (i * 90 / segements))), (1 - round * ASPECT_Y / 2) + (round * ASPECT_Y / 2) * Mathf.Cos(Mathf.PI / (180f / (i * 90 / segements))));
                }
                uivertex.position = new Vector3(w * u.x - w / 2, h * u.y - h / 2);
                vh.AddVert(uivertex);
                outterVertices.Add(new Vector3(w * u.x - w / 2, h * u.y - h / 2));
            }
            #endregion

            #region 右下角
            //4
            for (int i = 0; i < segements + 1; i++)
            {
                uivertex = new UIVertex();
                uivertex.color = color;
                if (i == 0)
                {
                    uivertex.uv0 = new Vector2(uv.x + (uv.z - uv.x) * ((1 - round * ASPECT_X / 2) + (round * ASPECT_X / 2) * Mathf.Cos(i * 90 / segements)), uv.y + (uv.w - uv.y) * ((round * ASPECT_Y / 2) - (round * ASPECT_Y / 2) * Mathf.Sin(i * 90 / segements)));
                    u = new Vector2((1 - round * ASPECT_X / 2) + (round * ASPECT_X / 2) * Mathf.Cos(i * 90 / segements), (round * ASPECT_Y / 2) - (round * ASPECT_Y / 2) * Mathf.Sin(i * 90 / segements));
                }
                else
                {
                    uivertex.uv0 = new Vector2(uv.x + (uv.z - uv.x) * ((1 - round * ASPECT_X / 2) + (round * ASPECT_X / 2) * Mathf.Cos(Mathf.PI / (180f / (i * 90 / segements)))), uv.y + (uv.w - uv.y) * ((round * ASPECT_Y / 2) - (round * ASPECT_Y / 2) * Mathf.Sin(Mathf.PI / (180f / (i * 90 / segements)))));
                    u = new Vector2((1 - round * ASPECT_X / 2) + (round * ASPECT_X / 2) * Mathf.Cos(Mathf.PI / (180f / (i * 90 / segements))), (round * ASPECT_Y / 2) - (round * ASPECT_Y / 2) * Mathf.Sin(Mathf.PI / (180f / (i * 90 / segements))));
                }
                uivertex.position = new Vector3(w * u.x - w / 2, h * u.y - h / 2);
                vh.AddVert(uivertex);
                outterVertices.Add(new Vector3(w * u.x - w / 2, h * u.y - h / 2));
            }
            #endregion

            //（（点*4+8个点-3）条弦+1）个三角形
            for (int i = 0; i < ((segements - 1) * 4 + 8 - 3 + 1); i++)
            {
                vh.AddTriangle(0, i + 1, i + 2);
            }
            ////首尾顶点相连
            //vh.AddTriangle(vh.currentVertCount - 2, vh.currentVertCount - 1, 0);
            //vh.AddTriangle(vh.currentVertCount - 1, 0, 1);
            #endregion
        }
        public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            Sprite sprite = overrideSprite;
            if (sprite == null)
                return true;
            if (eventCamera.orthographic == true)
            {
                return Contains(screenPoint - new Vector2(Screen.width / 2, Screen.height / 2), outterVertices, innerVertices);
            }
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out local);
            return Contains(local, outterVertices, innerVertices);
        }

        private bool Contains(Vector2 p, List<Vector3> outterVertices, List<Vector3> innerVertices)
        {
            var crossNumber = 0;
            RayCrossing(p, innerVertices, ref crossNumber);//检测内环
            RayCrossing(p, outterVertices, ref crossNumber);//检测外环
            return (crossNumber & 1) == 1;
        }

        /// <summary>
        /// 使用RayCrossing算法判断点击点是否在封闭多边形里
        /// </summary>
        /// <param name="p"></param>
        /// <param name="vertices"></param>
        /// <param name="crossNumber"></param>
        private void RayCrossing(Vector2 p, List<Vector3> vertices, ref int crossNumber)
        {
            for (int i = 0, count = vertices.Count; i < count; i++)
            {
                var v1 = vertices[i];
                var v2 = vertices[(i + 1) % count];

                //点击点水平线必须与两顶点线段相交
                if (((v1.y <= p.y) && (v2.y > p.y)) || ((v1.y > p.y) && (v2.y <= p.y)))
                {
                    //只考虑点击点右侧方向，点击点水平线与线段相交，且交点x > 点击点x，则crossNumber+1
                    if (p.x < v1.x + (p.y - v1.y) / (v2.y - v1.y) * (v2.x - v1.x))
                    {
                        crossNumber += 1;
                    }
                }
            }
        }
    }
}