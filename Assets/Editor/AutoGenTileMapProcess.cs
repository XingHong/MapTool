using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//using System.Drawing;

public class AutoGenTileMapProcess
{
    private static TextureColorScriptableObject curSO;

    [MenuItem("MapTool/CreateTileMapByScreenshot")]
    public static void CreateTileMapByScreenshot()
    {
        DeleteAllFilesInFolder();
        ReadScreenshot();
        bool success = false;
        success = GenColorTiles();
        if (success)
        {
            GenPaletteTool.CreatePalette(curSO.tileColors.Length);
        }
        else
        {
            Debug.LogError("GenColorTiles fail!");
            return;
        }
        PainTool.Pain(curSO);
        ExportTool.GenData();
    }

    static void DeleteAllFilesInFolder()
    {
        string[] unusedFolder = { MapToolPath.PalettesDir, MapToolPath.AutoGenPngsDir, MapToolPath.TilesDir };
        foreach (var asset in AssetDatabase.FindAssets("", unusedFolder))
        {
            var path = AssetDatabase.GUIDToAssetPath(asset);
            AssetDatabase.DeleteAsset(path);
        }
        AssetDatabase.DeleteAsset(MapToolPath.TextureColorSO);
        AssetDatabase.Refresh();
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
        //生成png
        foreach(var tcdata in curSO.tileColors)
        {
            GenPngEditor.CreateRhombusPng(tcdata.index, tcdata.color);
        }
        AssetDatabase.Refresh();
        //生成tile
        foreach (var tcdata in curSO.tileColors)
        {
            GenPngEditor.CreateRhombusTile(tcdata.index, tcdata.color);
        }
        AssetDatabase.Refresh();
        return true;
    }
}
