/****************************************************
 *  (c) 2012 narayana games UG (haftungsbeschränkt) *
 *  All rights reserved                             *
 ****************************************************/

//#define DEBUG_PERSIST_PLAYMODE_CHANGES

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;

using NarayanaGames.Common;

namespace NarayanaGames.UnityEditor.Common {
    /// <summary>
    ///     You can use this class in your customer editor scripts to support
    ///     persisting changes applied during play mode. It only works on the
    ///     component that the custom inspector is handling but supports changes
    ///     in multiple objects during one session.
    /// </summary>
    /// <remarks>
    ///     For a <strong>tutorial</strong> on persisting your changes made in play mode
    ///     see <a href="http://www.youtube.com/watch?v=7zAE6K2rdQE">
    ///         ScoreFlash-Tutorial: Persisting Changes after Playing</a>
    ///     <example>
    ///         The following additions are needed in your custom inspector
    ///         for changes in play mode to be persisted (if user tells the
    ///         component to do so ;-) ):
    ///         <code>
    ///using NarayanaGames.UnityEditor.Common; // on top of the file
    ///
    ///private static Dictionary&lt;Object, PlayModeChangesHelper&gt; playModeChangesHelpers = new Dictionary&lt;Object, PlayModeChangesHelper&gt;();
    ///private PlayModeChangesHelper PlayModeChangesHelper {
    ///    get {
    ///        if (!playModeChangesHelpers.ContainsKey(target)) {
    ///            playModeChangesHelpers[target] = new PlayModeChangesHelper(target, "PersistChanges_YourComponentName");
    ///        }
    ///        return playModeChangesHelpers[target];
    ///    }
    ///}
    ///
    ///void OnEnable() {
    ///    // ... whatever you need to do in OnEnable ;-)
    ///
    ///    PlayModeChangesHelper.InspectorEnabled(this.target);
    ///}
    ///
    ///public override void OnInspectorGUI() {
    ///    serializedObject.Update();
    ///
    ///    // DrawCustomGUI(); // ... or however you do this ;-)
    ///
    ///    // it is recommended that you put the persist GUI at the end of your inspector
    ///    PlayModeChangesHelper.DrawInspectorGUI(playModeChangesHelpers);
    ///
    ///    serializedObject.ApplyModifiedProperties();
    ///}
    ///
    ///         </code>
    ///     </example>
    /// </remarks>
    public class PlayModeChangesHelper {

        private static int counter = 0;
        private int counterForInstance = 0;

        private string componentName;
        private string persistChangesKey = "PersistChanges";
        /// <summary>
        ///     Should changes be kept after playing?
        /// </summary>
        public bool PersistChanges {
            get { return EditorGUIExtensions.GetEditorPrefsSettingBool(persistChangesKey, true); }
            set { EditorGUIExtensions.SetEditorPrefsSettingBool(persistChangesKey, value); }
        }

        private string PersistChangesImmediatelyKey {
            get { return string.Format("{0}Immediately", persistChangesKey); }
        }

        /// <summary>
        ///     Should the changes be written back into the object immediately or
        ///     only when calling <see cref="StoreChanges()"/>?
        /// </summary>
        public bool PersistChangesImmediately {
            get { return EditorGUIExtensions.GetEditorPrefsSettingBool(PersistChangesImmediatelyKey, false); }
            set { EditorGUIExtensions.SetEditorPrefsSettingBool(PersistChangesImmediatelyKey, value); }
        }

        private Object target;
        private Dictionary<int, PlayModeChangesHelper> allHelpers = new Dictionary<int, PlayModeChangesHelper>();

        private List<string> lastChanges = new List<string>();

        private bool hasChanges = false;
        /// <summary>
        ///     If there were changes applied during play mode, this returns
        ///     <c>true</c> after play mode. Read only.
        /// </summary>
        public bool HasChanges {
            get { return hasChanges; }
        }
        /// <summary>
        ///     Do any of the PlayModeChangesHelpers for the same type have changes?
        ///     Read only.
        /// </summary>
        public bool HasAnyOneChanges {
            get {
                bool hasAnyOneChanges = false;
                foreach (int obj in allHelpers.Keys) {
                    hasAnyOneChanges |= allHelpers[obj].HasChanges;
                }
                return hasAnyOneChanges;
            }
        }

        /// <summary>
        ///     Make sure "Keep changes" does not re-appear when the user
        ///     has already discarded changes by setting a flag which is
        ///     only reset on next play.
        /// </summary>
        private bool hasDiscardedChanges = false;

        private bool hasNotifiedUsersOfPendingChanges = false;

        /// <summary>
        ///     Cache to store field values while playing.
        /// </summary>
        private Dictionary<FieldInfo, object> cachedPropertyValues = new Dictionary<FieldInfo, object>();

        private bool catchMe = true;

        private bool firstInspectorGUIAfterEnable = true;

        /// <summary>
        ///     Creates a new PlayModeChangesHelper.
        /// </summary>
        /// <param name="persistChangesKey">
        ///     an optional key if you want to enable / disable changes just
        ///     for a specific object
        /// </param>
        /// <param name="target">the target this operates on</param>
        public PlayModeChangesHelper(Object target, string persistChangesKey = "PersistChanges") {
            this.componentName = target.GetType().Name;
            this.persistChangesKey = persistChangesKey;
            this.target = target;
            counterForInstance = counter++;
#if DEBUG_PERSIST_PLAYMODE_CHANGES
            Debug.Log(string.Format("Instantiated PlaymodeChangesHelper {0}={1}, {2}, {3}", target.name, target.GetInstanceID(), persistChangesKey, counterForInstance));
#endif
            EditorApplication.update += new EditorApplication.CallbackFunction(EditorUpdate);
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
            //EditorApplication.playModeStateChanged += new System.Action<PlayModeStateChange>(PlayModeChanged);
        }

        /// <summary>
        ///     Call this in 
        ///     <a href="http://docs.unity3d.com/Documentation/ScriptReference/ScriptableObject.OnEnable.html?from=Editor">OnEnable()</a> 
        ///     of your custom inspector.
        /// </summary>
        /// <param name="target">
        ///     the object being inspected, see 
        ///     <a href="http://docs.unity3d.com/Documentation/ScriptReference/Editor-target.html">Editor.target</a>
        /// </param>
        public void InspectorEnabled(Object target) {
#if DEBUG_PERSIST_PLAYMODE_CHANGES
            if (this.target != target || this.target.GetInstanceID() != target.GetInstanceID()) {
                Debug.Log(string.Format("New target: {0}, {1} ({2})", target.name, target.GetInstanceID(), counterForInstance));
            } else {
                Debug.Log(string.Format("Kept target: {0}, {1} ({2})", target.name, target.GetInstanceID(), counterForInstance));
            }
#endif
            this.target = target;
            firstInspectorGUIAfterEnable = true;
        }

        /// <summary>
        ///     HACK ALERT: Unity doesn't quite let me find out when it's "back from play mode" - except that
        ///     it calls PlayModeChanged one time before it actually has completed play mode, and one time after.
        ///     And here's the trick: the inspector gets re-instantiated; so I can catch Unity doing it ;-)
        ///     This needs to be called from the custom inspector's Awake() method!
        /// </summary>
        private void InspectorGUICalled() {
            if (firstInspectorGUIAfterEnable) {
                firstInspectorGUIAfterEnable = false;
                catchMe = true;
            }
        }

        /// <summary>
        ///     Draws the GUI both for setting whether or not changes should be
        ///     persisted (only while playing) and - if there are changes and
        ///     they were not stored - a button to store the changes.
        /// </summary>
        public void DrawInspectorGUI(Dictionary<int, PlayModeChangesHelper> allHelpers) {
            InspectorGUICalled();

            this.allHelpers = allHelpers;
            EditorGUILayout.Space();

            if (EditorApplication.isPlaying) {
                EditorGUIExtensions.PersistentToggle("Keep Changes after Play? "+counterForInstance, persistChangesKey, true);
                EditorGUIExtensions.PersistentToggle("Store immediately?", PersistChangesImmediatelyKey, false);
            } else {
                if (HasChanges) {
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Keep changes!")) {
                            StoreChanges();
                        }
                        if (GUILayout.Button("Discard changes!")) {
                            hasChanges = false;
                            hasDiscardedChanges = true;
                            lastChanges.Clear();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    if (HasAnyOneChanges) {
                        if (allHelpers.Count > 1) {
                            EditorGUIExtensions.LabelInfo(string.Format("There are more changed objects of type {0}, check console for details!", componentName));
                            ListAllRelatedToConsole();
                            ButtonApplyChangesForAll();
                        }
                    }
                } else {
                    if (HasAnyOneChanges) {
                        if (allHelpers.Count > 1) {
                            EditorGUIExtensions.LabelInfo(string.Format("There are other objects of type {0} which have changes after play mode, check console for details!", componentName));
                            ListAllRelatedToConsole();
                            ButtonApplyChangesForAll();
                        }
                    }
                }
            }
        }

        private void ListAllRelatedToConsole() {
            if (allHelpers.Count > 1) {
                if (GUILayout.Button("List all to console")) {
                    foreach (int instanceID in allHelpers.Keys) {
                        Object obj = EditorUtility.InstanceIDToObject(target.GetInstanceID());
                        Debug.Log(
                            string.Format("Click to select {0} ({1}), changed: {2}", obj.name, componentName, allHelpers[instanceID].HasChanges), 
                            obj);
                    }
                }
            }
        }

        private void ButtonApplyChangesForAll() {
            if (allHelpers.Count > 1) {
                if (GUILayout.Button("Keep changes of all!")) {
                    ApplyChangesForAll();
                }
            }
        }

        private void ApplyChangesForAll() {
            foreach (int obj in allHelpers.Keys) {
                allHelpers[obj].StoreChanges();
            }
        }

        private int framesSincePlay = 0;

        private void EditorUpdate() {
            /*
             * On each editor update, we cache all current values of our public 
             * fields (i.e. our "properties" exposed to the editor). We could be
             * smart and only do this whenever there is a change ... but ...
             * instead we are smart and work with significantly less code!
             */
            if (PersistChanges && EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode) {
#if DEBUG_PERSIST_PLAYMODE_CHANGES
                Debug.Log(string.Format("{5}.EditorUpdate(): isPlaying = {0}, isPlayingOrWillChangePlaymode = {1}, isPaused = {2}, isCompiling = {3}, catchMe = {4}",
                    EditorApplication.isPlaying,
                    EditorApplication.isPlayingOrWillChangePlaymode,
                    EditorApplication.isPaused,
                    EditorApplication.isCompiling,
                    catchMe,
                    counterForInstance));
#endif

                hasDiscardedChanges = false;
                hasNotifiedUsersOfPendingChanges = false;
                foreach (FieldInfo fieldInfo in target.GetType().GetFields()) {
                    if (fieldInfo.IsPublic && !fieldInfo.IsStatic) {
                        cachedPropertyValues[fieldInfo] = fieldInfo.GetValue(target);
                    }
                }
                framesSincePlay = 0;
            }

            if (!EditorApplication.isPlaying) {
                framesSincePlay++;
            }

#if DEBUG_PERSIST_PLAYMODE_CHANGES
            if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode &&  framesSincePlay > 55 && framesSincePlay < 63) 
                Debug.Log(string.Format("{0}.{1}.{2}.EditorUpdate() - framesSincePlay == {3}", target.name, target.GetType().Name, counterForInstance, framesSincePlay));
#endif

            if (PersistChanges && !hasChanges && !hasDiscardedChanges
                && !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode) {

                if (framesSincePlay == 60) {
#if DEBUG_PERSIST_PLAYMODE_CHANGES
                    Debug.Log(string.Format("{0}.{1}.{2}.EditorUpdate() - NOW TESTING", target.name, target.GetType().Name, counterForInstance));
#endif
                    TestAllForChanges();
                }
            }

            catchMe = false;
        }

        private void EditorApplication_playModeStateChanged(PlayModeStateChange obj) {
            PlayModeChanged();
        }

        private void PlayModeChanged() {
#if DEBUG_PERSIST_PLAYMODE_CHANGES
            Debug.Log(string.Format("{5} PlayModeChanged(): isPlaying = {0}, isPlayingOrWillChangePlaymode = {1}, isPaused = {2}, isCompiling = {3}, catchMe = {4}",
                EditorApplication.isPlaying,
                EditorApplication.isPlayingOrWillChangePlaymode,
                EditorApplication.isPaused,
                EditorApplication.isCompiling,
                catchMe,
                counterForInstance));
#endif

            if (hasDiscardedChanges) {
                return;
            }

            //TestAllForChanges();

            // when persisting on request, we can be a little more careful and only check
            // for changes once a changed object was clicked
            if (catchMe && PersistChanges && !PersistChangesImmediately
                && !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode) {

#if DEBUG_PERSIST_PLAYMODE_CHANGES
                    Debug.Log(string.Format("{5} Will call CheckForChanges(): isPlaying = {0}, isPlayingOrWillChangePlaymode = {1}, isPaused = {2}, isCompiling = {3}, catchMe = {4}",
                        EditorApplication.isPlaying,
                        EditorApplication.isPlayingOrWillChangePlaymode,
                        EditorApplication.isPaused,
                        EditorApplication.isCompiling,
                        catchMe,
                        counterForInstance));
#endif
                
                // are there changes?
                CheckForChanges();

                catchMe = false;
            }
        }

        private void TestAllForChanges() {
            // when persisting immediately, we can't wait for the user to click an item that was
            // changed ... so ... we just go for it ;-)
            if (PersistChanges
                && !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode) {
                foreach (PlayModeChangesHelper helper in allHelpers.Values) {
                    helper.CheckForChanges();
                    if (helper.HasChanges) {
                        framesSincePlay = 100; // we have what we need, no more delay needed ;-)
                        if (PersistChangesImmediately) {
                            helper.StoreChanges();
                        }
                    }
                }
            }
        }

        private void CheckForChanges() {
            RefreshTarget();

            lastChanges.Clear();
            hasChanges = false;

            if (hasDiscardedChanges) {
                return;
            }

            try {
                // first, let's find out if there were any changes - which is less trivial than one might think ;-)
                foreach (FieldInfo fieldInfo in cachedPropertyValues.Keys) {
                    object prePlayValue = fieldInfo.GetValue(target);
                    object playModeValue = cachedPropertyValues[fieldInfo];

                    //Debug.Log(string.Format("Testing ({0}): {1} != {2}", fieldInfo.Name, cachedPropertyValues[fieldInfo], fieldInfo.GetValue(target)));

                    if (playModeValue == null && prePlayValue == null) { // both are null: not changed
                        continue;
                    }
                    if ((playModeValue == null && prePlayValue != null) // one null, other not: changed
                        || (playModeValue != null && prePlayValue == null)
                        || !playModeValue.Equals(prePlayValue)) { // not not null, but different: most likely changed

                        // !valueCached.Equals(valueTarget) doesn't catch all cases, if both are not null:
                        if (playModeValue != null && prePlayValue != null) {
                            // animation curves might be the same even if Unity's "Equals" fails
                            if (fieldInfo.GetValue(target) is AnimationCurve) {
                                if (NGUtil.AreEqual((AnimationCurve)playModeValue, (AnimationCurve)prePlayValue)) {
                                    continue;
                                }
                            }
                            // lists and arrays also might return false for "Equals" but still have same elements
                            if (fieldInfo.GetValue(target) is IList) {
                                if (NGUtil.AreEqual((IList)playModeValue, (IList)prePlayValue)) {
                                    continue;
                                }
                            }
                        }
                        lastChanges.Add(
                            string.Format("{2}\t\t{3}",
                                target.name, target.GetType().Name, fieldInfo.Name,
                                NGUtil.ToReadableDiff(prePlayValue, playModeValue)));
                        hasChanges = true;
                    }
                }

                // if there are changes, let's store them
                if (hasChanges && !PersistChangesImmediately) {
                    if (!hasNotifiedUsersOfPendingChanges) {
                        Debug.Log(
                            string.Format(
                            "{0}.{1}: Remembering changes made during play mode but not writing them, yet.\n"
                            + "Click message in console to see full list of changes!\n\n"
                            + "{2}\n\n",
                            componentName, target.name, string.Join("\n", lastChanges.ToArray())), target);
                        hasNotifiedUsersOfPendingChanges = true;
                    }
                }
            } catch (System.Exception) {
                // silently fail ... this should rarely happen
            }
        }

        private void RefreshTarget() {
            // refresh target
#if DEBUG_PERSIST_PLAYMODE_CHANGES
            if (target != EditorUtility.InstanceIDToObject(target.GetInstanceID())) {
                Debug.Log(string.Format("Needed to restore target {0} (counterForInstance={1})", target.name, counterForInstance));
            }
#endif
            if (target != null) {
                target = EditorUtility.InstanceIDToObject(target.GetInstanceID());
            } else {
                //Debug.LogWarning("PlayModeChangesHelper.RefreshTarget() failed because no target was assigned.");
            }
        }

        /// <summary>
        ///     Stores changes that were remembered during play.
        /// </summary>
        public void StoreChanges() {
            // make sure we don't accidentally store discarded changes!
            if (hasDiscardedChanges) {
                return;
            }

            RefreshTarget();

            Undo.RecordObject(target, string.Format("Changes to {0}.{1} in Playmode", componentName, target.name));
            Debug.Log(
                string.Format(
                "{0}.{1}: Persisting changes made while playing in the editor\n"
                + "Click message in console to see full list of changes!\n\n"
                + "{2}\n\n",
                componentName, target.name, string.Join("\n", lastChanges.ToArray())),
                target);
            foreach (FieldInfo fieldInfo in cachedPropertyValues.Keys) {
                //Debug.Log(
                //    string.Format("{0}: Setting {1} from {2} to {3}", 
                //    target.name, fieldInfo.Name, fieldInfo.GetValue(target), cachedPropertyValues[fieldInfo]));
                fieldInfo.SetValue(target, cachedPropertyValues[fieldInfo]);
            }
            EditorUtility.SetDirty(target);
            hasChanges = false;
            hasDiscardedChanges = true; // make sure this does not re-happen ;-)
        }
    }
}