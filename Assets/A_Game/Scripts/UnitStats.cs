using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class UnitStats
{
    public int Strength;
    public int Defense;
    public int Health;

    public int EffectScale;

    public int Exp;
    public int Level;

    public UnitStats() { }

    public UnitStats(UnitStats stats)
    {
        Clone(stats);
    }

    public UnitStats(string statsSerialized)
    {
        Deserialize(statsSerialized);
    }

    public string Serialized()
    {
        Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();
        settings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        string str = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.None, settings);
        Debug.Log("Serialized: " + str);
        return str;
    }

    public void Deserialize(string str)
    {
        UnitStats stats = (UnitStats)Newtonsoft.Json.JsonConvert.DeserializeObject(str, typeof(UnitStats));
        Clone(stats);
    }

    public void Clone(UnitStats stats)
    {
        Strength = stats.Strength;
        Defense = stats.Defense;
        Health = stats.Health;

        EffectScale = stats.EffectScale;

        Exp = stats.Exp;
        Level = stats.Level;
    }
}
