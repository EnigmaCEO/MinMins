/****************************************************
 *  (c) 2012 narayana games UG (haftungsbeschränkt) *
 *  All rights reserved                             *
 ****************************************************/

using UnityEngine;
using UnityEditor;

namespace NarayanaGames.UnityEditor.Common {
    /// <summary>
    ///     Base class for info on extensions. Provides methods for showing version info
    ///     and documentation. Override for each extension.
    /// </summary>
    public class ExtensionInfo : ScriptableObject {

        private const string DOCS_URLBASE = "http://narayana-games.net/";

        /// <summary>
        ///     Priority for the ScoreFlashMenu (-1000).
        /// </summary>
        public const int ScoreFlashMenuPriority = -1000; // or use 10000 to put at the end of Help menu!

        /// <summary>
        ///     Priority for the STUUIMenu (-1500).
        /// </summary>
        public const int STUUIMenuPriority = -1500; // or use 10000 to put at the end of Help menu!

        /// <summary>
        ///     Base Priority for menu entries under Game Object / Create Other for ScoreFlash.
        /// </summary>
        public const int ScoreFlashGameObjectMenuPriority = 2000;

        /// <summary>
        ///     Priority for the Report a Problem menu (-970).
        /// </summary>
        public const int NarayanaGamesLastEntryPriority = ScoreFlashMenuPriority + 30;


        /// <summary>
        ///     Menu that takes the user to a "report problem" page:
        ///     <em>Help/narayana games/Report a Problem</em>
        /// </summary>
        [MenuItem("Help/narayana games/Report a Problem", false, NarayanaGamesLastEntryPriority)]
        public static void ShowReportProblemPage() {
            ShowDocumentation("Contact/ReportAProblem.aspx");
        }

        /// <summary>
        ///     Show version info for a plugin.
        /// </summary>
        /// <param name="extensionName">the name of the plugin</param>
        /// <param name="version">the current version</param>
        /// <param name="built">the date this version was built</param>
        public static void ShowInfo(string extensionName, string version, string built) {
            string title = string.Format("Plugin Info: {0}", extensionName);

            string msg = string.Format(
                "{0} V{1}\nBuilt: {2}\n\n(c) 2012-2015, narayana games UG (haftungsbeschraenkt)\nnarayana-games.net\nAll rights reserved.",
                extensionName, version, built);

    		EditorUtility.DisplayDialog(title, msg, "Got it, thanks!");
    	}

        /// <summary>
        ///     Open a browser to show the documentation of a plugin.
        /// </summary>
        /// <param name="detailURL">the path to the documentation (without domain)</param>
        public static void ShowDocumentation(string detailURL) {
            Help.BrowseURL(string.Format("{0}{1}", DOCS_URLBASE, detailURL));
        }
    }
}