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
using System.IO;
using System;
using UnityEngine;
using UnityEditor;

public class GameOfWhalesMenu{
    [MenuItem("Window/Game Of Whales", false, 0)]
    private static void OpenSettings(){
        if (GameOfWhalesSettings.instance == null){
            CreateSettings();
        }

        Selection.activeObject = GameOfWhalesSettings.instance;
    }


    private static void CreateSettings(){
        string path = "Assets/Resources/" + "GameOfWhalesSettings" + ".asset";

        Directory.CreateDirectory(Application.dataPath + "/Resources");

        if (File.Exists(path)){
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.Refresh();
        }

        var asset = ScriptableObject.CreateInstance<GameOfWhalesSettings>();
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.Refresh();

        AssetDatabase.SaveAssets();
    }
}