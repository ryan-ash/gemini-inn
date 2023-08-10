using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

[System.Serializable]
public enum StatType
{
    Strength = 1,
    Intelligence = 2,
    Speed = 3,
    Charisma = 4,
}

[System.Serializable]
public enum StatRequirementStrength
{
    Weak = 1,
    Average = 2,
    Strong = 3,
}

[System.Serializable]
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
    [JsonConverter(typeof(StringEnumConverter))]
    public StatType Type;
    [Range(0, 100)]
    public int Value;
}

[System.Serializable]
public class StatModifier
{
    [JsonConverter(typeof(StringEnumConverter))]
    public StatType Type;
    [JsonConverter(typeof(StringEnumConverter))]
    public StatRequirementStrength Strength = StatRequirementStrength.Weak;
    [Range(0.0f, 1.0f)]
    public float MaxSuccessModifier = 0.1f;
    [Range(-1.0f, 0.0f)]
    public float MaxFailureModifier = 0.0f;
}

[System.Serializable]
public class Ability
{
    [JsonConverter(typeof(StringEnumConverter))]
    public AbilityType Type;
    [Range(1, 100)]
    public int Level = 0;
}

[System.Serializable]
public class AbilityModifier
{
    [JsonConverter(typeof(StringEnumConverter))]
    public AbilityType Type;
    [Range(0.0f, 1.0f)]
    public float ModifierPerLevel = 0.1f;
    [Range(1, 10)]
    public int MaxLevel = 1;
}

// new logic for modifiers & success:
// - we have base success rate of the quest. it is configured by designer.
// - ability modifier are always > 1; they are applied to the base success rate for every ability level required by the modifier and present in group stats
// - stat modifiers have max success modifier & max failure modifier
// - while quest is in progress, each "use stat" time we roll a dice for each stat modifier
// - dice depends group stat value and stat requirement of the modifier
// - success moves needle towards max success modifier, failure moves needle towards max failure modifier
// - in the end, we apply all modifiers to the base success rate



