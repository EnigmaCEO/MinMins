using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;


[InitializeOnLoad]
public class GameOfWhalesInitOnLoad : Editor{
    public static readonly string[] Symbols = new string[]{
        "GAME_OF_WHALES",
    };

    static GameOfWhalesInitOnLoad(){
        string definesString =
            PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        List<string> allDefines = definesString.Split(';').ToList();
        allDefines.AddRange(Symbols.Except(allDefines));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup,
            string.Join(";", allDefines.ToArray()));
    }
}