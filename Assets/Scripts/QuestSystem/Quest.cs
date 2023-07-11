using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestState
{
    NotStarted,
    OnRoad,
    InProgress,
    Success,
    Failure
}

[System.Serializable]
public class Quest
{
    public string questName;
    public string questDescription;
    [Range(0.0f, 1.0f)]
    public float BaseSuccessRate = 1.0f;
    public List<AbilityModifier> AbilityModifiers;
    public List<StatModifier> StatModifiers;
    public Vector3 questPosition;
    public List<string> Biomes;
    public QuestState questState = QuestState.NotStarted;

    public AdventurerGroup adventureGroup;

    public Quest()
    {
        AbilityModifiers = new List<AbilityModifier>();
        StatModifiers = new List<StatModifier>();
        Biomes = new List<string>();
    }

    public static float CalculateModifier(Quest InQuest, Ability InAbility)
    {
        AbilityModifier Mod = InQuest.AbilityModifiers.Find(Ab => Ab.Type == InAbility.Type);
        if (Mod == null)
        {
            return 1.0f;
        }
        float Modifier = Mod.Modifier * Mathf.Lerp(1.0f, 2.0f, InAbility.Level / 100.0f);
        return Modifier;
    }

    public static float CalculateModifier(Quest InQuest, Stat InStat)
    {
        StatModifier Mod = InQuest.StatModifiers.Find(St => St.Type == InStat.Type);
        if (Mod == null || InStat.Value == 0)
        {
            return 1.0f;
        }
        float Modifier = Mod.Modifier * Mathf.Lerp(1.0f, 2.0f, InStat.Value / 100.0f);
        return Modifier;
    }

    public void SetState(QuestState inState)
    {
        questState = inState;
    }

    public void SetPosition(Vector3 inPosition)
    {
        questPosition = inPosition;
    }
}
