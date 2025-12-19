/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年7月8 10:14
 * function    :
 * ===============================================
 * */

using UnityEditor;

public class AssetPostEditor : AssetPostprocessor 
{
    private void OnPreprocessTexture()
    {
        if (assetImporter is not TextureImporter importer) return;
        if (importer.assetPath.Contains("@sprite"))
        {
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
            }
        }
        else if (importer.assetPath.Contains("@normalmap"))
        {
            if (importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
            }
        }
        else if (importer.assetPath.Contains("@cubemap"))
        {
            if (importer.textureShape != TextureImporterShape.TextureCube)
            {
                importer.textureShape = TextureImporterShape.TextureCube;
            }
        }
    }
}
