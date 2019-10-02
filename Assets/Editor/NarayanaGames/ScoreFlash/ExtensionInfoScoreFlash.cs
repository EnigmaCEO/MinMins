/****************************************************
 *  (c) 2012 narayana games UG (haftungsbeschränkt) *
 *  All rights reserved                             *
 ****************************************************/

using UnityEngine;
using UnityEditor;

using NarayanaGames.UnityEditor.Common;
using NarayanaGames.ScoreFlashComponent;

namespace NarayanaGames.UnityEditor.ScoreFlashComponent {
    /// <summary>
    ///     Creates Unity editor menu items to show extension info and documentation for narayana games' Score Flash.
    ///     The help menu items appear in Unity's Help menu.
    /// </summary>
    public class ExtensionInfoScoreFlash : ExtensionInfo {

        /// <summary>
        ///     Menu item to open the documentation in a Web browser:
        ///     <em>Help/narayana games/First Steps with Score Flash</em>
        /// </summary>
        [MenuItem("Help/narayana games/First Steps with Score Flash", false, ScoreFlashMenuPriority)]
        public static void ShowFirstSteps() {
            ShowDocumentation("static/docs/assetstore/ScoreFlash/ScoreFlashBasics.pdf");
        }

        /// <summary>
        ///     Menu item to open ScoreFlash on the Asset Store:
        ///     <em>Help/narayana games/Check for Updates</em>
        /// </summary>
        [MenuItem("Help/narayana games/Check for Score Flash Updates", false, ScoreFlashMenuPriority + 1)]
        public static void OpenScoreFlashInAssetStore() {
            UnityEditorInternal.AssetStore.Open("content/4476");
        }


        /// <summary>
        ///     Menu item to open the documentation in a Web browser:
        ///     <em>Help/narayana games/Score Flash Product Page</em>
        /// </summary>
        [MenuItem("Help/narayana games/Score Flash Product Page", false, ScoreFlashMenuPriority + 2)]
        public static void ShowDocs() {
            ShowDocumentation("Products/ScoreFlash.aspx");
        }

        /// <summary>
        ///     Menu item to let the user join the discussion at the Unity forums:
        ///     <em>Help/narayana games/Score Flash on Unity Forums</em>
        /// </summary>
        [MenuItem("Help/narayana games/Score Flash on Unity Forums", false, ScoreFlashMenuPriority + 3)]
        public static void ShowForum() {
            Help.BrowseURL("http://forum.unity3d.com/threads/152517-RELEASED-Score-Flash-Easy-to-use-GUI-for-Scores-PowerUps-Achievements-Tutorials");
        }

        /// <summary>
        ///     Menu item to show an about box with the current version info:
        ///     <em>Help/narayana games/About Score Flash</em>
        /// </summary>
        [MenuItem("Help/narayana games/About Score Flash", false, ScoreFlashMenuPriority + 4)]
        public static void ShowInfo() {
            ShowInfo("Score Flash", ScoreFlashBase.VERSION, ScoreFlashBase.BUILT);
    	}

        /// <summary>
        ///     Menu item to link to Text Box addon:
        ///     <em>Help/narayana games/Score Flash Addons/Text Mesh Pro</em>
        /// </summary>
        [MenuItem("Help/narayana games/Score Flash Addons/Text Mesh Pro", false, ScoreFlashMenuPriority + 5)]
        public static void OpenAddonTextMeshPro() {
            UnityEditorInternal.AssetStore.Open("content/20178");
        }

        /// <summary>
        ///     Menu item to link to Text Box addon:
        ///     <em>Help/narayana games/Score Flash Addons/NoesisGUI</em>
        /// </summary>
        [MenuItem("Help/narayana games/Score Flash Addons/NoesisGUI", false, ScoreFlashMenuPriority + 6)]
        public static void OpenAddonNoesisGUI() {
            UnityEditorInternal.AssetStore.Open("content/20388");
        }

     //   /// <summary>
     //   ///     Menu item to link to Daikon Forge addon:
     //   ///     <em>Help/narayana games/Score Flash Addons/Daikon Forge</em>
     //   /// </summary>
     //   [MenuItem("Help/narayana games/Score Flash Addons/Daikon Forge", false, ScoreFlashMenuPriority + 7)]
     //   public static void OpenAddonDaikonForge() {
     //       UnityEditorInternal.AssetStore.Open("content/20368");
    	//}

        /// <summary>
        ///     Menu item to link to NGUI addon:
        ///     <em>Help/narayana games/Score Flash Addons/NGUI</em>
        /// </summary>
        [MenuItem("Help/narayana games/Score Flash Addons/NGUI", false, ScoreFlashMenuPriority + 8)]
        public static void OpenAddonNGUI() {
            UnityEditorInternal.AssetStore.Open("content/20386");
        }

        /// <summary>
        ///     Menu item to link to PlayMaker addon:
        ///     <em>Help/narayana games/Score Flash Addons/PlayMaker</em>
        /// </summary>
        [MenuItem("Help/narayana games/Score Flash Addons/PlayMaker", false, ScoreFlashMenuPriority + 9)]
        public static void OpenAddonPlayMaker() {
            UnityEditorInternal.AssetStore.Open("content/20391");
        }

        ///// <summary>
        /////     Menu item to link to EZ GUI addon:
        /////     <em>Help/narayana games/Score Flash Addons/EZ GUI</em>
        ///// </summary>
        //[MenuItem("Help/narayana games/Score Flash Addons/EZ GUI", false, ScoreFlashMenuPriority + 10)]
        //public static void OpenAddonEZGUI() {
        //    UnityEditorInternal.AssetStore.Open("content/20398");
        //}

        ///// <summary>
        /////     Menu item to link to SpriteManager 2 addon:
        /////     <em>Help/narayana games/Score Flash Addons/SpriteManager 2</em>
        ///// </summary>
        //[MenuItem("Help/narayana games/Score Flash Addons/SpriteManager 2", false, ScoreFlashMenuPriority + 11)]
        //public static void OpenAddonSM2() {
        //    UnityEditorInternal.AssetStore.Open("content/20397");
        //}

        /// <summary>
        ///     Menu item to link to Text Box addon:
        ///     <em>Help/narayana games/Score Flash Addons/Text Box</em>
        /// </summary>
        //[MenuItem("Help/narayana games/Score Flash Addons/Text Box", false, ScoreFlashMenuPriority + 12)]
        //public static void OpenAddonTextBox() {
        //    UnityEditorInternal.AssetStore.Open("content/20178");
        //}


        /// <summary>
        ///     Renders the name and current version number into a custom inspector.
        /// </summary>
        public static void ShowVersionInInspector() {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(string.Format("ScoreFlash V{0}", ScoreFlashBase.VERSION));
            }
            EditorGUILayout.EndHorizontal();
        }

    }
}