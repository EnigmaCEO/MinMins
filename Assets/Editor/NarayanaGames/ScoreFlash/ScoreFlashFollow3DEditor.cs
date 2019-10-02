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

namespace NarayanaGames.UnityEditor.ScoreFlashComponent {
    /// <summary>
    ///     This is the custom inspector GUI for ScoreFlashFollow3D. It is sparsely 
    ///     documented because you don't really need to use any of its
    ///     methods. Unity uses this and it knows what it does ;-)
    /// </summary>
    [CustomEditor(typeof(ScoreFlashFollow3D))]
    [CanEditMultipleObjects()]
    public class ScoreFlashFollow3DEditor : ScoreFlashEditorBase {

        #region Boring Stuff no one wants to see
        private SerializedProperty version;

        private SerializedProperty isTestAutogenerateMessages;
        private SerializedProperty pushLocally;
        private SerializedProperty testMessageDelaySeconds;
        private SerializedProperty scoreFlashTestInstances;

        private SerializedProperty targetRenderer;
        private SerializedProperty isDesignMode;
        private SerializedProperty enableDeselectHack;
        private SerializedProperty designText;

        private SerializedProperty keepStatic;
        private SerializedProperty leaveBehind;
        private SerializedProperty loseMomentum;
        private SerializedProperty screenPositionOffset;
        private SerializedProperty worldPositionOffset;
        private SerializedProperty innerAnchor;
        private SerializedProperty freezeOnRead;
        private SerializedProperty referenceCamera;
        private SerializedProperty defaultScoreFlash;

        void OnEnable() {
            isTestAutogenerateMessages = serializedObject.FindProperty("isTestAutogenerateMessages");
            pushLocally = serializedObject.FindProperty("pushLocally");
            testMessageDelaySeconds = serializedObject.FindProperty("testMessageDelaySeconds");
            scoreFlashTestInstances = serializedObject.FindProperty("scoreFlashTestInstances");

            targetRenderer = serializedObject.FindProperty("targetRenderer");

            isDesignMode = serializedObject.FindProperty("isDesignMode");
            enableDeselectHack = serializedObject.FindProperty("enableDeselectHack");
            designText = serializedObject.FindProperty("designText");

            keepStatic = serializedObject.FindProperty("keepStatic");
            leaveBehind = serializedObject.FindProperty("leaveBehind");
            loseMomentum = serializedObject.FindProperty("loseMomentum");
            screenPositionOffset = serializedObject.FindProperty("screenPositionOffset");
            worldPositionOffset = serializedObject.FindProperty("worldPositionOffset");
            innerAnchor = serializedObject.FindProperty("innerAnchor");
            freezeOnRead = serializedObject.FindProperty("freezeOnRead");
            referenceCamera = serializedObject.FindProperty("referenceCamera");
            defaultScoreFlash = serializedObject.FindProperty("defaultScoreFlash");

            version = serializedObject.FindProperty("version");
            ScoreFlashFollow3D scoreFlashFollow3D = (ScoreFlashFollow3D)serializedObject.targetObject;
            if (scoreFlashFollow3D.UpgradeCheck()) {
                Debug.Log(string.Format("Handling upgrade in Editor, version: {0} -> {1}", version.stringValue, scoreFlashFollow3D.version));
                version.stringValue = scoreFlashFollow3D.version;

                serializedObject.ApplyModifiedProperties();
            }

            #region Code for persisting changes after play (Part 1/3)
            PlayModeChangesHelper.InspectorEnabled(this.target);
            #endregion Code for persisting changes after play (Part 1/3)

            SetSelected<ScoreFlashFollow3D>(true);
        }

        /// <summary>
        ///     Sets "unselected".
        /// </summary>
        public void OnDisable() {
            SetSelected<ScoreFlashFollow3D>(false);
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
                    playModeChangesHelpers[instanceID] = new PlayModeChangesHelper(target, "PersistChanges_Follow3D");
                }
                return playModeChangesHelpers[instanceID];
            }
        }
        #endregion Code for persisting changes after play (Part 2/3)

        /// <summary>
        ///     Renders a custom inspector for ScoreFlashFollow3D.
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
            PropertyField(keepStatic, "Keep Static");

            EditorGUILayout.Space();
            PropertyField(targetRenderer, "Renderer of Target");

            if (!keepStatic.boolValue) {
                NiceFloatSlider(leaveBehind, 0F, 1F, "Leave Behind");
                NiceFloatSlider(loseMomentum, 0F, 1F, "Lose Momentum");
                if (leaveBehind.floatValue == 0 && loseMomentum.floatValue == 1) {
                    EditorGUIExtensions.LabelInfo("Messages are now locked on object");
                }

                EditorGUILayout.Space();
            }

            PropertyField(isDesignMode, "Design in Scene View?");
            if (isDesignMode.boolValue) {
                PropertyField(designText, "Text for Design Mode");
                PropertyField(enableDeselectHack, "Enable 'Deselect Hack'");
            }


#if UNITY_4_6 || UNITY_5_0 || UNITY_5_1
            EditorGUIUtility.LookLikeControls();
#endif
            NiceVector2Field(screenPositionOffset, "Screen Position Offset");
            NiceVector3Field(worldPositionOffset, "World Position Offset");

            EditorGUILayout.Space();

            PropertyField(innerAnchor, "Inner Anchor");

            EditorGUILayout.Space();
            PropertyField(freezeOnRead, "Freeze on Read");

            EditorGUILayout.Space();
            PropertyField(referenceCamera, "Reference Camera");

            EditorGUILayout.Space();
            PropertyField(defaultScoreFlash, "Default Score Flash");

            EditorGUILayout.Space();
            EditorGUI.indentLevel = 0;
            if (EditorGUIExtensions.PersistentFoldout("Testing", "TestingFollow3D", false)) {
                EditorGUI.indentLevel = 1;
                DrawTestingSection();
                EditorGUI.indentLevel = 0;
            }
        }

        private string textToSend = "Enter some text and hit Trigger Message";

        private void DrawTestingSection() {
            ScoreFlashFollow3D follow3D = (ScoreFlashFollow3D)target;
            if (!ScoreFlash.HasInstance || (follow3D.NextFlashInstanceForTestMessage == null && follow3D.DefaultScoreFlash == null)) {
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

                PropertyField(pushLocally, "Push locally?");

                if (!pushLocally.boolValue) {
                    NiceArrayWithFoldout(scoreFlashTestInstances, "ScoreFlash Instances for Testing");

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Ping")) {
                            EditorGUIUtility.PingObject(follow3D.NextFlashInstanceForTestMessage);
                        }
                        GUILayout.Space(1F);
                        GUILayout.Label(string.Format("Next ScoreFlash: {0}", follow3D.NextFlashInstanceForTestMessage.name));

                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                }

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
                    ScoreFlashFollow3D follow3D = (ScoreFlashFollow3D)target;
                    if (follow3D.pushLocally) {
                        follow3D.Push(textToSend);
                    } else {
                        follow3D.NextFlashInstanceForTestMessage.PushWorld(follow3D, textToSend);
                        follow3D.NextFlashInstanceForTesting();
                    }
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        #endregion Drawing the Custom GUI

    }
}