using UnityEditor;
using UnityEngine;
using DarkTonic.MasterAudio;

// ReSharper disable once CheckNamespace
[InitializeOnLoad]
public class MasterAudioWelcomeWindow : EditorWindow
{
    private const string Physics2DSymbol = "PHY2D_ENABLED";
    private const string Physics3DSymbol = "PHY3D_ENABLED";
    private const string AddresablesSymbol = "ADDRESSABLES_ENABLED";

    private const string ShowOnStartEditorPrefsKey = "DarkTonic.MasterAudio.WelcomeWindow.ShowOnStart";

    private static bool showOnStartPrefs
    { // Records the customer's preference to show the window on start or not.
        get { return EditorPrefs.GetBool(ShowOnStartEditorPrefsKey, true); }
        set { EditorPrefs.SetBool(ShowOnStartEditorPrefsKey, value); }
    }
    public bool showOnStart = true;

    [MenuItem("Window/Master Audio/Welcome Window", false, -2)]
    public static MasterAudioWelcomeWindow ShowWindow()
    {
        var window = GetWindow<MasterAudioWelcomeWindow>(false, "Welcome");
        var height = 278;

#if UNITY_2018_2_OR_NEWER
        height += 12;
#endif

        window.minSize = new Vector2(482, height);
		window.maxSize = new Vector2(482, height);
        window.showOnStart = true; // Can't check EditorPrefs when constructing window, so set this instead.
        return window;
    }

    [InitializeOnLoadMethod]
    private static void InitializeOnLoadMethod()
    {
        RegisterWindowCheck();
    }

    private static void RegisterWindowCheck()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.update += CheckShowWelcomeWindow;
        }
    }

    private static void CheckShowWelcomeWindow()
    {
        EditorApplication.update -= CheckShowWelcomeWindow;
        if (showOnStartPrefs)
        {
            ShowWindow();
        }
    }

    void OnGUI()
    {
        DTGUIHelper.ShowHeaderTexture(MasterAudioInspectorResources.LogoTexture);
        //DTGUIHelper.HelpHeader("http://www.dtdevtools.com/docs/masteraudio/MasterAudioManager.htm");

        DTGUIHelper.DrawUILine(DTGUIHelper.DividerColor);
        GUILayout.Label("Welcome to Master Audio for Unity! The buttons below are shortcuts to commonly used help options.", EditorStyles.textArea);
        DTGUIHelper.DrawUILine(DTGUIHelper.DividerColor);

        GUILayout.Label("Help", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Quick Starts", GUILayout.Width(90)))
        {
            Application.OpenURL("https://www.dtdevtools.com/docs/masteraudio/HowToCreateSFX.htm");
        }
        if (GUILayout.Button("Manual", GUILayout.Width(90)))
        {
            Application.OpenURL("https://www.dtdevtools.com/docs/masteraudio/TOC.htm");
        }
        if (GUILayout.Button("Videos", GUILayout.Width(90)))
        {
            Application.OpenURL("https://www.youtube.com/watch?v=Ue9waU8g0c0&index=1&list=PLW6fMWQDKB24osBmTuJd0IG8R5tOim6eV");
        }
        if (GUILayout.Button("Scripting API", GUILayout.Width(90)))
        {
            Application.OpenURL("http://www.dtdevtools.com/API/masteraudio/annotated.html");
        }
        if (GUILayout.Button("Support Forum", GUILayout.Width(100)))
        {
            Application.OpenURL("http://darktonic.freeforums.net/board/1/master-audio-aaa-sound-solution");
        }
        EditorGUILayout.EndHorizontal();
        DTGUIHelper.DrawUILine(DTGUIHelper.DividerColor);

        GUILayout.Label("Optional package support", EditorStyles.boldLabel);
        GUILayout.Label("Enable support for:");

        // physics 2D
        var enable2D = DTDefineHelper.DoesScriptingDefineSymbolExist(Physics2DSymbol);
        var new2D = GUILayout.Toggle(enable2D, " 2D Physics (" + Physics2DSymbol + ")");
        if (new2D != enable2D)
        {
            if (new2D)
            {
                DTDefineHelper.TryAddScriptingDefineSymbols(Physics2DSymbol);
            }
            else
            {
                DTDefineHelper.TryRemoveScriptingDefineSymbols(Physics2DSymbol);
            }
        }

        // physics 3D
        var enable3D = DTDefineHelper.DoesScriptingDefineSymbolExist(Physics3DSymbol);
        var new3D = GUILayout.Toggle(enable3D, " 3D Physics (" + Physics3DSymbol + ")");
        if (new3D != enable3D)
        {
            if (new3D)
            {
                DTDefineHelper.TryAddScriptingDefineSymbols(Physics3DSymbol);
            }
            else
            {
                DTDefineHelper.TryRemoveScriptingDefineSymbols(Physics3DSymbol);
            }
        }

#if UNITY_2018_2_OR_NEWER
        // Addressables
        var enableAddress = DTDefineHelper.DoesScriptingDefineSymbolExist(AddresablesSymbol);
        var newAddress = GUILayout.Toggle(enableAddress, " Addressables (" + AddresablesSymbol + ")");
        if (newAddress != enableAddress)
        {
            if (newAddress)
            {
                DTDefineHelper.TryAddScriptingDefineSymbols(AddresablesSymbol);
            }
            else
            {
                DTDefineHelper.TryRemoveScriptingDefineSymbols(AddresablesSymbol);
            }
        }
#endif

        DTGUIHelper.ShowLargeBarAlert("Enabling a package you do not have installed will cause a compile error and you will not be able to use this window to undo until you install the missing package.");

        DTGUIHelper.DrawUILine(DTGUIHelper.DividerColor);

        EditorGUILayout.BeginHorizontal();
        var show = showOnStartPrefs;
        var newShow = GUILayout.Toggle(show, " Show at start");
        if (newShow != show)
        {
            showOnStartPrefs = newShow;
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(new GUIContent("Email Support", "support@darktonic.com"), GUILayout.Width(100)))
        {
            Application.OpenURL("mailto:support@darktonic.com");
        }

        EditorGUILayout.EndHorizontal();
    }
}