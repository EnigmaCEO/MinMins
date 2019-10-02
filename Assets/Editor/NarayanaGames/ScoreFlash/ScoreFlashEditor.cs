/****************************************************
 *  (c) 2012 narayana games UG (haftungsbeschränkt) *
 *  All rights reserved                             *
 ****************************************************/

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using NarayanaGames.UnityEditor.Common;
using NarayanaGames.Common.UI;
using NarayanaGames.ScoreFlashComponent;

namespace NarayanaGames.UnityEditor.ScoreFlashComponent {
    /// <summary>
    ///     This is the custom inspector GUI for ScoreFlash. It is sparsely 
    ///     documented because you don't really need to use any of its
    ///     methods. Unity uses this and it knows what it does ;-)
    /// </summary>
    [CustomEditor(typeof(ScoreFlash))]
    public class ScoreFlashEditor : ScoreFlashEditorBase {

        #region Boring Stuff no one wants to see
        // HEY - don't you trust me? This *really* is boring stuff - see for yourself:
        private SerializedProperty version;

        private SerializedProperty isTestAutogenerateMessages;
        private SerializedProperty testMessageDelaySeconds;
        private SerializedProperty includeMessageSpam;
        private SerializedProperty includeVeryLongMessages;
        private SerializedProperty isTestForceHighDensity;
        private SerializedProperty isDesignMode;
        private SerializedProperty enableDeselectHack;
        private SerializedProperty designText;
        private SerializedProperty isDebugMode;
        private SerializedProperty currentDebugMode;

        private SerializedProperty rendering;
        private SerializedProperty skin;
        private SerializedProperty skinHighDensity;
        private SerializedProperty guiStyleName;
        private SerializedProperty font;
        private SerializedProperty fontHighDensity;
        private SerializedProperty scoreFlashRenderer;
        private SerializedProperty warmUpCount;
        private SerializedProperty customParent;

        private SerializedProperty screenAlign;
        private SerializedProperty position;
        private SerializedProperty innerAnchor;
        private SerializedProperty lockInnerAnchor;
        //private SerializedProperty paddingY;
        private SerializedProperty minPaddingX;
        private SerializedProperty maxWidth;
        private SerializedProperty timeReference;
        private SerializedProperty forceSingleMessageQueue;
        private SerializedProperty maxSimultanuousMessages;
        private SerializedProperty minDistanceBetweenMessages;
        private SerializedProperty spreadSpeed;
        private SerializedProperty spreadImmediately;
        private SerializedProperty replacePreviousMessage;

        private SerializedProperty forceOutlineOnMobile;
        private SerializedProperty disableOutlines;
        private SerializedProperty colorOutline;

        private SerializedProperty colorSelectionMode;
        private SerializedProperty colors;
        private SerializedProperty alphaFadeInMultiplier;
        private SerializedProperty alphaReadStartMultiplier;
        private SerializedProperty alphaReadEndMultiplier;
        private SerializedProperty alphaFadeOutMultiplier;

        private SerializedProperty fadeInTimeSeconds;
        private SerializedProperty fadeInColor;
        private SerializedProperty fadeInColorCurve;
        private SerializedProperty fadeInOffsetX;
        private SerializedProperty fadeInOffsetXCurve;
        private SerializedProperty fadeInOffsetY;
        private SerializedProperty fadeInOffsetYCurve;
        private SerializedProperty fadeInScale;
        private SerializedProperty fadeInScaleCurve;

        private SerializedProperty readTimeSeconds;
        private SerializedProperty readMinLengthCharsToAddTime;
        private SerializedProperty readAddTimeSeconds;
        private SerializedProperty readColorStart;
        private SerializedProperty readColorCurve;
        private SerializedProperty readColorEnd;
        private SerializedProperty readVelocityXCurve;
        private SerializedProperty readFloatRightVelocity;
        private SerializedProperty readVelocityCurve;
        private SerializedProperty readFloatUpVelocity;
        private SerializedProperty readScaleCurve;
        private SerializedProperty readScale;

        private SerializedProperty fadeOutTimeSeconds;
        private SerializedProperty fadeOutColorCurve;
        private SerializedProperty fadeOutColor;
        private SerializedProperty fadeOutVelocityXCurve;
        private SerializedProperty fadeOutFloatRightVelocity;
        private SerializedProperty fadeOutVelocityCurve;
        private SerializedProperty fadeOutFloatUpVelocity;
        private SerializedProperty fadeOutScaleCurve;
        private SerializedProperty fadeOutScale;
        private SerializedProperty fadeOutInitialRotationSpeed;
        private SerializedProperty fadeOutRotationAcceleration;

        private SerializedProperty ensureSingleton;

        void OnEnable() {
            // another great creative burst ... not :-/
            isTestAutogenerateMessages = serializedObject.FindProperty("isTestAutogenerateMessages");
            testMessageDelaySeconds = serializedObject.FindProperty("testMessageDelaySeconds");
            includeMessageSpam = serializedObject.FindProperty("includeMessageSpam");
            includeVeryLongMessages = serializedObject.FindProperty("includeVeryLongMessages");
            isTestForceHighDensity = serializedObject.FindProperty("isTestForceHighDensity");
            isDesignMode = serializedObject.FindProperty("isDesignMode");
            enableDeselectHack = serializedObject.FindProperty("enableDeselectHack");
            designText = serializedObject.FindProperty("designText");
            isDebugMode = serializedObject.FindProperty("isDebugMode");
            currentDebugMode = serializedObject.FindProperty("currentDebugMode");

            rendering = serializedObject.FindProperty("rendering");
            skin = serializedObject.FindProperty("skin");
            skinHighDensity = serializedObject.FindProperty("skinHighDensity");
            guiStyleName = serializedObject.FindProperty("guiStyleName");
            font = serializedObject.FindProperty("font");
            fontHighDensity = serializedObject.FindProperty("fontHighDensity");
            scoreFlashRenderer = serializedObject.FindProperty("scoreFlashRenderer");
            warmUpCount = serializedObject.FindProperty("warmUpCount");
            customParent = serializedObject.FindProperty("customParent");

            screenAlign = serializedObject.FindProperty("screenAlign");
            position = serializedObject.FindProperty("position");
            lockInnerAnchor = serializedObject.FindProperty("lockInnerAnchor");
            innerAnchor = serializedObject.FindProperty("innerAnchor");
            //paddingY = serializedObject.FindProperty("paddingY");
            minPaddingX = serializedObject.FindProperty("minPaddingX");
            maxWidth = serializedObject.FindProperty("maxWidth");

            timeReference = serializedObject.FindProperty("timeReference");
            forceSingleMessageQueue = serializedObject.FindProperty("forceSingleMessageQueue");
            maxSimultanuousMessages = serializedObject.FindProperty("maxSimultanuousMessages");
            minDistanceBetweenMessages = serializedObject.FindProperty("minDistanceBetweenMessages");
            spreadSpeed = serializedObject.FindProperty("spreadSpeed");
            spreadImmediately = serializedObject.FindProperty("spreadImmediately");
            replacePreviousMessage = serializedObject.FindProperty("replacePreviousMessage");

            forceOutlineOnMobile = serializedObject.FindProperty("forceOutlineOnMobile");
            disableOutlines = serializedObject.FindProperty("disableOutlines");
            colorOutline = serializedObject.FindProperty("colorOutline");

            colorSelectionMode = serializedObject.FindProperty("colorSelectionMode");
            colors = serializedObject.FindProperty("colors");
            alphaFadeInMultiplier = serializedObject.FindProperty("alphaFadeInMultiplier");
            alphaReadStartMultiplier = serializedObject.FindProperty("alphaReadStartMultiplier");
            alphaReadEndMultiplier = serializedObject.FindProperty("alphaReadEndMultiplier");
            alphaFadeOutMultiplier = serializedObject.FindProperty("alphaFadeOutMultiplier");

            fadeInTimeSeconds = serializedObject.FindProperty("fadeInTimeSeconds");
            fadeInColor = serializedObject.FindProperty("fadeInColor");
            fadeInColorCurve = serializedObject.FindProperty("fadeInColorCurve");
            fadeInOffsetX = serializedObject.FindProperty("fadeInOffsetX");
            fadeInOffsetXCurve = serializedObject.FindProperty("fadeInOffsetXCurve");
            fadeInOffsetY = serializedObject.FindProperty("fadeInOffsetY");
            fadeInOffsetYCurve = serializedObject.FindProperty("fadeInOffsetYCurve");
            fadeInScale = serializedObject.FindProperty("fadeInScale");
            fadeInScaleCurve = serializedObject.FindProperty("fadeInScaleCurve");

            readTimeSeconds = serializedObject.FindProperty("readTimeSeconds");
            readMinLengthCharsToAddTime = serializedObject.FindProperty("readMinLengthCharsToAddTime");
            readAddTimeSeconds = serializedObject.FindProperty("readAddTimeSeconds");
            readColorStart = serializedObject.FindProperty("readColorStart");
            readColorCurve = serializedObject.FindProperty("readColorCurve");
            readColorEnd = serializedObject.FindProperty("readColorEnd");
            readVelocityXCurve = serializedObject.FindProperty("readVelocityXCurve");
            readFloatRightVelocity = serializedObject.FindProperty("readFloatRightVelocity");
            readVelocityCurve = serializedObject.FindProperty("readVelocityCurve");
            readFloatUpVelocity = serializedObject.FindProperty("readFloatUpVelocity");
            readScaleCurve = serializedObject.FindProperty("readScaleCurve");
            readScale = serializedObject.FindProperty("readScale");

            fadeOutTimeSeconds = serializedObject.FindProperty("fadeOutTimeSeconds");
            fadeOutColorCurve = serializedObject.FindProperty("fadeOutColorCurve");
            fadeOutColor = serializedObject.FindProperty("fadeOutColor");
            fadeOutVelocityXCurve = serializedObject.FindProperty("fadeOutVelocityXCurve");
            fadeOutFloatRightVelocity = serializedObject.FindProperty("fadeOutFloatRightVelocity");
            fadeOutVelocityCurve = serializedObject.FindProperty("fadeOutVelocityCurve");
            fadeOutFloatUpVelocity = serializedObject.FindProperty("fadeOutFloatUpVelocity");
            fadeOutScaleCurve = serializedObject.FindProperty("fadeOutScaleCurve");
            fadeOutScale = serializedObject.FindProperty("fadeOutScale");
            fadeOutInitialRotationSpeed = serializedObject.FindProperty("fadeOutInitialRotationSpeed");
            fadeOutRotationAcceleration = serializedObject.FindProperty("fadeOutRotationAcceleration");

            ensureSingleton = serializedObject.FindProperty("ensureSingleton");

            version = serializedObject.FindProperty("version");
            ScoreFlash scoreFlash = (ScoreFlash)serializedObject.targetObject;
            if (scoreFlash.UpgradeCheck()) {
                Debug.Log(string.Format("Handling upgrade in Editor, version: {0} -> {1}", version.stringValue, scoreFlash.version));
                version.stringValue = scoreFlash.version;

                Debug.Log(string.Format("Field position {0} -> {1}", position.vector2Value, scoreFlash.position));
                position.vector2Value = scoreFlash.position;

                serializedObject.ApplyModifiedProperties();
            }


            #region Code for persisting changes after play (Part 1/3)
            PlayModeChangesHelper.InspectorEnabled(this.target);
            #endregion Code for persisting changes after play (Part 1/3)

            SetSelected<ScoreFlash>(true);
        }

        /// <summary>
        ///     Sets "unselected".
        /// </summary>
        public void OnDisable() {
            SetSelected<ScoreFlash>(false);

            // ARGH - I want the game view to be refreshed - but Unity seemingly has no API for that :-/
            // so ... I'll go for a terrible terrible hack:
            #region Hack to make GameView Refresh on Deselect
            try {
                bool saved = isDesignMode.boolValue;
                if (saved && enableDeselectHack.boolValue) { // only if we are actually in design mode
                    isDesignMode.boolValue = false;
                    isDesignMode.boolValue = true;
                    isDesignMode.boolValue = saved;

                    serializedObject.ApplyModifiedProperties();
                }
            } catch { } // the bad news is: the exception still bubbles through :-/
            #endregion
        }


        #endregion Boring stuff

        #region Code for persisting changes after play (Part 2/3)
        private static Dictionary<int, PlayModeChangesHelper> playModeChangesHelpers = new Dictionary<int, PlayModeChangesHelper>();
        private PlayModeChangesHelper PlayModeChangesHelper {
            get {
                int instanceID = target.GetInstanceID();
                if (!playModeChangesHelpers.ContainsKey(instanceID)) {
                    playModeChangesHelpers[instanceID] = new PlayModeChangesHelper(target, "PersistChanges_ScoreFlash");
                }
                return playModeChangesHelpers[instanceID];
            }
        }
        #endregion Code for persisting changes after play (Part 2/3)

        /// <summary>
        ///     Renders the default inspector or ScoreFlash custom inspector 
        ///     depending on whether <em>Use Default Inspector</em> is
        ///     checked.
        /// </summary>
        public override void OnInspectorGUI() {
            serializedObject.Update();

            #region Code for persisting changes after play (Part 3/3)
            PlayModeChangesHelper.DrawInspectorGUI(playModeChangesHelpers);
            #endregion Code for persisting changes after play (Part 3/3)

            bool useDefaultInspectorGUI = EditorGUIExtensions.PersistentToggle("Use Default Inspector", "UseDefaultInspectorGUI", false);
            showFullAPIDocumentation = EditorGUIExtensions.PersistentToggle("Show documentation", "ShowFullDocumentation", false);
            DrawFullDocumentation(serializedObject.targetObject.GetType().Name, 20);

            if (useDefaultInspectorGUI) {
                DrawDefaultInspector();
            } else {
                DrawCustomGUI();
            }

            ExtensionInfoScoreFlash.ShowVersionInInspector();

            serializedObject.ApplyModifiedProperties();
        }

        #region Drawing the Custom GUI
        private void DrawCustomGUI() {
            EditorGUILayout.Space();
            if (EditorGUIExtensions.PersistentFoldout("Main Layout", "MainLayout", true)) {
                EditorGUI.indentLevel = 1;
                DrawMainLayoutSection();
                EditorGUI.indentLevel = 0;
            }
            EditorGUILayout.Space();
            // when we have a problem in this area, open it until the user fixes the problem
            if (Mathf.Sign(spreadSpeed.floatValue) != Mathf.Sign(readFloatUpVelocity.floatValue)) {
                // this is probably too much for most users, hence it's commented out ;-)
                //EditorGUIExtensions.SetEditorPrefsSettingBool("PerformanceTweaks", true);
            }

            if (EditorGUIExtensions.PersistentFoldout("Readability and Performance Tweaks", "PerformanceTweaks", false)) {
                EditorGUI.indentLevel = 1;
                DrawReadabilityAndPerformanceSection();
                EditorGUI.indentLevel = 0;
            }
            EditorGUILayout.Space();
            if (EditorGUIExtensions.PersistentFoldout("Colors", "Colors", false)) {
                EditorGUI.indentLevel = 1;
                DrawColorsSection();
                EditorGUI.indentLevel = 0;
            }
            EditorGUILayout.Space();
            if (EditorGUIExtensions.PersistentFoldout("Fade In Phase", "FadeIn", true)) {
                EditorGUI.indentLevel = 1;
                DrawFadeInSection();
                EditorGUI.indentLevel = 0;
            }
            EditorGUILayout.Space();
            if (EditorGUIExtensions.PersistentFoldout("Reading Phase", "Read", true)) {
                EditorGUI.indentLevel = 1;
                DrawReadSection();
                EditorGUI.indentLevel = 0;
            }
            EditorGUILayout.Space();
            if (EditorGUIExtensions.PersistentFoldout("Fade Out Phase", "FadeOut", true)) {
                EditorGUI.indentLevel = 1;
                DrawFadeOutSection();
                EditorGUI.indentLevel = 0;
            }
            EditorGUILayout.Space();
            if (EditorGUIExtensions.PersistentFoldout("Testing", "Testing", false)) {
                EditorGUI.indentLevel = 1;
                DrawTestingSection();
                EditorGUI.indentLevel = 0;
            }

            if (((ScoreFlash)serializedObject.targetObject).transform.parent != null
                && ((ScoreFlash)serializedObject.targetObject).transform.parent.GetComponent<ScoreFlashManager>() != null) {
                // if this is a child of a ScoreFlashManager, we don't need it to be a singleton anymore :-)
                ensureSingleton.boolValue = false;
            } else {
                EditorGUILayout.Space();

                if (EditorGUIExtensions.PersistentFoldout("Advanced", "Advanced", false)) {
                    EditorGUI.indentLevel = 1;
                    DrawAdvancedSection();
                    EditorGUI.indentLevel = 0;
                }
            }
        }

        private string textToSend = "Enter some text and hit Trigger Message";

        private void DrawTestingSection() {
            if (EditorApplication.isPlaying) {
                DrawTriggerMessage();
            }

            PropertyField(isTestAutogenerateMessages, "Autogenerate Messages?");

            if (isTestAutogenerateMessages.boolValue) {
                NiceFloatSlider(testMessageDelaySeconds, 0.1F, 15F, "Usual Delay Between Msgs");
                PropertyField(includeMessageSpam, "Include Message Burst");
                PropertyField(includeVeryLongMessages, "Include Long Messages");
            }

            PropertyField(isTestForceHighDensity, "Force High Density?");

            EditorGUILayout.Space();

            PropertyField(isDebugMode, "Render Debug Information?");
            PropertyField(currentDebugMode, "Debug Mode");

            EditorGUILayout.Space();

            EditorGUIExtensions.TimeScaleSlider();
        }

        private void DrawTriggerMessage() {
            EditorGUILayout.BeginHorizontal();
            {
                textToSend = GUILayout.TextField(textToSend);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2F);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Show Message")) {
                    ((ScoreFlash)target).PushFromInspector(textToSend);
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private bool addDefaultStyles = false;
        private int guiStyleOption = 0;

        private void DrawMainLayoutSection() {
            PropertyField(rendering, "Rendering");

            ScoreFlash.RenderingType renderingType = (ScoreFlash.RenderingType)rendering.intValue;
            EditorGUILayout.Space();

            if (renderingType == ScoreFlash.RenderingType.UnityGUI_Font) {
                PropertyField(font, "Generic Font");
                PropertyField(fontHighDensity, "Font for High Density (Retina)");
                EditorGUILayout.Space();
            }

            ScoreFlashRendererBase scoreFlashRendererComponent = null;

            if (renderingType == ScoreFlash.RenderingType.CustomRenderer) {
                PropertyField(scoreFlashRenderer, "ScoreFlash Renderer");
                if (scoreFlashRenderer.objectReferenceValue != null) {
                    scoreFlashRendererComponent = (ScoreFlashRendererBase)scoreFlashRenderer.objectReferenceValue;

                    if (scoreFlashRendererComponent.RequiresCustomParent) {
                        PropertyField(customParent, "Custom Parent for Renderer Instances");
                    }

                }
                NiceIntSlider(warmUpCount, 0, 30, "Warm up Message Instantiation");
                
                EditorGUILayout.Space();

                if (isDesignMode.boolValue) {
                    PropertyField(font, "Generic Font (for design mode)");
                    fontHighDensity.objectReferenceValue = font.objectReferenceValue;
                    //PropertyField(fontHighDensity, "Font for High Density (Retina)");
                    EditorGUILayout.Space();
                }
            }

            // we only need to assign skins when we're either not using a custom scoreFlash renderer, or
            // we are using the one based on UnityGUI
            if (renderingType == ScoreFlash.RenderingType.UnityGUI_GUISkin
                || (scoreFlashRendererComponent != null && scoreFlashRendererComponent.UsesGUISkin)) {

                bool errorShown = false;
                PropertyField(skin, "Generic Skin");
                PropertyField(skinHighDensity, "Skin for High Density (Retina)");
                EditorGUILayout.Space();

                if (skin.objectReferenceValue != null) {
                    List<GUIContent> availableStyles = new List<GUIContent>();
                    foreach (GUIStyle customStyle in ((GUISkin)skin.objectReferenceValue).customStyles) {
                        availableStyles.Add(new GUIContent(customStyle.name));
                    }
                    if (addDefaultStyles) { // we should really only add those that kind of make sense ... in a way ;-)
                        availableStyles.Add(new GUIContent("---"));
                        availableStyles.Add(new GUIContent("Label"));
                        availableStyles.Add(new GUIContent("Button"));
                        availableStyles.Add(new GUIContent("TextField"));
                        availableStyles.Add(new GUIContent("TextArea"));
                    }
                    GUIContent addDefaultStylesContent = new GUIContent("Include default styles", "Check this to include default styles like Label, Button, TextField, TextArea for AvailableGUIStyles.");
                    addDefaultStyles = EditorGUILayout.Toggle(addDefaultStylesContent, addDefaultStyles);

                    GUIContent newGuiStyleOptionContent = new GUIContent("Available GUIStyles", "Custom and default GUI styles found in the GUISkin you have assigned");
                    int newGuiStyleOption = EditorGUILayout.Popup(newGuiStyleOptionContent, guiStyleOption, availableStyles.ToArray());
                    if (newGuiStyleOption != guiStyleOption) {
                        if ("---".Equals(availableStyles[newGuiStyleOption])) {
                            newGuiStyleOption++;
                        }
                        guiStyleName.stringValue = availableStyles[newGuiStyleOption].text;
                        guiStyleOption = newGuiStyleOption;
                    }
                    PropertyField(guiStyleName, "Name of GUIStyle");
                }

                if (skin.objectReferenceValue == null) {
                    EditorGUIExtensions.LabelWarning("Please assign a GUI skin!");
                } else if (((GUISkin)skin.objectReferenceValue).FindStyle(guiStyleName.stringValue) == null) {
                    EditorGUIExtensions.LabelWarning(
                        string.Format("Custom style '{0}' is missing in your GUI Skin!", guiStyleName.stringValue)
                        );
                    Color col = GUI.color;
                    GUI.color = new Color(0.6F, 1.0F, 0.6F);
                    if (GUILayout.Button("Help me, please!")) {
                        ExtensionInfo.ShowDocumentation("static/docs/assetstore/ScoreFlash/html/F_ScoreFlash_guiStyleName.htm");
                    }
                    GUI.color = col;
                    errorShown = true;
                }

                if (!errorShown && skinHighDensity.objectReferenceValue != null
                    && ((GUISkin)skinHighDensity.objectReferenceValue).FindStyle(guiStyleName.stringValue) == null) {
                    EditorGUIExtensions.LabelWarning(
                        string.Format("Custom style '{0}' is missing in your High Density Skin!", guiStyleName.stringValue)
                        );
                }

                EditorGUILayout.Space();
            }


            PropertyField(isDesignMode, "Design in Scene View?");
            if (isDesignMode.boolValue) {
                PropertyField(designText, "Text for Design Mode");
                PropertyField(enableDeselectHack, "Enable 'Deselect Hack'");
            }

            // NOTE: This isn't really great in the inspector - but now we have an awesome visual designer :-)
            PropertyField(screenAlign, "Screen Align");
#if UNITY_4_6 || UNITY_5_0 || UNITY_5_1
            EditorGUIUtility.LookLikeControls();
#endif
            NiceVector2Field(position, "Position relative to Screen Align");

            PropertyField(lockInnerAnchor, "Lock Inner Anchor?");
            if (!lockInnerAnchor.boolValue) {
                PropertyField(innerAnchor, "Inner Anchor");
            } else {
                innerAnchor.intValue = screenAlign.intValue;
            }

            //PropertyField(position, "Position relative to Screen Align");
            // only valid until version 3.0.0 - in 3.1, this was replaced with "position"
            //if ((ScoreFlash.ScreenAlign)screenAlign.intValue == ScoreFlash.ScreenAlign.TopCenter) {
            //    PropertyField(paddingY, "Padding from Top of Screen");
            //}
            //if ((ScoreFlash.ScreenAlign)screenAlign.intValue == ScoreFlash.ScreenAlign.BottomCenter) {
            //    PropertyField(paddingY, "Padding from Bottom of Screen");
            //}
            string text = "Padding left/right before Wrap";
            NGAlignment.HorizontalAlign horizontalAlign = NGAlignment.Horizontal((NGAlignment.ScreenAlign)innerAnchor.intValue);
            if (horizontalAlign == NGAlignment.HorizontalAlign.Left) {
                text = "Padding from Right Screen Border";
            } else if (horizontalAlign == NGAlignment.HorizontalAlign.Right) {
                text = "Padding from Left Screen Border";
            }
            PropertyField(minPaddingX, text);

            PropertyField(maxWidth, "Maximum Width of Messages");
        }

        private void DrawReadabilityAndPerformanceSection() {

            ScoreFlash.RenderingType renderingType = (ScoreFlash.RenderingType)rendering.intValue;

            PropertyField(timeReference, "Time Reference");

            EditorGUILayout.Space();
            NiceIntSlider(maxSimultanuousMessages, 1, 15, "Max Simultaneous Messages");
            PropertyField(forceSingleMessageQueue, "Always only use Single Message Queue");

            EditorGUILayout.Space();
            PropertyField(minDistanceBetweenMessages, "Min Distance between Msgs");
            PropertyField(spreadSpeed, "Spread Speed");
            PropertyField(spreadImmediately, "Spread Immediately");
            PropertyField(replacePreviousMessage, "Replace Previous Msg");
            
            if (Mathf.Sign(spreadSpeed.floatValue) != Mathf.Sign(readFloatUpVelocity.floatValue)) {
                EditorGUIExtensions.LabelWarning(
                    string.Format(
                    "Usually, you will want spreadSpeed and readFloatUpVelocity have the same sign (+/-). "
                    + " Currently, readFloatUpVelocity is {0}, you may want to adjust spreadSpeed accordingly!",
                    readFloatUpVelocity.floatValue
                    ));
                if (GUILayout.Button("Fix this for me, please!")) {
                    spreadSpeed.floatValue = Mathf.Sign(readFloatUpVelocity.floatValue) * Mathf.Abs(spreadSpeed.floatValue);
                }
            }
            EditorGUILayout.Space();

            // outlines handled by the renderer (if handled at all) when using custom renderers => hide

            if (renderingType != ScoreFlash.RenderingType.CustomRenderer) {
                PropertyField(disableOutlines, "Disable Outlines?");
                if (!disableOutlines.boolValue) {
                    PropertyField(forceOutlineOnMobile, "Render Outline on Mobile?");
                    PropertyField(colorOutline, "Outline Color");
                }
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (disableOutlines.boolValue) {
                        GUILayout.Label("=> 1 Drawcall per Message");
                    } else if (forceOutlineOnMobile.boolValue) {
                        GUILayout.Label("=> 9 Drawcalls per Message");
                    } else {
                        GUILayout.Label("=> Drawcalls per Msg: 1 on mobile, 9 on desktop");
                    }
                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndHorizontal();

            }
        }

        private void DrawColorsSection() {
            PropertyField(colorSelectionMode, "Color Selection Mode");
            if (colorSelectionMode.intValue == (int)ScoreFlash.ColorControl.Random
                || colorSelectionMode.intValue == (int)ScoreFlash.ColorControl.Sequence) {

                NiceArrayWithFoldout(colors, "Available Colors");
            }

            if (colorSelectionMode.intValue != (int)ScoreFlash.ColorControl.FadePhases) {
                NiceFloatSlider(alphaFadeInMultiplier, 0F, 1F, "Alpha Multiplier Fade In");
                NiceFloatSlider(alphaReadStartMultiplier, 0F, 1F, "Alpha Multiplier Read Start");
                NiceFloatSlider(alphaReadEndMultiplier, 0F, 1F, "Alpha Multiplier Read End");
                NiceFloatSlider(alphaFadeOutMultiplier, 0F, 1F, "Alpha Multiplier Fade Out");
            }
        }

        private void DrawFadeInSection() {
            PropertyField(fadeInTimeSeconds, "Fade In Time (sec)");
            EditorGUILayout.Space();
            if (colorSelectionMode.intValue == (int)ScoreFlash.ColorControl.FadePhases) {
                PropertyField(fadeInColor, "Initial Color");
            }
            PropertyField(fadeInColorCurve, "Color Initial to Read Start");
            EditorGUILayout.Space();
            PropertyField(fadeInOffsetX, "Initial Offset X");
            PropertyField(fadeInOffsetXCurve, "Offset X to 0 (no offset)");
            PropertyField(fadeInOffsetY, "Initial Offset Y");
            PropertyField(fadeInOffsetYCurve, "Offset Y to 0 (no offset)");
            EditorGUILayout.Space();
            PropertyField(fadeInScale, "Initial Scale");
            PropertyField(fadeInScaleCurve, "Scale from Initial to 1");
        }

        private void DrawReadSection() {
            PropertyField(readTimeSeconds, "Read Time (sec)");
            EditorGUILayout.Space();
            if (colorSelectionMode.intValue == (int)ScoreFlash.ColorControl.FadePhases) {
                PropertyField(readColorStart, "Read Start Color");
            }
            PropertyField(readColorCurve, "Color from Start to End");
            if (colorSelectionMode.intValue == (int)ScoreFlash.ColorControl.FadePhases) {
                PropertyField(readColorEnd, "Read End Color");
            }
            EditorGUILayout.Space();
            PropertyField(readVelocityXCurve, "Velocity X from 0 to Read End");
            PropertyField(readFloatRightVelocity, "Read End Float Right Velocity");
            PropertyField(readVelocityCurve, "Velocity Y from 0 to Read End");
            PropertyField(readFloatUpVelocity, "Read End Float Up Velocity");
            if (Mathf.Sign(spreadSpeed.floatValue) != Mathf.Sign(readFloatUpVelocity.floatValue)) {
                EditorGUIExtensions.LabelWarning("Check readability and Performance Tweaks section!");
            }

            EditorGUILayout.Space();
            PropertyField(readScaleCurve, "Scale from 1 to Read End");
            PropertyField(readScale, "Scale at Read End");

            EditorGUILayout.Space();
            if (EditorGUIExtensions.PersistentFoldout("Read Time Length Optimization", "AddTime", false)) {
                EditorGUI.indentLevel = 2;
                NiceIntSlider(readMinLengthCharsToAddTime, 3, 40, "Min Char Count to Add Time");
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(string.Format("{0:0.0}s: ", readTimeSeconds.floatValue) + "You can read this much in that time there".Substring(0, readMinLengthCharsToAddTime.intValue));
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
                PropertyField(readAddTimeSeconds, "Add to time if longer");
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(string.Format("{0:0.0}s: ", readTimeSeconds.floatValue + readAddTimeSeconds.floatValue)
                        + "You can read this much in that time there");
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel = 1;
            }
        }

        private void DrawFadeOutSection() {
            PropertyField(fadeOutTimeSeconds, "Fade Out Time (sec)");
            EditorGUILayout.Space();
            PropertyField(fadeOutColorCurve, "Color Read End to Final");
            if (colorSelectionMode.intValue == (int)ScoreFlash.ColorControl.FadePhases) {
                PropertyField(fadeOutColor, "Final Color");
            }
            EditorGUILayout.Space();
            PropertyField(fadeOutVelocityXCurve, "Velocity X from Read End to Final");
            PropertyField(fadeOutFloatRightVelocity, "Final Float Right Velocity");
            PropertyField(fadeOutVelocityCurve, "Velocity Y from Read End to Final");
            PropertyField(fadeOutFloatUpVelocity, "Final Float Up Velocity");
            EditorGUILayout.Space();
            PropertyField(fadeOutScaleCurve, "Scale from Read End to Final Scale");
            PropertyField(fadeOutScale, "Final Scale");
            EditorGUILayout.Space();
            PropertyField(fadeOutInitialRotationSpeed, "Fade Out Initial Rotation Speed");
            PropertyField(fadeOutRotationAcceleration, "Rotation Acceleration");
        }

        private void DrawAdvancedSection() {
            PropertyField(ensureSingleton, "Ensure Singleton?");
            if (!ensureSingleton.boolValue) {
                GUISkin skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
                GUIStyle labelWrap = new GUIStyle(skin.label);
                labelWrap.wordWrap = true;
                labelWrap.alignment = TextAnchor.UpperCenter;
                GUILayout.Label(
                    "When you don't use the Singleton-approach, ScoreFlash no longer ensures that "
                    + "there is only one instance of ScoreFlash in any given scene. "
                    + "So, using ScoreFlash.Push(...) will produce unpredictable "
                    + "results when you have multiple game objects with ScoreFlash "
                    + "attached in your scene (you don't know which one will be used). "
                    + "Instead, you should use a ScoreFlashManager!",
                    labelWrap
                    );
                if (GUILayout.Button("Open Documentation")) {
                    ExtensionInfo.ShowDocumentation("static/docs/assetstore/ScoreFlash/html/T_ScoreFlashManager.htm");
                }
            }
        }


        #endregion Drawing the Custom GUI
    }
}