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
    [MenuItem("MapTool/PainTest")]
    public static void Pain()
    {
        Tilemap sceneTM = GetSceneTileMap();
        sceneTM.ClearAllTiles();
        Tilemap palettleTM = GetPaletteTileMap();

        //生成瓦片
        TileBase tb = palettleTM.GetTile(Vector3Int.zero);
        Vector3Int[] positions = new Vector3Int[size.x * size.y];
        TileBase[] tileArray = new TileBase[positions.Length];
        for (int index = 0; index < positions.Length; index++)
        {
            positions[index] = ToCustomPos(index / size.y, index % size.y);
            tileArray[index] = tb;
        }
        sceneTM.SetTiles(positions, tileArray);
    }

    public static Tilemap GetSceneTileMap()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
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

    private static Vector3Int ToCustomPos(int x, int y)
    {
        int basex = -(x / 2);
        int basey = -(x + 1) / 2;
        int tox = basex + y;
        int toy = basey - y;
        return new Vector3Int(tox, toy, 0);
    }
}
