using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GenPngEditor
{
    private const int width = 256;
    private const int height = 256;

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
        File.WriteAllBytes(dirPath + "Image1" + ".png", bytes);
        AssetDatabase.Refresh();
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
        texture.Apply();
    }

    //Bresenham's line algorithm
    private static void DrawLine(Texture2D texture, int x0, int x1, int y0, int y1)
    {
        bool steep = Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0);
        if (steep)
        {
            swap(ref x0, ref y0);
            swap(ref x1, ref y1);
        }
        if (x0 > x1)
        {
            swap(ref x0, ref x1);
            swap(ref y0, ref y1);
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
                plot(texture, y, x);
            }
            else
            {
                plot(texture, x, y);
            }
            error = error - deltay;
            if (error < 0)
            {
                y += ystep;
                error += deltax;
            }
        }
    }

    private static void swap(ref int x, ref int y)
    {
        x ^= y;
        y ^= x;
        x ^= y;
    }

    private static void plot(Texture2D texture, int x, int y)
    {
        texture.SetPixel(x, y, Color.black);
    }
}
