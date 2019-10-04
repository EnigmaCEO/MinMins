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

    [Header("Only for display. Set at runtime:")]
    public Modes Mode = Modes.None;
    public int SelectedLevelNumber = 0;

    public bool UsesAiForPvp = false;
    public bool IsEnjinLinked = false;
    public bool HasEnjinWeapon = false;
    public bool HasEnjinShield = false;
    public bool HasEnjinEnigmaToken = false;

    public List<Vector3> PreparationPositions = new List<Vector3>();
    public List<string> TeamUnits = new List<string>();

    public int Rating = 0;

    public List<string> UnitsDamagedInSingleDestroyerAction = new List<string>();
}
