using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Drawing;
using Color = System.Drawing.Color;

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
        Bitmap bitmap = new Bitmap(MapToolPath.ScreenshotPngEX);
        SaveTextureColorAsset(bitmap);
        AssetDatabase.Refresh();
    }

    private static void SaveTextureColorAsset(Bitmap bitmap)
    {
        var pixels = GetPixels(bitmap);
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

    private static List<Color> GetPixels(Bitmap bitmap)
    {
        List<Color> res = new List<Color>();
        for (int i = 0; i < bitmap.Width; ++i)
        {
            for (int j = 0; j < bitmap.Height; ++j)
            {
                res.Add(bitmap.GetPixel(i, j));
            }
        }
        return res;
    }

    private static bool GenColorTiles()
    {
        curSO = (TextureColorScriptableObject)AssetDatabase.LoadAssetAtPath(MapToolPath.TextureColorSO, typeof(TextureColorScriptableObject));
        if (curSO == null)
            return false;
        //生成png
        foreach(var tcdata in curSO.tileColors)
        {
            GenPngEditor.CreateRhombusPng(tcdata.index, TransformColor(tcdata.color));
        }
        AssetDatabase.Refresh();
        //生成tile
        foreach (var tcdata in curSO.tileColors)
        {
            GenPngEditor.CreateRhombusTile(tcdata.index, TransformColor(tcdata.color));
        }
        AssetDatabase.Refresh();
        return true;
    }

    private static UnityEngine.Color TransformColor(Color color)
    {
        return new UnityEngine.Color(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    }
}
