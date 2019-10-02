/****************************************************
 *  (c) 2013 narayana games UG (haftungsbeschränkt) *
 *  All rights reserved                             *
 ****************************************************/

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using NarayanaGames.Common.UI;
using NarayanaGames.UnityEditor.Common;
using NarayanaGames.ScoreFlashComponent;

namespace NarayanaGames.UnityEditor.ScoreFlashComponent {
    /// <summary>
    ///     This is the custom inspector GUI for ScoreFlashLayout. It is sparsely 
    ///     documented because you don't really need to use any of its
    ///     methods. Unity uses this and it knows what it does ;-)
    /// </summary>
    [CustomEditor(typeof(ScoreFlashLayout))]
    [CanEditMultipleObjects()]
    public class ScoreFlashLayoutEditor : ScoreFlashEditorBase {

        #region Boring Stuff no one wants to see
        private SerializedProperty version;

        private SerializedProperty isTestAutogenerateMessages;
        private SerializedProperty testMessageDelaySeconds;

        private SerializedProperty isDesignMode;
        private SerializedProperty enableDeselectHack;
        private SerializedProperty designText;

        private SerializedProperty screenAlign;
        private SerializedProperty position;
        private SerializedProperty innerAnchor;
        private SerializedProperty lockInnerAnchor;
        private SerializedProperty minPaddingX;
        private SerializedProperty maxWidth;

        private SerializedProperty freezeOnRead;

        private SerializedProperty defaultScoreFlash;

        void OnEnable() {
            isTestAutogenerateMessages = serializedObject.FindProperty("isTestAutogenerateMessages");
            testMessageDelaySeconds = serializedObject.FindProperty("testMessageDelaySeconds");

            isDesignMode = serializedObject.FindProperty("isDesignMode");
            enableDeselectHack = serializedObject.FindProperty("enableDeselectHack");
            designText = serializedObject.FindProperty("designText");

            screenAlign = serializedObject.FindProperty("screenAlign");
            position = serializedObject.FindProperty("position");
            lockInnerAnchor = serializedObject.FindProperty("lockInnerAnchor");
            innerAnchor = serializedObject.FindProperty("innerAnchor");
            minPaddingX = serializedObject.FindProperty("minPaddingX");
            maxWidth = serializedObject.FindProperty("maxWidth");

            freezeOnRead = serializedObject.FindProperty("freezeOnRead");
            defaultScoreFlash = serializedObject.FindProperty("defaultScoreFlash");

            version = serializedObject.FindProperty("version");
            ScoreFlashLayout scoreFlashLayout = (ScoreFlashLayout)serializedObject.targetObject;
            if (scoreFlashLayout.UpgradeCheck()) {
                if (!string.IsNullOrEmpty(scoreFlashLayout.version) && !scoreFlashLayout.version.Equals(version.stringValue)) {
                    Debug.Log(string.Format("Handling upgrade in Editor, version: {0} -> {1}", version.stringValue, scoreFlashLayout.version));
                    version.stringValue = scoreFlashLayout.version;
                }

                serializedObject.ApplyModifiedProperties();
            }

            #region Code for persisting changes after play (Part 1/3)
            PlayModeChangesHelper.InspectorEnabled(this.target);
            #endregion Code for persisting changes after play (Part 1/3)

            SetSelected<ScoreFlashLayout>(true);
        }

        /// <summary>
        ///     Sets "unselected".
        /// </summary>
        public void OnDisable() {
            SetSelected<ScoreFlashLayout>(false);
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
                    playModeChangesHelpers[instanceID] = new PlayModeChangesHelper(target, "PersistChanges_Layout");
                }
                return playModeChangesHelpers[instanceID];
            }
        }
        #endregion Code for persisting changes after play (Part 2/3)

        /// <summary>
        ///     Renders a custom inspector for ScoreFlashLayout.
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
            PropertyField(isDesignMode, "Design in Scene View?");
            if (isDesignMode.boolValue) {
                PropertyField(designText, "Text for Design Mode");
                PropertyField(enableDeselectHack, "Enable 'Deselect Hack'");
            }

            PropertyField(screenAlign, "Screen Align");
            NiceVector2Field(position, "Position relative to Screen Align");

            PropertyField(lockInnerAnchor, "Lock Inner Anchor?");
            if (!lockInnerAnchor.boolValue) {
                PropertyField(innerAnchor, "Inner Anchor");
            } else {
                innerAnchor.intValue = screenAlign.intValue;
            }

            string text = "Padding left/right before Wrap";
            NGAlignment.HorizontalAlign horizontalAlign = NGAlignment.Horizontal((NGAlignment.ScreenAlign)innerAnchor.intValue);
            if (horizontalAlign == NGAlignment.HorizontalAlign.Left) {
                text = "Padding from Right Screen Border";
            } else if (horizontalAlign == NGAlignment.HorizontalAlign.Right) {
                text = "Padding from Left Screen Border";
            }
            PropertyField(minPaddingX, text);

            PropertyField(maxWidth, "Maximum Width of Messages");

            EditorGUILayout.Space();
            PropertyField(freezeOnRead, "Freeze on Read");

            EditorGUILayout.Space();
            PropertyField(defaultScoreFlash, "Default Score Flash");

            EditorGUILayout.Space();
            EditorGUI.indentLevel = 0;
            if (EditorGUIExtensions.PersistentFoldout("Testing", "TestingLayout", false)) {
                EditorGUI.indentLevel = 1;
                DrawTestingSection();
                EditorGUI.indentLevel = 0;
            }
        }

        private string textToSend = "Enter some text and hit Trigger Message";

        private void DrawTestingSection() {
            ScoreFlashLayout layout = (ScoreFlashLayout)target;
            if (layout.DefaultScoreFlash == null) {
                EditorGUIExtensions.LabelWarning("You need at least one instance of ScoreFlash in your scene!");

                if (GUILayout.Button("Create ScoreFlash instance!")) {
                    ScoreFlashManagerEditor.CreateScoreFlash();
                }

                return;
            }

            if (EditorApplication.isPlaying) {
                DrawTriggerMessage();
            }

            PropertyField(isTestAutogenerateMessages, "Autogenerate Messages?");

            if (isTestAutogenerateMessages.boolValue) {
                NiceFloatSlider(testMessageDelaySeconds, 0.1F, 15F, "Usual Delay Between Msgs");

                EditorGUIExtensions.TimeScaleSlider();
            }

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
                    ScoreFlashLayout layout = (ScoreFlashLayout)target;
                    layout.Push(textToSend);
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        #endregion Drawing the Custom GUI

    }
}