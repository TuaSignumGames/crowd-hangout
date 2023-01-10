using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TaggedColor
{
    public string tag;
    public Color color;

    public TaggedColor(string tag, Color color)
    {
        this.tag = tag;
        this.color = color;
    }
}
