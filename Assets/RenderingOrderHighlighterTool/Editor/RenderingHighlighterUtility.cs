//Zenith Code 2014
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

public static class RenderingHighlighterUtility
{
    private static readonly Color defaultColor = new Color(43 / 255.0f, 210 / 255.0f, 204 / 255.0f, 255);
    private static RenderingHighlighters _renderingHighlightersDataFile;

    #region public API
    public static RenderingHighlighters RenderingHighlightersDataFile
    {
        get
        {
            if (_renderingHighlightersDataFile == null)
            {
                _renderingHighlightersDataFile = LoadAssetFile();
            }
            return _renderingHighlightersDataFile ?? (_renderingHighlightersDataFile = CreateNew());
        }
    }


    public static RenderingHighlighter GetRenderingHighlighterForLayer(string sortLayer)
    {
        foreach (var renderingHighlighter in RenderingHighlightersDataFile.RenderingHighlightersList)
        {
            if (renderingHighlighter.LayerName.Equals(sortLayer))
            {
                return renderingHighlighter;
            }
        }
        return GetDefaultRenderHighlighter(sortLayer);
    }


    public static void ResyncAssetFile()
    {
        string[] sortingLayerNames = GetSortingLayerNames();

        //Fill them up with defaults
        var newRenderingHighlighters = new List<RenderingHighlighter>(sortingLayerNames.Length);
        for (int i = 0; i < sortingLayerNames.Length; i++)
        {
            newRenderingHighlighters.Add(GetDefaultRenderHighlighter(sortingLayerNames[i]));
        }

        var renderingHighlighters = _renderingHighlightersDataFile.RenderingHighlightersList;
        for (int i = 0; i < newRenderingHighlighters.Count; i++)
        {
            RenderingHighlighter newRenderingHighlighter = newRenderingHighlighters[i];
            foreach (RenderingHighlighter renderingHighlighter in renderingHighlighters)
            {
                if (newRenderingHighlighter.LayerName != renderingHighlighter.LayerName)
                {
                    continue;
                }

                if (renderingHighlighter.layerTexture == null)
                {
                    bool isUsingDefaultTexture;
                    renderingHighlighter.layerTexture = GetCorrectTextureForLayer(newRenderingHighlighter.LayerName, out isUsingDefaultTexture);
                }

                //If custom layer was changed before - use the one set by the user
                if (!string.IsNullOrEmpty(renderingHighlighter.customLayerName))
                {
                    newRenderingHighlighter.customLayerName = renderingHighlighter.customLayerName;
                }
                newRenderingHighlighters[i] = renderingHighlighter;
            }
        }
        _renderingHighlightersDataFile.RenderingHighlightersList = newRenderingHighlighters;
    }


    public static List<RendererObject> GetRenderersObjectList()
    {
        var rendererObjects = new List<RendererObject>();

        //Process all sprite renderers
        var allSpriteRenderers = Resources.FindObjectsOfTypeAll(typeof(SpriteRenderer)) as SpriteRenderer[];
        if (allSpriteRenderers != null)
        {
            foreach (SpriteRenderer currentRenderer in allSpriteRenderers)
            {
                string sortLayer = currentRenderer.sortingLayerName;
                if (string.IsNullOrEmpty(sortLayer))
                {
                    sortLayer = RendererHighlighterConstants.DEFAULT_LAYER_NAME;
                }
                rendererObjects.Add(new RendererObject(currentRenderer.gameObject.GetInstanceID(), sortLayer,
                                                       currentRenderer.sortingOrder));
            }
        }


        //Process all vfx sorters
        var allVfxSorters = Resources.FindObjectsOfTypeAll(typeof(VFXSorter)) as VFXSorter[];
        if (allVfxSorters != null)
        {
            foreach (VFXSorter sorter in allVfxSorters)
            {
                var vfxSorter = sorter.GetComponent<VFXSorter>();
                string sortLayer = vfxSorter.sortingLayer;
                if (string.IsNullOrEmpty(sortLayer))
                {
                    sortLayer = RendererHighlighterConstants.DEFAULT_LAYER_NAME;
                }
                rendererObjects.Add(new RendererObject(sorter.gameObject.GetInstanceID(), sortLayer, vfxSorter.sortingOrder));
            }
        }
        return rendererObjects;
    }


    public static int[] GetObjectIDsOnLayer(string sortingLayerName)
    {
        var renderersObjectList = GetRenderersObjectList();
        return (
            from rendererObject
            in renderersObjectList
            where rendererObject.sortingLayerName == sortingLayerName
            select rendererObject.instanceId)
                .ToArray();
    }


    #region GetSortingLayers
    //Thanks to: http://answers.unity3d.com/questions/585108/how-do-you-access-sorting-layers-via-scripting.html
    public static string[] GetSortingLayerNames()
    {
        Type internalEditorUtilityType = typeof(InternalEditorUtility);
        PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
        return (string[])sortingLayersProperty.GetValue(null, new object[0]);
    }
    #endregion
    #endregion



    #region Helpers
    private static RenderingHighlighter GetDefaultRenderHighlighter(string sortLayer)
    {
        //If this layer is not available in tool yet, allow default values.
        bool usingDefaultValues;

        var defaultRenderingHighlighter = new RenderingHighlighter
        {
            LayerName = sortLayer,
            sortingOrderTextColor = GetCorrectTextColorForLayer(sortLayer),
            layerTexture =
                GetCorrectTextureForLayer(sortLayer, out usingDefaultValues)
        };

        //If using default values, we set a custom layer name - a substring of the original name
        if (usingDefaultValues)
        {
            defaultRenderingHighlighter.customLayerName = sortLayer.Substring(0, 3);
        }
        return defaultRenderingHighlighter;
    }


    public static RenderingHighlighters LoadAssetFile()
    {
        if (!File.Exists(RendererHighlighterConstants.assetFilePath))
        {
            Debug.LogWarning("File does not exist at " + RendererHighlighterConstants.assetFilePath);
        }

        _renderingHighlightersDataFile = AssetDatabase.LoadAssetAtPath(RendererHighlighterConstants.assetFilePath, typeof(RenderingHighlighters)) as RenderingHighlighters;
        if (_renderingHighlightersDataFile != null)
        {
            ResyncAssetFile();
        }
        return _renderingHighlightersDataFile;
    }


    private static RenderingHighlighters CreateNew()
    {
        var highlighters = ScriptableObject.CreateInstance<RenderingHighlighters>();
        highlighters.RenderingHighlightersList = new List<RenderingHighlighter>();

        string[] sortingLayerNames = GetSortingLayerNames();

        for (int i = 0; i < sortingLayerNames.Length - 1; i++)
        {
            highlighters.RenderingHighlightersList.Add(GetDefaultRenderHighlighter(sortingLayerNames[i]));
        }

        //	Create the asset.
        if (!Directory.Exists(RendererHighlighterConstants.baseResourcesPath))
        {
            Directory.CreateDirectory(RendererHighlighterConstants.baseResourcesPath);
        }

        AssetDatabase.CreateAsset(highlighters, RendererHighlighterConstants.assetFilePath);
        _renderingHighlightersDataFile = highlighters;

        //SaveAssetFile();
        return _renderingHighlightersDataFile;
    }


    private static void SaveAssetFile()
    {
        EditorUtility.SetDirty(_renderingHighlightersDataFile);
        AssetDatabase.SaveAssets();
    }


    private static Texture2D GetDefaultTexture()
    {
        var defaultTexture = AssetDatabase.LoadAssetAtPath(RendererHighlighterConstants.defaultIconPath, typeof(Texture2D)) as Texture2D;
        if (defaultTexture == null)
        {
            Debug.LogError(string.Format("Default icon cannot be found!! - Update the base path in RendererHighlighterConstants.cs"));
        }
        return defaultTexture;
    }



    private static Texture2D GetCorrectTextureForLayer(string sortLayer, out bool isUsingDefaultTexture)
    {
        isUsingDefaultTexture = true;
        if (_renderingHighlightersDataFile != null)
        {
            foreach (var renderingHighlighter in _renderingHighlightersDataFile.RenderingHighlightersList)
            {
                if (renderingHighlighter.LayerName.Equals(sortLayer))
                {
                    if (renderingHighlighter.layerTexture == null)
                    {
                        return GetDefaultTexture();
                    }

                    //Not using default values if texture is already set BUT is not the blank texture
                    if (!renderingHighlighter.layerTexture.name.Equals(RendererHighlighterConstants.BLANK))
                    {
                        isUsingDefaultTexture = false;
                    }
                    return renderingHighlighter.layerTexture;
                }
            }
        }

        return GetDefaultTexture();
    }


    private static Color GetCorrectTextColorForLayer(string sortLayer)
    {
        if (_renderingHighlightersDataFile != null)
        {
            foreach (var renderingHighlighter in _renderingHighlightersDataFile.RenderingHighlightersList)
            {
                if (renderingHighlighter.LayerName.Equals(sortLayer))
                {
                    return renderingHighlighter.sortingOrderTextColor;
                }
            }
        }
        return defaultColor;
    }
    #endregion
}