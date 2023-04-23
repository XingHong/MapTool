using UnityEngine;
using UnityEditor;

public class PngTextureProcesser : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        string lowerCaseAssetPath = assetPath.ToLower();
        if (lowerCaseAssetPath.IndexOf("/autogenpngs/") != -1)
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.spritePixelsPerUnit = 256;
            textureImporter.filterMode = FilterMode.Point;
        }
        else if (lowerCaseAssetPath.IndexOf("/screenshot/") != -1)
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.spritePixelsPerUnit = 1024;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.isReadable = true;
        }
            
    }
}