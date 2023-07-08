using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    Strength = 1,
    Intelligence = 2,
    Speed = 3,
    Charisma = 4,
}

public enum AbilityType
{
    Healing = 1,
    RangeDamage = 2,
    CloseDamage = 3,
    Magic = 4,
}

[System.Serializable]
public class Stat
{
    public StatType Type;
    [Range(0, 100)]
    public int Value;
}

[System.Serializable]
public class StatModifier
{
    public StatType Type;
    [Range(0.0f, 1.0f)]
    public float Modifier;
}

[System.Serializable]
public class Ability
{
    public AbilityType Type;
    [Range(1, 100)]
    public int Level = 0;
}

[System.Serializable]
public class AbilityModifier
{
    public AbilityType Type;
    [Range(0.0f, 1.0f)]
    public float Modifier;
}
