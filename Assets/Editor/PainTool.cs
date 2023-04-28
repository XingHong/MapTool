using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEditor.Tilemaps;
using UnityEngine.SceneManagement;
using TMPro;
using Color = System.Drawing.Color;
using System.Drawing;

public class PainTool
{
    private static readonly Vector3Int size = new Vector3Int(1025, 257, 0);         //高宽（这样才是正方形）
    private static readonly int screenshotLen = 1024;
    private static readonly List<Vector3Int> dirs = new List<Vector3Int>() { new Vector3Int(0,1,0), new Vector3Int(1,0,0), new Vector3Int(0,-1,0), new Vector3Int(-1,0,0)};
    private static Dictionary<Color, int> colorDict;

    public static void Pain(TextureColorScriptableObject tcSO)
    {
        CreateColorDict(tcSO);
        Bitmap bitmap = new Bitmap(MapToolPath.ScreenshotPngEX);
        var scene = EditorSceneManager.OpenScene(MapToolPath.MapSecene);
        Tilemap sceneTM = GetSceneTileMap(scene);
        sceneTM.ClearAllTiles();

        //生成瓦片
        Vector3Int[] positions = new Vector3Int[size.x * size.y];
        TileBase[] tileArray = new TileBase[positions.Length];
        for (int index = 0; index < positions.Length; index++)
        {
            positions[index] = ToTilePos(index / size.y, index % size.y);
            tileArray[index] = GetTileBase(bitmap, index / size.y, index % size.y); ;
        }
        sceneTM.SetTiles(positions, tileArray);
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.Refresh();
    }

    public static void ClearSceneTilemap()
    {
        var scene = EditorSceneManager.OpenScene(MapToolPath.MapSecene);
        Tilemap sceneTM = GetSceneTileMap(scene);
        sceneTM.ClearAllTiles();
        EditorSceneManager.SaveOpenScenes();
    }

    [MenuItem("MapTool/ClearEdgeInfo")]
    static void ClearEdgeInfo()
    {
        EditorSceneManager.OpenScene(MapToolPath.MapSecene);
    }

    [MenuItem("MapTool/RefreshEdgeInfo")]
    static void RefreshEdgeInfo()
    {
        var scene = EditorSceneManager.OpenScene(MapToolPath.MapSecene);
        Tilemap sceneTM = GetSceneTileMap(scene);
        int len = size.x * size.y;
        var prefab = (GameObject)AssetDatabase.LoadAssetAtPath(MapToolPath.MapInfoPrefab, typeof(GameObject));
        var root = GetCanvas(scene);
        for (int index = 0; index < len; ++index)
        {
            var pos = ToTilePos(index / size.y, index % size.y);
            string dirText = GetEdgeInfo(sceneTM, pos);
            if (dirText != "0000")
            {
                var worldPos = sceneTM.GetCellCenterWorld(pos);
                worldPos.y -= 0.25f;
                var go = GameObject.Instantiate<GameObject>(prefab);
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = dirText;
                go.hideFlags = HideFlags.HideAndDontSave;
                go.transform.SetParent(root.transform);
                go.transform.position = worldPos;
            }
        }
        EditorSceneManager.SaveOpenScenes();
    }

    public static List<ExportData> GenEdgeInfo()
    {
        List<ExportData> res = new List<ExportData>();
        var scene = EditorSceneManager.OpenScene(MapToolPath.MapSecene);
        Tilemap sceneTM = GetSceneTileMap(scene);
        int len = size.x * size.y;
        for (int index = 0; index < len; ++index)
        {
            var pos = ToTilePos(index / size.y, index % size.y);
            string dirText = GetEdgeInfo(sceneTM, pos);
            var curTile = sceneTM.GetTile(pos);
            var groundId = GetTileGroupId(curTile.name);
            var data = new ExportData();
            data.groupId = groundId;
            data.dirInfo = dirText == "0000" ? string.Empty : dirText;
            res.Add(data);
        }
        return res;
    }

    private static string GetEdgeInfo(Tilemap tileMap, Vector3Int pos)
    {
        var curTile = tileMap.GetTile(pos);
        string res = "";
        for (int i = 0; i < dirs.Count; ++i)
        {
            Vector3Int nextPos = pos + dirs[i];
            var tile = tileMap.GetTile(nextPos);
            if (tile == null)
            {
                res += "0";
            }
            else
            { 
                res += tile.name == curTile.name ? "0" : "1";
            }
        }
        return res;
    }

    private static int GetTileGroupId(string str)
    {
        if (str.Length <= 4)
        {
            Debug.LogError("Export data error, string too short!");
            return -1;
        }
        string numStr = str.Substring(4);
        return int.Parse(numStr);
    }

    private static void CreateColorDict(TextureColorScriptableObject tcSO)
    {
        colorDict = new Dictionary<Color, int>();
        foreach (var item in tcSO.tileColors)
        {
            colorDict.Add(item.color, item.index);
        }
    }

    public static Tilemap GetSceneTileMap(Scene scene)
    {
        
        foreach (var go in scene.GetRootGameObjects())
        {
            Tilemap res = go.GetComponentInChildren<Tilemap>();
            if (res != null)
                return res;
        }
        return null;
    }

    public static GameObject GetCanvas(Scene scene)
    {
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name == "Canvas")
                return go;
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

    private static TileBase GetTileBase(Bitmap bitmap, int x, int y)
    {
        var texturePos = ToTexturePos(x, y);
        Color color;
        if (texturePos.x >= 0 && texturePos.x < screenshotLen && texturePos.y >= 0 && texturePos.y < screenshotLen)
        {
            color = bitmap.GetPixel(texturePos.y, texturePos.x);     //宽高相反
        }
        else
        {
            int xx = Mathf.Min(texturePos.x, 1023);
            int yy = Mathf.Min(texturePos.y, 1023);
            color = bitmap.GetPixel(yy, xx);
            //return (TileBase)AssetDatabase.LoadAssetAtPath($"Assets/Tiles/Tile1.asset", typeof(TileBase));
        }
        return (TileBase)AssetDatabase.LoadAssetAtPath($"Assets/Tiles/Tile{colorDict[color]}.asset", typeof(TileBase));
    }

    private static Vector2Int ToTexturePos(int x, int y)
    {
        float px = x;
        float py = ((x % 2) * 0.5f + y) * 4;
        return new Vector2Int((int)px, (int)py);
    }
}
