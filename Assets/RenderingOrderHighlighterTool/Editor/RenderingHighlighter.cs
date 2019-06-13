//Zenith Code 2014
using System;
using UnityEngine;

[Serializable]
public class RenderingHighlighter
{
    public Texture2D layerTexture;
    public Texture2D layerSecondaryTexture;

    [SerializeField]
    private string _layerName = string.Empty;

    public string LayerName
    {
        get { return _layerName; }
        set { _layerName = value; }
    }

    public Color sortingOrderTextColor = Color.white;

    public string customLayerName = string.Empty;
}


