using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenPngEditor
{
    private const int width = 256;
    private const int height = 256;

    private static readonly string PalettesDir = "Assets/Palettes/";

    [MenuItem("MapTool/GenRhombusPng")]
    public static void GenRhombusPng()
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        ChangePixels(texture);
        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/Resources/AutoGenPngs/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "Image11.png", bytes);
        AssetDatabase.Refresh();

        Sprite sp = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/Resources/AutoGenPngs/Image11.png", typeof(Sprite));
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sp;
        AssetDatabase.CreateAsset(tile, "Assets/Tiles/test.asset");
        AssetDatabase.Refresh();
    }

    [MenuItem("MapTool/Test")]
    static void ImportExample()
    {
        GameObject go = GridPaletteUtility.CreateNewPalette(PalettesDir, "test", GridLayout.CellLayout.Isometric, GridPalette.CellSizing.Manual, new Vector3(1, 0.5f)
            , GridLayout.CellSwizzle.XYZ, TransparencySortMode.Default, new Vector3(0, 0, 1f));
        var owner = GridPaintPaletteWindow.instances.Count > 0 ? GridPaintPaletteWindow.instances[0] : null;
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
            else
            {
                string path = AssetDatabase.GetAssetPath(go);
                var instance = GameObject.Instantiate<GameObject>(go);
                PrefabUtility.SaveAsPrefabAssetAndConnect(instance, path, InteractionMode.AutomatedAction);
                GameObject.DestroyImmediate(instance);               
                AssetDatabase.Refresh();
            }
        }
    }

    private static void ChangePixels(Texture2D texture)
    {
        //图的背景为透明
        FillBackGround(texture);

        int halfH = height / 2;
        //菱形的四个顶点
        int xa, ya, xb, yb, xc, yc, xd, yd;
        xa = 0;
        ya = halfH / 2;
        xb = width / 2;
        yb = 0;
        xc = width;
        yc = halfH / 2;
        xd = width / 2;
        yd = halfH;
        //连接四个顶点
        DrawLine(texture, xa, xb, ya, yb);
        DrawLine(texture, xb, xc, yb, yc);
        DrawLine(texture, xc, xd, yc, yd);
        DrawLine(texture, xd, xa, yd, ya);

        //从菱形中点开始填充颜色
        int cx, cy;
        cx = width / 2;
        cy = halfH / 2;
        FillColor(texture, cx, cy);
        texture.Apply();
    }

    private static void FillBackGround(Texture2D texture)
    {
        var pixels = texture.GetPixels();
        Color zero = new Color(0, 0, 0, 0);
        for (int index = 0; index < pixels.Length; ++index)
        {
            pixels[index] = zero;
        }
        texture.SetPixels(pixels);
    }

    //Bresenham's line algorithm
    private static void DrawLine(Texture2D texture, int x0, int x1, int y0, int y1)
    {
        bool steep = Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0);
        if (steep)
        {
            Swap(ref x0, ref y0);
            Swap(ref x1, ref y1);
        }
        if (x0 > x1)
        {
            Swap(ref x0, ref x1);
            Swap(ref y0, ref y1);
        }
        int deltax = x1 - x0;
        int deltay = Mathf.Abs(y1 - y0);
        int error = deltax / 2;
        int ystep;
        int y = y0;
        ystep = y0 < y1 ? 1 : -1;

        for (int x = x0; x <= x1; ++x)
        {
            if (steep)
            {
                Plot(texture, y, x);
            }
            else
            {
                Plot(texture, x, y);
            }
            error = error - deltay;
            if (error < 0)
            {
                y += ystep;
                error += deltax;
            }
        }
    }

    private static void Swap(ref int x, ref int y)
    {
        x ^= y;
        y ^= x;
        x ^= y;
    }

    private static void Plot(Texture2D texture, int x, int y)
    {
        texture.SetPixel(x, y, Color.black);
    }

    private static void FillColor(Texture2D texture, int x, int y)
    {
        var pixels = texture.GetPixels();
        List<bool> visisted = new List<bool>();
        for (int i = 0; i < pixels.Length; ++i)
        {
            Vector2Int v = GetVectorXY(i);
            Color color = texture.GetPixel(v.x, v.y);
            visisted.Add(color.a > 0 ? true : false);
        }

        int w = width;
        int h = height / 2;
        int[] dirs = new int[] {-1,0,1,0,-1 };
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(new Vector2Int(x, y));
        visisted[GetIndex(new Vector2Int(x, y))] = true;
        Plot(texture, x, y);

        while (q.Count > 0)
        {
            int len = q.Count;
            for (int i = 0; i < len; ++i)
            {
                Vector2Int cur = q.Dequeue();
                for (int k = 0; k < 4; ++k)
                {
                    int nx = cur.x + dirs[k];
                    int ny = cur.y + dirs[k + 1];
                    int nIndex = GetIndex(new Vector2Int(nx, ny));
                    if (nx >= 0 && nx < w && ny >= 0 && ny < h && !visisted[nIndex])
                    {
                        visisted[nIndex] = true;
                        q.Enqueue(new Vector2Int(nx, ny));
                        Plot(texture, nx, ny);
                    }
                }
            }
        }
    }

    private static Vector2Int GetVectorXY(int index)
    {
        return new Vector2Int(index % width, index / width);
    }

    private static int GetIndex(Vector2Int v)
    {
        return v.x + v.y * width;
    }
}
