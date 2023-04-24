using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEditor.Tilemaps;

public class PainTool
{
    private static readonly Vector3Int size = new Vector3Int(1025, 257, 0);         //高宽（这样才是正方形）
    private static readonly int screenshotLen = 1024;
    private static Dictionary<Color, int> colorDict;

    public static void Pain(TextureColorScriptableObject tcSO)
    {
        CreateColorDict(tcSO);
        var tex = (Texture2D)AssetDatabase.LoadAssetAtPath(MapToolPath.ScreenshotPng, typeof(Texture2D));

        Tilemap sceneTM = GetSceneTileMap();
        sceneTM.ClearAllTiles();
        Tilemap palettleTM = GetPaletteTileMap();

        //生成瓦片
        TileBase tb = palettleTM.GetTile(Vector3Int.zero);
        Vector3Int[] positions = new Vector3Int[size.x * size.y];
        TileBase[] tileArray = new TileBase[positions.Length];
        for (int index = 0; index < positions.Length; index++)
        {
            positions[index] = ToTilePos(index / size.y, index % size.y);
            tileArray[index] = GetTileBase(tex, index / size.y, index % size.y);
        }
        sceneTM.SetTiles(positions, tileArray);
        EditorSceneManager.SaveOpenScenes();
    }

    private static void CreateColorDict(TextureColorScriptableObject tcSO)
    {
        colorDict = new Dictionary<Color, int>();
        foreach (var item in tcSO.tileColors)
        {
            colorDict.Add(item.color, item.index);
        }
    }

    public static Tilemap GetSceneTileMap()
    {
        var scene = EditorSceneManager.OpenScene(MapToolPath.MapSecene);
        foreach (var go in scene.GetRootGameObjects())
        {
            Tilemap res = go.GetComponentInChildren<Tilemap>();
            if (res != null)
                return res;
        }
        return null;
    }

    public static Tilemap GetPaletteTileMap()
    {
        var owner = GridPaintPaletteWindow.instances[0];
        var palette = owner.palette;
        return palette.GetComponentInChildren<Tilemap>();
    }

    private static Vector3Int ToTilePos(int x, int y)
    {
        int basex = -(x / 2);
        int basey = -(x + 1) / 2;
        int tox = basex + y;
        int toy = basey - y;
        return new Vector3Int(tox, toy, 0);
    }

    private static TileBase GetTileBase(Texture2D tex, int x, int y)
    {
        var texturePos = ToTexturePos(x, y);
        Color color;
        if (texturePos.x >= 0 && texturePos.x < screenshotLen && texturePos.y >= 0 && texturePos.y < screenshotLen)
        {
            color = tex.GetPixel(texturePos.y, texturePos.x);     //宽高相反
        }
        else
        {
            int xx = Mathf.Min(texturePos.x, 1023);
            int yy = Mathf.Min(texturePos.y, 1023);
            color = tex.GetPixel(yy, xx);
        }
        return (TileBase)AssetDatabase.LoadAssetAtPath($"Assets/Tiles/Tile{colorDict[color]}.asset", typeof(TileBase));
    }

    private static Vector2Int ToTexturePos(int x, int y)
    {
        float px = screenshotLen - x;
        float py = ((x % 2) * 0.5f + y) * 4;
        return new Vector2Int((int)px, (int)py);
    }
}
