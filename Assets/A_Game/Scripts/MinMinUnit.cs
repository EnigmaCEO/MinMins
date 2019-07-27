using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinMinUnit : MonoBehaviour
{
    public enum Types
    {
        Scouts,
        Bombers,
        Tanks,
        Destroyers,
        Healers
    }

    public UnitStats Stats;
    public int Effect;
    public Types Type;
    public int[] Attacks;
}
