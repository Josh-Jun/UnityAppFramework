using UnityEngine;
/// <summary>
/// 图片工具类
/// </summary>
public static class PictureManager
{
    /// <summary> 创建Sprite</summary>
    public static Texture2D CreateTexture(byte[] bytes, int width, int height)
    {
        //设置头像   
        Texture2D tex2d = new Texture2D(width, height);
        tex2d.LoadImage(bytes);
        return tex2d;
    }
    /// <summary> 创建Sprite</summary>
    public static Sprite CreateSprite(byte[] bytes, int width, int height)
    {
        //设置头像   
        Texture2D tex2d = CreateTexture(bytes, width, height);
        Sprite sprite = Sprite.Create(tex2d, new Rect(0, 0, tex2d.width, tex2d.height), new Vector2(0.5f, 0.5f));//后面Vector2就是你Anchors的Pivot的x/y属性值
        return sprite;
    }
    public static byte[] CreateByte(Sprite sp)
    {
        //转换成Texture
        Texture2D temp = sp.texture;
        //在转换成bytes
        byte[] textureByte = temp.EncodeToPNG();
        return textureByte;
    }
    public static byte[] CreateByte(Texture2D texture)
    {
        //在转换成bytes
        byte[] textureByte = texture.EncodeToPNG();
        return textureByte;
    }
    /// <summary>
    /// 图片自适应
    /// </summary>
    /// <param name="limitRange"></param>
    /// <param name="textureSize"></param>
    /// <returns></returns>
    public static Vector2 AdaptSize(Vector2 limitRange, Vector2 textureSize)
    {
        Vector2 size = textureSize;
        float standard_ratio = limitRange.x / limitRange.y;
        float ratio = size.x / size.y;
        if (ratio > standard_ratio)
        {
            //宽于标准宽度，宽固定
            float scale = size.x / limitRange.x;
            size.x = limitRange.x;
            size.y /= scale;
        }
        else
        {
            //高于标准宽度，高固定
            float scale = size.y / limitRange.y;
            size.y = limitRange.y;
            size.x /= scale;
        }
        return size;
    }
}
