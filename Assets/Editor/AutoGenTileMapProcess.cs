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
        EditorUtility.DisplayProgressBar("依据缩略图生成瓦片数据", "清理老数据", 0.1f);
        DeleteAllFilesInFolder();
        EditorUtility.DisplayProgressBar("依据缩略图生成瓦片数据", "读取缩略图", 0.3f);
        ReadScreenshot();
        bool success = false;
        EditorUtility.DisplayProgressBar("依据缩略图生成瓦片数据", "生成自定义瓦片", 0.5f);
        success = GenColorTiles();
        if (success)
        {
            EditorUtility.DisplayProgressBar("依据缩略图生成瓦片数据", "生成调色板", 0.6f);
            GenPaletteTool.CreatePalette(curSO.tileColors.Length);
        }
        else
        {
            Debug.LogError("GenColorTiles fail!");
            EditorUtility.ClearProgressBar();
            return;
        }
        EditorUtility.DisplayProgressBar("依据缩略图生成瓦片数据", "生成场景", 0.7f);
        PainTool.Pain(curSO);
        EditorUtility.DisplayProgressBar("依据缩略图生成瓦片数据", "生成数据", 0.9f);
        ExportTool.GenData();
        EditorUtility.ClearProgressBar();
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
