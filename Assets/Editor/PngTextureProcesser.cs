using UnityEngine;
using UnityEditor;

public class PngTextureProcesser : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        string lowerCaseAssetPath = assetPath.ToLower();
        if (lowerCaseAssetPath.IndexOf("/autogenpngs/") == -1)
            return;
        TextureImporter textureImporter = (TextureImporter)assetImporter;
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Single;
        textureImporter.spritePixelsPerUnit = 256;
        textureImporter.filterMode = FilterMode.Point;
    }
}