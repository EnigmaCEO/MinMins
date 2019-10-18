using System;

[System.Serializable]
public class ValueHack
{
    public bool Enabled;
    public string Value;

    public float ValueAsFloat { get { return float.Parse(Value); } }
    public int ValueAsInt { get { return int.Parse(Value); } }

    public ValueHack(string value, bool enabled = false)
    {
        Enabled = enabled;
        Value = value;
    }

    public T GetValueAsEnum<T>()
    {
        return (T)Enum.Parse(typeof(T), Value);
    }
}
