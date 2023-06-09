using UnityEngine;
using Color = System.Drawing.Color;

public class TextureColorScriptableObject : ScriptableObject
{
    public TextureColorData[] tileColors;
}

public class TextureColorData
{
    public int index;
    public Color color;

    public TextureColorData()
    {
        index = 0;
        color = Color.White;
    }
}
