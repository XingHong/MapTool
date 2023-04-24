using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AutoGenTileMapProcess
{
    private static TextureColorScriptableObject curSO;

    [MenuItem("MapTool/CreateTileMapByScreenshot")]
    public static void CreateTileMapByScreenshot()
    {
        bool success = false;
        ReadScreenshot();
        success = GenColorTiles();
        if (success)
            GenPaletteTool.CreatePalette(curSO.tileColors.Length);
        PainTool.Pain(curSO);
    }

    public static void ReadScreenshot()
    {
        var tex = (Texture2D)AssetDatabase.LoadAssetAtPath(MapToolPath.ScreenshotPng, typeof(Texture2D));
        SaveTextureColorAsset(tex);
        AssetDatabase.Refresh();
    }

    private static void SaveTextureColorAsset(Texture2D texture)
    {
        var pixels = texture.GetPixels();
        HashSet<Color> colorSet = new HashSet<Color>(pixels);
        TextureColorScriptableObject newScriptableObject = ScriptableObject.CreateInstance<TextureColorScriptableObject>();
        newScriptableObject.tileColors = new TextureColorData[colorSet.Count];
        int i = 0;
        foreach (var col in colorSet)
        {
            TextureColorData newdata = new TextureColorData();
            newdata.color = col;
            newdata.index = i;
            newScriptableObject.tileColors[i] = newdata;
            ++i;
        }
        AssetDatabase.CreateAsset(newScriptableObject, MapToolPath.TextureColorSO);
    }

    private static bool GenColorTiles()
    {
        curSO = (TextureColorScriptableObject)AssetDatabase.LoadAssetAtPath(MapToolPath.TextureColorSO, typeof(TextureColorScriptableObject));
        if (curSO == null)
            return false;
        //����png
        foreach(var tcdata in curSO.tileColors)
        {
            GenPngEditor.CreateRhombusPng(tcdata.index, tcdata.color);
        }
        AssetDatabase.Refresh();
        //����tile
        foreach (var tcdata in curSO.tileColors)
        {
            GenPngEditor.CreateRhombusTile(tcdata.index, tcdata.color);
        }
        AssetDatabase.Refresh();
        return true;
    }
}