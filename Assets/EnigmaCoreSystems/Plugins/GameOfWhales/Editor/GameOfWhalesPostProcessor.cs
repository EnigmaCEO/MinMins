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
using UnityEditor.Callbacks;
using System.IO;
using System.Xml;


#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#if UNITY_2017_1_OR_NEWER
using UnityEditor.iOS.Xcode.Extensions;
#endif
#endif

public class GameOfWhalesPostProcessor {


#if UNITY_IOS
    [PostProcessBuild(9999)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            GameOfWhalesSettings settings = GameOfWhalesSettings.instance;

            string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            PlistElementDict rootDict = plist.root;
            string projPath = Path.Combine(pathToBuiltProject, Path.Combine("Unity-iPhone.xcodeproj",  "project.pbxproj"));


            PBXProject project = new PBXProject();
            project.ReadFromString(File.ReadAllText(projPath));

            string targetGUID = project.TargetGuidByName("Unity-iPhone");
            string projectString = project.WriteToString ();



            if (settings.notificationsEnabled)
            {
                var buildKey = "UIBackgroundModes";
                var backgroundModes = rootDict.CreateArray(buildKey);
				backgroundModes.AddString ("fetch");
                backgroundModes.AddString ("remote-notification");
                File.WriteAllText(plistPath, plist.WriteToString());

                projectString = projectString.Replace ("SystemCapabilities = {\n", "SystemCapabilities = {\n\t\t\t\t\t\t\tcom.apple.Push = {\n\t\t\t\t\t\t\t\tenabled = 1;\n\t\t\t\t\t\t\t};");
                File.WriteAllText(projPath, projectString);
            }
		
#if UNITY_2017_1_OR_NEWER
			if (settings.generateNotificationService)
			{
				Directory.CreateDirectory(pathToBuiltProject + "/NotificationService");
				File.Copy("Assets/GameOfWhales//NotificationService/NotificationServiceh", pathToBuiltProject + "/NotificationService/NotificationService.h");
				File.Copy("Assets/GameOfWhales/NotificationService/NotificationServicem", pathToBuiltProject + "/NotificationService/NotificationService.m");
				File.Copy("Assets/GameOfWhales/NotificationService/Infoplist", pathToBuiltProject + "/NotificationService/Info.plist");

				var pathToNotificationService = pathToBuiltProject + "/NotificationService";
				var notificationServicePlistPath = pathToNotificationService + "/Info.plist";
				PlistDocument notificationServicePlist = new PlistDocument();
				notificationServicePlist.ReadFromFile (notificationServicePlistPath);
				notificationServicePlist.root.SetString ("CFBundleShortVersionString", PlayerSettings.bundleVersion);
				notificationServicePlist.root.SetString ("CFBundleVersion", PlayerSettings.iOS.buildNumber.ToString ());
			
				var notificationServiceTarget = PBXProjectExtensions.AddAppExtension (project, targetGUID, "notificationservice", PlayerSettings.GetApplicationIdentifier (BuildTargetGroup.iOS) + "." + settings.notificationServiceBundlePostix, "NotificationService/Info.plist");
				project.AddFileToBuild (notificationServiceTarget, project.AddFile (pathToNotificationService + "/NotificationService.h", "NotificationService/NotificationService.h"));
				project.AddFileToBuild (notificationServiceTarget, project.AddFile (pathToNotificationService + "/NotificationService.m", "NotificationService/NotificationService.m"));
				project.AddFrameworkToProject (notificationServiceTarget, "NotificationCenter.framework", true);
				project.AddFrameworkToProject (notificationServiceTarget, "UserNotifications.framework", true);
				project.SetBuildProperty (notificationServiceTarget, "ARCHS", "$(ARCHS_STANDARD)");
				project.SetBuildProperty (notificationServiceTarget, "DEVELOPMENT_TEAM", PlayerSettings.iOS.appleDeveloperTeamID);
				notificationServicePlist.WriteToFile (notificationServicePlistPath);
				
				project.WriteToFile (projPath);
				plist.WriteToFile (plistPath);
			}
#endif
        }


    }


#endif

}
