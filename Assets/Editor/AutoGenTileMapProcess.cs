using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Drawing;
using Color = System.Drawing.Color;
using System.IO;

public class AutoGenTileMapProcess
{
    private static TextureColorScriptableObject curSO;

    [MenuItem("MapTool/CreateTileMapByScreenshot")]
    public static void CreateTileMapByScreenshot()
    {
        EditorUtility.DisplayProgressBar("依据缩略图生成瓦片数据", "清理老数据", 0.1f);
        DeleteAllFilesInFolder();

        EditorUtility.DisplayProgressBar("依据缩略图生成瓦片数据", "读取缩略图", 0.3f);
        int errorCode = ReadScreenshot();
        bool success = errorCode == 0;
        if (!success)
        {
            switch (errorCode)
            {
                case -1:
                    ErrorDialog("screenshot error!", "数据生成", "数据生成失败，缩率图规格不符合", "ok");
                    break;
                case -2:
                    ErrorDialog("color count error!", "数据生成", "数据生成失败，颜色数量不符合（大于64种颜色）", "ok");
                    break;
                case -3:
                    ErrorDialog("screenshot png not exist!", "数据生成", "数据生成失败，缩率图不存在", "ok");
                    break;
            }
            return;
        }
        EditorUtility.DisplayProgressBar("依据缩略图生成瓦片数据", "生成自定义瓦片", 0.5f);
        success = GenColorTiles();
        if (success)
        {
            EditorUtility.DisplayProgressBar("依据缩略图生成瓦片数据", "生成调色板", 0.6f);
            GenPaletteTool.CreatePalette(curSO.tileColors.Length);
        }
        else
        {
            ErrorDialog("GenColorTiles fail!", "数据生成", "数据生成失败，瓦片数据有问题", "ok");
            return;
        }
        EditorUtility.DisplayProgressBar("依据缩略图生成瓦片数据", "生成场景", 0.7f);
        PainTool.Pain(curSO);
        EditorUtility.DisplayProgressBar("依据缩略图生成瓦片数据", "生成数据", 0.9f);
        //ExportTool.GenData();
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("数据生成", "数据生成成功", "ok");
    }

    private static void ErrorDialog(string errorStr, string title, string msg, string ok)
    {
        Debug.LogError(errorStr);
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog(title, msg, ok);
    }

    //[MenuItem("MapTool/ClearAllData")]
    static void DeleteAllFilesInFolder()
    {
        PainTool.ClearSceneTilemap();
        string[] unusedFolder = { MapToolPath.PalettesDir, MapToolPath.AutoGenPngsDir, MapToolPath.TilesDir };
        foreach (var asset in AssetDatabase.FindAssets("", unusedFolder))
        {
            var path = AssetDatabase.GUIDToAssetPath(asset);
            AssetDatabase.DeleteAsset(path);
        }
        AssetDatabase.DeleteAsset(MapToolPath.TextureColorSO);
        AssetDatabase.Refresh();
    }

    public static int ReadScreenshot()
    {
        if (!File.Exists(MapToolPath.ScreenshotPngEX))
            return -3;
        Bitmap bitmap = new Bitmap(MapToolPath.ScreenshotPngEX);
        if (bitmap.Width != bitmap.Height && bitmap.Width != 1024)
            return -1;
        int errorCode = SaveTextureColorAsset(bitmap);
        AssetDatabase.Refresh();
        return errorCode;
    }

    private static int SaveTextureColorAsset(Bitmap bitmap)
    {
        int errorCode = 0;
        var pixels = GetPixels(bitmap);
        HashSet<Color> colorSet = new HashSet<Color>(pixels);
        if (colorSet.Count > 64)
            return -2;
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
        return errorCode;
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
