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

    public Modes Mode = Modes.None;
    public int SelecteLevelNumber = 0;
    public bool UsesAiForPvp = false;
    public bool IsEnjinLinked = false;
}
