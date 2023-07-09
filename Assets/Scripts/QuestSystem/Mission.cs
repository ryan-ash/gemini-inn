using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random=UnityEngine.Random;

[System.Serializable]
public class Mission
{
    public List<Quest> Quests;
    public Reward CompletionReward;

    public Mission()
    {
        Quests = new List<Quest>();
    }

    private static string textMockup = "Lorem ipsum dolor sit amet, consectetur adipi";

    public static Mission GenerateRandomMission()
    {
        Mission Generated = new Mission();
        int QuestsNum = Random.Range(2, 6);
        for (int I = 0; I < QuestsNum; ++I)
        {
            Quest Quest = new Quest();
            Quest.questName = "Quest " + Random.Range(0, 1000).ToString();
            Quest.questDescription = textMockup;
            Quest.Biomes.Add("Jungle");
            Quest.Biomes.Add("Forest");
            Quest.Biomes.Add("Lake");
            Quest.BaseSuccessRate = Random.Range(0.5f, 1.0f);
            int MaxAbilities = Enum.GetValues(typeof(AbilityType)).Length;
            int MaxStats = Enum.GetValues(typeof(StatType)).Length;

            int AbilitiesNum = Random.Range(1, MaxAbilities);
            int StatsNum = Random.Range(1, MaxAbilities);

            for (int J = 0; J < AbilitiesNum; ++J)
            {
                AbilityModifier Mod = new AbilityModifier();
                Mod.Type = (AbilityType)Random.Range(1, MaxAbilities);
                Mod.Modifier = Random.Range(0.75f, 1.25f);
                Quest.AbilityModifiers.Add(Mod);
            }

            for (int J = 0; J < StatsNum; ++J)
            {
                StatModifier Mod = new StatModifier();
                Mod.Type = (StatType)Random.Range(1, MaxStats);
                Mod.Modifier = Random.Range(0.75f, 1.25f);
                Quest.StatModifiers.Add(Mod);
            }

            Generated.Quests.Add(Quest);
        }
        return Generated;
    }

    public static Mission GrabRandomMissionFromDB()
    {
        return MissionsDatabase.instance.Missions[Random.Range(0, MissionsDatabase.instance.Missions.Count)];
    }

    public Quest GetAvailableQuest()
    {
        foreach (Quest Quest in Quests)
        {
            if (Quest.questState == QuestState.InProgress)
            {
                return null;
            }

            if (Quest.questState == QuestState.NotStarted)
            {
                return Quest;
            }
        }
        return null;
    }

    public bool IsMissionOver()
    {
        foreach (Quest quest in Quests)
        {
            if (quest.questState != QuestState.Success && quest.questState != QuestState.Failure)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsMissionSuccessful()
    {
        foreach (Quest quest in Quests)
        {
            if (quest.questState != QuestState.Success)
            {
                return false;
            }
        }
        return true;
    }
}
