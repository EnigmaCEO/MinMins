using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using NarayanaGames.Common;
using NarayanaGames.UnityEditor.Common;
using System.Xml;
using NarayanaGames.ScoreFlashComponent;

namespace NarayanaGames.UnityEditor.ScoreFlashComponent {
    /// <summary>
    ///     Base class for all score flash editors / inspectors.
    /// </summary>
    public class ScoreFlashEditorBase : NGEditorBase {
        protected void SetSelected<T>(bool isSelected) where T : ScoreFlashBase {
            try {
                foreach (T selectedObject in serializedObject.targetObjects) {
                    selectedObject.IsSelected = isSelected;
                    selectedObject.IsMultiSelect = serializedObject.targetObjects.Length > 1;
                    if (isSelected) {
                        SceneView.onSceneGUIDelegate += selectedObject.OnSceneViewGUI;
                    } else {
                        SceneView.onSceneGUIDelegate -= selectedObject.OnSceneViewGUI;
                    }
                }
            } catch { } // if this fails - simply ignore!
        }

        /// <summary>
        ///     Draws the scene view.
        /// </summary>
        /// <typeparam name="T">Anything based on ScoreFlashBase</typeparam>
        /// <param name="sceneView">the SceneView</param>
        public void OnSceneViewGUI<T>(SceneView sceneView) where T : ScoreFlashBase {
            foreach (T selectedObject in serializedObject.targetObjects) {
                selectedObject.OnSceneViewGUI(sceneView);
            }
        }

    }
}