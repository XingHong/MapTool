using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        color = Color.blue;
    }
}
