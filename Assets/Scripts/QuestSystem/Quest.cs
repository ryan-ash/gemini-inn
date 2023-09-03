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
    [Header("Quest Info")]
    public string questName;
    public string questDescription;
    public List<string> Biomes;
    public AllianceType Alliance;
    public int successAttempts = 0;
    public float timeout = 0.0f;
    public float baseDuration = 15.0f;
    public bool successOnTimeout = false;

    [Header("Quest Difficulty & Traits")]
    [Range(0.0f, 1.0f)]
    public float BaseSuccessRate = 1.0f;
    public List<AbilityModifier> AbilityModifiers;
    public List<StatModifier> StatModifiers;

    [System.NonSerialized] [HideInInspector] public Vector3 questPosition;
    [HideInInspector] public QuestState questState = QuestState.NotStarted;
    [System.NonSerialized] [HideInInspector] public float questTimer = 0.0f;
    [System.NonSerialized] [HideInInspector] public float roadDuration = 0.0f;

    [System.NonSerialized] [HideInInspector] public QuestGroup questGroup;
    [System.NonSerialized] [HideInInspector] public Mission mission;
    [System.NonSerialized] [HideInInspector] public QuestInfo questInfo;
    [System.NonSerialized] [HideInInspector] public QuestLine questLine;
    [System.NonSerialized] [HideInInspector] public GroupOnMap groupOnMap;

    [System.NonSerialized] [HideInInspector] public Tile tile;

    [System.NonSerialized] [HideInInspector] public List<Quest> questsToFailOnStart;

    [HideInInspector] public int ID = 0;

    public Quest()
    {
        AbilityModifiers = new List<AbilityModifier>();
        StatModifiers = new List<StatModifier>();
        Biomes = new List<string>();
        questsToFailOnStart = new List<Quest>();
    }

    public static float CalculateModifier(Quest InQuest, Ability InAbility)
    {
        AbilityModifier Mod = InQuest.AbilityModifiers.Find(Ab => Ab.Type == InAbility.Type);
        if (Mod == null)
            return 0.0f;
        float Modifier = Mod.ModifierPerLevel * Mathf.Min(Mod.MaxLevel, InAbility.Level);
        return Modifier;
    }

    public static float CalculateModifier(Quest InQuest, Stat InStat)
    {
        StatModifier Mod = InQuest.StatModifiers.Find(St => St.Type == InStat.Type);
        if (Mod == null)
            return 0.0f;
        const float StatDelta = 25;
        float StatRequirenemt = (int)(Mod.Strength) * StatDelta;
        float StatCap = StatRequirenemt + StatDelta;
        float StatFloor = StatRequirenemt - StatDelta;
        float ModifierAlpha = Mathf.Max(InStat.Value - StatFloor, 0) / (StatDelta * 2);
        float Modifier = Mathf.Lerp(Mod.MaxFailureModifier, Mod.MaxSuccessModifier, ModifierAlpha);
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

    public void SetTile(Tile tile)
    {
        this.tile = tile;
    }

    public float CalculateSuccessRateWithGroupStats(GroupStats groupStats)
    {
        float SuccessRate = BaseSuccessRate;

        if (groupStats.abilities != null)
        {
            foreach (Ability ability in groupStats.abilities)
            {
                SuccessRate += CalculateModifier(this, ability);
            }
        }

        if (groupStats.stats != null)
        {
            foreach (Stat stat in groupStats.stats)
            {
                SuccessRate += CalculateModifier(this, stat);
            }
        }

        return SuccessRate;
    }

    public bool RollSuccessDice()
    {
        float SuccessRate = CalculateSuccessRateWithGroupStats(questGroup.groupStats);
        float Roll = Random.Range(0.0f, 1.0f);
        return Roll <= SuccessRate;
    }
}

public class QuestToStop
{
    public Mission mission;
    public Quest quest;
    public QuestState questResult;
    public bool isTimeout;

    public QuestToStop(Mission mission, Quest quest, QuestState questResult, bool isTimeout)
    {
        this.mission = mission;
        this.quest = quest;
        this.questResult = questResult;
        this.isTimeout = isTimeout;
    }
}

[System.Serializable]
public enum AllianceType
{
    None,
    Nature,
    Humans,
    Elves,
    Dwarves,
    Orcs,
    Goblins,
    Druids,
    Frogs,
    Mermaids
}