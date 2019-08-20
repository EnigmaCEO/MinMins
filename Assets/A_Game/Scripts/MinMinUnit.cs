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

    public int Strength = 1;
    public int Defense = 1;
    public int MaxHealth = 5;

    public int EffectScale = 1;

    public int Effect;
    public Types Type;
    public int[] Attacks;
}
