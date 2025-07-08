/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年7月8 10:14
 * function    :
 * ===============================================
 * */

using UnityEditor;

public class PostAssetEditor : AssetPostprocessor 
{
    private void OnPreprocessTexture()
    {
        if (assetImporter is not TextureImporter texImpoter) return;
        if (texImpoter.assetPath.Contains("@sprite"))
        {
            if (texImpoter.textureType != TextureImporterType.Sprite)
            {
                texImpoter.textureType = TextureImporterType.Sprite;
            }
        }
        else if (texImpoter.assetPath.Contains("@normalmap"))
        {
            if (texImpoter.textureType != TextureImporterType.NormalMap)
            {
                texImpoter.textureType = TextureImporterType.NormalMap;
            }
        }
    }
}
