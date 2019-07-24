using System.Collections;
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

    [HideInInspector] public int Rating = 0;
    [HideInInspector] public Modes Mode = Modes.None;
    [HideInInspector] public int SelecteLevelNumber = 0;
}
