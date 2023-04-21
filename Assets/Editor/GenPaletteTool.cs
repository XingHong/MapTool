using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenPaletteTool
{
    private static readonly string PalettesDir = "Assets/Palettes/";
    [MenuItem("MapTool/GenPaletteTest")]
    public static void CreatePalette()
    {
        GameObject go = GridPaletteUtility.CreateNewPalette(PalettesDir, "test", GridLayout.CellLayout.Isometric, GridPalette.CellSizing.Manual, new Vector3(1, 0.5f)
            , GridLayout.CellSwizzle.XYZ, TransparencySortMode.Default, new Vector3(0, 0, 1f));
        if (GridPaintPaletteWindow.instances.Count == 0)
        {
            GridPaintPaletteWindow.OpenTilemapPalette();
        }
        var owner = GridPaintPaletteWindow.instances[0];
        if (owner != null)
            owner.Focus();
        if (go != null)
        {
            Tilemap tm = go.GetComponentInChildren<Tilemap>();
            tm.tileAnchor = new Vector3(1, 1, 0);
            TilemapRenderer tmr = go.GetComponentInChildren<TilemapRenderer>();
            tmr.mode = TilemapRenderer.Mode.Individual;
            var tile = (TileBase)AssetDatabase.LoadAssetAtPath("Assets/Tiles/base00.asset", typeof(TileBase));
            tm.SetTile(Vector3Int.zero, tile);
            if (owner != null)
            {
                owner.palette = go;
                owner.Repaint();
                owner.SavePalette();
            }
            //else
            //{
            //    string path = AssetDatabase.GetAssetPath(go);
            //    var instance = GameObject.Instantiate<GameObject>(go);
            //    PrefabUtility.SaveAsPrefabAssetAndConnect(instance, path, InteractionMode.AutomatedAction);
            //    GameObject.DestroyImmediate(instance);
            //    AssetDatabase.Refresh();
            //}
        }
    }
}
