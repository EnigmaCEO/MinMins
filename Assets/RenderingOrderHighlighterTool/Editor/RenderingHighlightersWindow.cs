//Zenith Code 2014
using UnityEditor;
using UnityEngine;

public class RenderingHighlightersWindow : EditorWindow
{
    private RenderingHighlighters _renderingHighlightersData;

    private const int CUSTOM_NAME_MAX_LENGTH = 4;
    Vector2 _scroll = Vector2.zero;

    [MenuItem("Window/Rendering highlighters customizer")]
    private static void OpenWindow()
    {
        var window = GetWindow<RenderingHighlightersWindow>();
        window.title = "Highlighters";
    }

    void OnEnable()
    {
        LoadData();
    }

    private void LoadData()
    {
        _renderingHighlightersData = RenderingHighlighterUtility.RenderingHighlightersDataFile;
    }


    private void OnGUI()
    {
        if (_renderingHighlightersData == null)
        {
            return;
        }

        if (_renderingHighlightersData.RenderingHighlightersList == null)
        {
            return;
        }


        EditorGUILayout.BeginVertical();
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        GUILayout.Space(10);

        foreach (var renderingHighlighter in _renderingHighlightersData.RenderingHighlightersList)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.color = renderingHighlighter.sortingOrderTextColor;
            if (GUILayout.Button(string.Format("Sorting Layer Name: {0}", renderingHighlighter.LayerName), GUILayout.Width(250)))
            {
                Selection.instanceIDs = RenderingHighlighterUtility.GetObjectIDsOnLayer(renderingHighlighter.LayerName);
            }
            GUI.color = Color.white;
            renderingHighlighter.sortingOrderTextColor = EditorGUILayout.ColorField("Order in layer text color:",
                renderingHighlighter.sortingOrderTextColor, GUILayout.Width(250));
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField("Custom layer name:");
            string customName = renderingHighlighter.customLayerName;
            if (!string.IsNullOrEmpty(customName) && customName.Length > CUSTOM_NAME_MAX_LENGTH)
            {
                customName = customName.Substring(0, CUSTOM_NAME_MAX_LENGTH);
            }
            renderingHighlighter.customLayerName = EditorGUILayout.TextField(customName, GUILayout.Width(150));
            GUILayout.EndVertical();
            renderingHighlighter.layerTexture = (Texture2D)EditorGUILayout.ObjectField(renderingHighlighter.layerTexture,
                typeof(Texture2D), false, GUILayout.Width(100), GUILayout.Height(30));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();
        if (GUILayout.Button("Resync layers"))
        {
            RenderingHighlighterUtility.ResyncAssetFile();
        }


        EditorGUILayout.EndVertical();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_renderingHighlightersData);
            AssetDatabase.SaveAssets();
            RenderingHighlighterUtility.LoadAssetFile();
        }
    }
}