﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStats : SingletonMonobehaviour<GameStats>
{
    public enum Modes
    {
        None,
        SinglePlayer,
        Pvp
    }

    [Header("Only for display. Set at runtime:")]
    public Modes Mode = Modes.None;
    public int SelecteLevelNumber = 0;
    public bool UsesAiForPvp = false;
    public bool IsEnjinLinked = false;
    public List<Vector3> PreparationPositions = new List<Vector3>();
    public List<string> TeamUnits = new List<string>();
    public int Rating = 0;
}
