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
using System.Linq;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#if UNITY_2017_1_OR_NEWER
using UnityEditor.iOS.Xcode.Extensions;
#endif
#endif

public class GameOfWhalesPostProcessor{
#if UNITY_IOS
    [PostProcessBuild(9999)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
	        
	        
            GameOfWhalesSettings settings = GameOfWhalesSettings.instance;
	        string projPath = Path.Combine(pathToBuiltProject, Path.Combine("Unity-iPhone.xcodeproj", "project.pbxproj"));
	        
            string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");

	        {
		        PlistDocument plist = new PlistDocument();
		        plist.ReadFromString(File.ReadAllText(plistPath));
		        PlistElementDict rootDict = plist.root;
		        
		        PBXProject project = new PBXProject();
		        project.ReadFromString(File.ReadAllText(projPath));
		        string projectString = project.WriteToString();



		        if (settings.notificationsEnabled)
		        {
			        var buildKey = "UIBackgroundModes";
			        var backgroundModes = rootDict.CreateArray(buildKey);
			        backgroundModes.AddString("fetch");
			        backgroundModes.AddString("remote-notification");
			        File.WriteAllText(plistPath, plist.WriteToString());

			        projectString =
				        projectString.Replace("SystemCapabilities = {\n",
					        "SystemCapabilities = {\n\t\t\t\t\t\t\tcom.apple.Push = {\n\t\t\t\t\t\t\t\tenabled = 1;\n\t\t\t\t\t\t\t};");
			        File.WriteAllText(projPath, projectString);
		        }

	        }
	        
	        {
		        PBXProject project = new PBXProject();
		        project.ReadFromString(File.ReadAllText(projPath));
		        string projectString = project.WriteToString();
		        
				FileUtil.CopyFileOrDirectory("Assets/GameOfWhales/Plugins/IOS/GameOfWhalesBundle.bundle", pathToBuiltProject + "/GameOfWhalesBundle.bundle");
				const string GOW_UID_REPLACE = "{GWUID}";
				const string GOW_UID_FILE_REPLACE = "{GWUID_FILE}";
	
				string GOW_BUNDLE_FILE_UID = GetUID(projectString);
	
				AddToPlistString(ref projectString, "/* CustomTemplate */ = {", "children = (\n", "\t\t\t\t{GWUID_FILE} /* GameOfWhalesBundle.bundle */,\n"
					.Replace(GOW_UID_FILE_REPLACE, GOW_BUNDLE_FILE_UID));
				
				string GOW_BUNDLE_UID = GetUID(projectString);
	
	
				AddToPlistString(ref projectString, "/* Begin PBXFileReference section */\n", "", "\t\t{GWUID_FILE} /* GameOfWhalesBundle.bundle */ = {isa = PBXFileReference; lastKnownFileType = \"wrapper.plug-in\"; path = GameOfWhalesBundle.bundle; sourceTree = \"<group>\"; };\n"
					.Replace(GOW_UID_FILE_REPLACE, GOW_BUNDLE_FILE_UID));
				AddToPlistString(ref projectString, "/* Begin PBXResourcesBuildPhase section */", "files = (\n", "\t\t\t\t{GWUID} /* GameOfWhalesBundle.bundle in Resources */,\n"
					.Replace(GOW_UID_REPLACE, GOW_BUNDLE_UID));
				AddToPlistString(ref projectString, "/* Begin PBXBuildFile section */\n", "", "\t\t{GWUID} /* GameOfWhalesBundle.bundle in Resources */ = {isa = PBXBuildFile; fileRef = {GWUID_FILE} /* GameOfWhalesBundle.bundle */; };\n"
					.Replace(GOW_UID_FILE_REPLACE, GOW_BUNDLE_FILE_UID)
					.Replace(GOW_UID_REPLACE, GOW_BUNDLE_UID));
				
				File.WriteAllText(projPath, projectString);
	        }

#if UNITY_2017_1_OR_NEWER
			if (settings.generateNotificationService)
			{
				PBXProject project = new PBXProject();
				project.ReadFromString(File.ReadAllText(projPath));
				string projectString = project.WriteToString();
				PlistDocument plist = new PlistDocument();
				plist.ReadFromString(File.ReadAllText(plistPath));
				string targetGUID = project.TargetGuidByName("Unity-iPhone");
				
				Directory.CreateDirectory(pathToBuiltProject + "/NotificationService");
				File.Copy("Assets/GameOfWhales/NotificationService/NotificationServiceh", pathToBuiltProject + "/NotificationService/NotificationService.h");
				File.Copy("Assets/GameOfWhales/NotificationService/NotificationServicem", pathToBuiltProject + "/NotificationService/NotificationService.m");
				File.Copy("Assets/GameOfWhales/NotificationService/Infoplist", pathToBuiltProject + "/NotificationService/Info.plist");

				var pathToNotificationService = pathToBuiltProject + "/NotificationService";
				var notificationServicePlistPath = pathToNotificationService + "/Info.plist";
				PlistDocument notificationServicePlist = new PlistDocument();
				notificationServicePlist.ReadFromFile (notificationServicePlistPath);
				notificationServicePlist.root.SetString ("CFBundleShortVersionString", PlayerSettings.bundleVersion);
				notificationServicePlist.root.SetString ("CFBundleVersion", PlayerSettings.iOS.buildNumber.ToString ());
			
				var notificationServiceTarget =
 PBXProjectExtensions.AddAppExtension (project, targetGUID, "notificationservice", PlayerSettings.GetApplicationIdentifier (BuildTargetGroup.iOS) + "." + settings.notificationServiceBundlePostix, "NotificationService/Info.plist");
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

    private static System.Random random = new System.Random();
    private static string RandomUIDString()
    {
        int length = 24;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }


    private static string GetUID(string projectString)
    {
        string result = RandomUIDString();
        while(true)
        {
            if (projectString.IndexOf(result) == -1)
                break;

            result = RandomUIDString();
        }

        return result;
    }

    private static void AddToPlistString(ref string str, string first, string second, string insert)
    {
	    
        int startIndex = str.IndexOf(first);

	    if (startIndex == 0)
	    {
		    Debug.LogError("GameOfWhales::AddToPlistString cannot find '" + first + "'");
		    return;
	    }
	    
        int lenght = first.Length;
        if (second.Length > 0)
        {
            int secIndex = str.Substring(startIndex).IndexOf(second);
	        
	        if (startIndex == 0)
	        {
		        Debug.LogError("GameOfWhales::AddToPlistString cannot find '" + second + "' after '" + first + "'");
		        return;
	        }
            lenght = secIndex + second.Length;
        }
        str = str.Insert(startIndex + lenght, insert);
    }


#endif
}