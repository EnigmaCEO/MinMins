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
    ///     This is the custom inspector GUI for ScoreFlashManager. It is sparsely 
    ///     documented because you don't really need to use any of its
    ///     methods. Unity uses this and it knows what it does ;-)
    /// </summary>
    [CustomEditor(typeof(ScoreFlashManager))]
    public class ScoreFlashManagerEditor : NGEditorBase, IComparer<ScoreFlash> {

        /// <summary>
        ///     Menu item to create a new instance of ScoreFlash:
        ///     <em>GameObject/Create Other/Score Flash Manager</em>
        /// </summary>
        [MenuItem("GameObject/Create Other/Score Flash", false, ExtensionInfo.ScoreFlashGameObjectMenuPriority)]
        public static void CreateScoreFlash() {
            if (GameObject.FindObjectsOfType(typeof(ScoreFlash)).Length > 0) {
                bool hasSingleton = false;
                foreach (ScoreFlash scoreFlashA in GameObject.FindObjectsOfType(typeof(ScoreFlash))) {
                    if (scoreFlashA.ensureSingleton) {
                        hasSingleton = true;
                        Debug.LogError(
                            string.Format(
                            "ScoreFlash '{0}' has ensureSingleton active - no more ScoreFlash instances allowed!",
                            scoreFlashA.name), scoreFlashA);
                    }
                }
                if (hasSingleton) {
                    return;
                }
            }
            GameObject scoreFlash = new GameObject();
            scoreFlash.name = "ScoreFlash";
            scoreFlash.AddComponent<ScoreFlash>();
            scoreFlash.transform.localPosition = Vector2.zero;

            ScoreFlashManager manager = (ScoreFlashManager)GameObject.FindObjectOfType(typeof(ScoreFlashManager));
            if (manager != null) {
                scoreFlash.transform.SetParent(manager.transform);
                // when adding to a manager, select the manager
                Selection.activeGameObject = manager.gameObject;
            } else {
                // when adding just this one, select this one ;-)
                Selection.activeGameObject = scoreFlash;
            }
        }

        /// <summary>
        ///     Menu item to create a new ScoreFlashLayout:
        ///     <em>GameObject/Create Other/Score Flash Layout</em>
        /// </summary>
        [MenuItem("GameObject/Create Other/Score Flash Layout", false, ExtensionInfo.ScoreFlashGameObjectMenuPriority + 10)]
        public static void CreateScoreFlashLayout() {
            GameObject scoreFlashLayout = new GameObject();
            scoreFlashLayout.name = "ScoreFlashLayout";
            scoreFlashLayout.AddComponent<ScoreFlashLayout>();
            scoreFlashLayout.transform.localPosition = Vector2.zero;
            if (Selection.activeGameObject != null) {
                scoreFlashLayout.transform.SetParent(Selection.activeGameObject.transform);
            }
            Selection.activeGameObject = scoreFlashLayout;
        }

        /// <summary>
        ///     Menu item to create a new ScoreFlashManager:
        ///     <em>GameObject/Create Other/Score Flash Manager</em>
        /// </summary>
        [MenuItem("GameObject/Create Other/Score Flash Manager", false, ExtensionInfo.ScoreFlashGameObjectMenuPriority + 20)]
        public static void CreateScoreFlashManager() {
            if (GameObject.FindObjectsOfType(typeof(ScoreFlashManager)).Length > 0) {
                Debug.LogError("There can only be one score flash manager in any scene!");
                return;
            }
            GameObject scoreFlashManager = new GameObject();
            scoreFlashManager.name = "ScoreFlashManager";
            scoreFlashManager.AddComponent<ScoreFlashManager>();
            scoreFlashManager.transform.localPosition = Vector2.zero;
            Selection.activeGameObject = scoreFlashManager;
        }


        private List<ScoreFlash> instances = new List<ScoreFlash>();


        private SerializedProperty ensureSingleton;

        void OnEnable() {
            ensureSingleton = serializedObject.FindProperty("ensureSingleton");
        }

        /// <summary>
        ///     Renders a GUI to manage multiple instances of ScoreFlash in a scene.
        /// </summary>
        public override void OnInspectorGUI() {
            serializedObject.Update();

            UpdateInstances();

#if UNITY_4_6 || UNITY_5_0 || UNITY_5_1
            EditorGUIUtility.LookLikeControls();
#endif

            if (instances.Count == 0) {
                if (GUILayout.Button("Create ScoreFlash instance!")) {
                    CreateScoreFlash();
                }
            }

            ScoreFlashManager me = (ScoreFlashManager)target;
            if (PrefabUtility.GetPrefabType(me) != PrefabType.None) {
                EditorGUIExtensions.LabelWarning("ScoreFlashManager must not be a prefab!");
                if (GUILayout.Button("Disconnect")) {
                    PrefabUtility.DisconnectPrefabInstance(me);
                }
            }

            string previousName = null;

            foreach (ScoreFlash scoreFlash in instances) {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Ping")) {
                        EditorGUIUtility.PingObject(scoreFlash);
                    }

                    if (GUILayout.Button("Duplicate")) {
                        GameObject copy = (GameObject)Instantiate(scoreFlash.gameObject);
                        copy.transform.SetParent(me.transform);
                        copy.name = string.Format("New {0}", scoreFlash.name);
                    }

                    if (GUILayout.Button("Copy Ref")) {
                        EditorGUIUtility.systemCopyBuffer = string.Format("ScoreFlashManager.Get(\"{0}\")", scoreFlash.name);
                    }

                    GUILayout.Space(1F);

                    GUILayout.Label(scoreFlash.name);

                    GUILayout.FlexibleSpace();

                    if (scoreFlash.transform.parent != me.transform) {
                        if (GUILayout.Button("Pull", GUILayout.Width(40F))) {
                            scoreFlash.transform.SetParent(me.transform);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (scoreFlash.name.Equals(previousName)) {
                    EditorGUIExtensions.LabelWarning(
                        "Name collision: No two instances of ScoreFlash may have the same name! "
                        + "You can fix this the hard way by using the button below - or pick some "
                        + "name that makes sense (preferred ;-) )!");
                    if (scoreFlash.TemporaryName == null || scoreFlash.TemporaryName.Trim().Equals(string.Empty)) {
                        scoreFlash.TemporaryName = scoreFlash.name;
                    }

                    scoreFlash.TemporaryName = GUILayout.TextField(scoreFlash.TemporaryName);

                    if (IsNameUnique(scoreFlash)) {
                        if (GUILayout.Button("Fix name!")) {
                            /*
                             * Trimming to make sure there are no empty spaces at the end or beginning
                             * which could give your trouble when using the name in ScoreFlashManager.Get(...)
                             */
                            scoreFlash.name = scoreFlash.TemporaryName.Trim();
                            Selection.activeGameObject = scoreFlash.gameObject;
                        }
                    } else {
                        EditorGUIExtensions.LabelWarning("Enter a unique name!");
                    }
                }
                previousName = scoreFlash.name;
                GUILayout.Space(1F);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(ensureSingleton, new GUIContent("Ensure Singleton?"));

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Singleton Documentation")) {
                    ExtensionInfo.ShowDocumentation("static/docs/assetstore/ScoreFlash/html/F_ScoreFlashManager_ensureSingleton.htm");
                }
            }
            EditorGUILayout.EndHorizontal();

            ExtensionInfoScoreFlash.ShowVersionInInspector();

            serializedObject.ApplyModifiedProperties();
        }

        private bool IsNameUnique(ScoreFlash scoreFlash) {
            bool isUnique = true;
            foreach (ScoreFlash other in instances) {
                isUnique &= !other.name.Equals(scoreFlash.TemporaryName);
            }
            return isUnique;
        }

        private void UpdateInstances() {
            ScoreFlash[] scoreFlashInstances = (ScoreFlash[])GameObject.FindObjectsOfType(typeof(ScoreFlash));
            instances.Clear();
            foreach (ScoreFlash scoreFlash in scoreFlashInstances) {
                instances.Add(scoreFlash);
            }
            instances.Sort(this);
        }

        /// <summary>
        ///     Compares to instances of ScoreFlash using their names.
        /// </summary>
        /// <param name="x">one instance</param>
        /// <param name="y">another instance</param>
        /// <returns>x.name.CompareTo(y.name) ... really! ;-)</returns>
        public int Compare(ScoreFlash x, ScoreFlash y) {
            if (x == null && y == null) {
                return 0;
            }
            if (x == null) {
                return -1;
            }
            if (y == null) {
                return 1;
            }
            return x.name.CompareTo(y.name);
        }
    }
}