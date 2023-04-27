using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenPaletteTool
{
    public static void CreatePalette(int len)
    {
        GameObject go = GridPaletteUtility.CreateNewPalette(MapToolPath.PalettesDir, "CustomPalette", GridLayout.CellLayout.Isometric, GridPalette.CellSizing.Manual, new Vector3(1, 0.5f)
            , GridLayout.CellSwizzle.XYZ, TransparencySortMode.Default, new Vector3(0, 0, 1f));
        var owner = GridPaintPaletteWindow.instances.Count > 0 ? GridPaintPaletteWindow.instances[0] : null;
        if (owner != null)
            owner.Focus();
        if (go != null)
        {
            Tilemap tm = go.GetComponentInChildren<Tilemap>();
            tm.tileAnchor = new Vector3(1, 1, 0);
            TilemapRenderer tmr = go.GetComponentInChildren<TilemapRenderer>();
            int size = (int)Mathf.Sqrt(len) + 1;
            for (int i = 0; i < len; ++i)
            { 
                var tile = (TileBase)AssetDatabase.LoadAssetAtPath($"Assets/Tiles/Tile{i}.asset", typeof(TileBase));
                Vector3Int pos = new Vector3Int(i / size, i % size);
                tm.SetTile(pos, tile);
            }
            if (owner != null)
            {
                owner.palette = go;
                owner.Repaint();
                owner.SavePalette();
            }
            else
            {
                string path = AssetDatabase.GetAssetPath(go);
                var instance = GameObject.Instantiate<GameObject>(go);
                PrefabUtility.SaveAsPrefabAssetAndConnect(instance, path, InteractionMode.AutomatedAction);
                GameObject.DestroyImmediate(instance);
                AssetDatabase.Refresh();
                GridPaintPaletteWindow.OpenTilemapPalette();
            }
        }
    }
}
