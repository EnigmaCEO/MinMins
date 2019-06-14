/*
 * Game Of Whales SDK
 *
 * https://www.gameofwhales.com/
 * 
 * Copyright © 2018 GameOfWhales. All rights reserved.
 *
 * Licence: https://github.com/Game-of-whales/GOW-SDK-UNITY/blob/master/LICENSE
 *
 */

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameOfWhalesSettings))]
public class GameOfWhalesSettingsEditor : UnityEditor.Editor {

	
    private GameOfWhalesSettings _settings;

    public override void OnInspectorGUI()
    {
        _settings = (GameOfWhalesSettings)target;

        if (_settings == null)
            return;

        _settings.gameID = EditorGUILayout.TextField("Game ID", _settings.gameID);
        _settings.androidProjectID = EditorGUILayout.TextField("Android Project ID", _settings.androidProjectID);
        _settings.debugLogging = EditorGUILayout.Toggle("Debug Logging", _settings.debugLogging);
        _settings.notificationsEnabled = EditorGUILayout.Toggle("Enable notifications", _settings.notificationsEnabled); 

#if UNITY_2017_1_OR_NEWER
		_settings.generateNotificationService = EditorGUILayout.Toggle("Notification Service", _settings.generateNotificationService); 

		if (_settings.generateNotificationService) {
			_settings.notificationServiceBundlePostix = EditorGUILayout.TextField("Bundle postfix", _settings.notificationServiceBundlePostix); 
		}
#endif


        SerializedObject serializedSettings = new UnityEditor.SerializedObject(_settings);
        serializedSettings.ApplyModifiedProperties();
        if (GUI.changed) {
            EditorUtility.SetDirty(_settings);
        }
    }


}
