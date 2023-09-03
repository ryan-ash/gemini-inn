using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random=UnityEngine.Random;

[System.Serializable]
public class Mission
{
    public string MissionName;
    public bool ExcludeFromSpawn = false;
    public bool Solo = false;
    public List<Quest> Quests;
    public Reward CompletionReward;
    public MissionRecurrenceType RecurrenceType = MissionRecurrenceType.Simultaneous;
    public PostMissionSpawn MissionsToSpawnOnSuccess;
    public PostMissionSpawn MissionsToSpawnOnFailure;

    [HideInInspector] public int ID = 0;

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
            Quest quest = new Quest();
            quest.questName = "Quest " + Random.Range(0, 1000).ToString();
            quest.questDescription = textMockup;
            quest.Biomes.Add("Jungle");
            quest.Biomes.Add("Forest");
            quest.Biomes.Add("Lake");
            quest.BaseSuccessRate = Random.Range(0.5f, 1.0f);

            int MaxAbilities = Enum.GetValues(typeof(AbilityType)).Length;
            int MaxStats = Enum.GetValues(typeof(StatType)).Length;

            int AbilitiesNum = Random.Range(1, MaxAbilities);
            int StatsNum = Random.Range(1, MaxAbilities);

            for (int J = 0; J < AbilitiesNum; ++J)
            {
                AbilityModifier Mod = new AbilityModifier();
                Mod.Type = (AbilityType)Random.Range(1, MaxAbilities);
                Mod.ModifierPerLevel = Random.Range(0.1f, 0.2f);
                Mod.MaxLevel = Random.Range(1, 3);
                quest.AbilityModifiers.Add(Mod);
            }

            for (int J = 0; J < StatsNum; ++J)
            {
                StatModifier Mod = new StatModifier();
                Mod.Type = (StatType)Random.Range(1, MaxStats);
                Mod.MaxSuccessModifier = Random.Range(0.1f, 0.2f);
                Mod.MaxFailureModifier = Random.Range(-0.1f, -0.2f);
                quest.StatModifiers.Add(Mod);
            }

            Generated.Quests.Add(quest);
        }
        return Generated;
    }

    public static Mission GrabRandomMissionFromDB()
    {
        Mission missionToFind = null;
        while (missionToFind == null)
        {
            List<Mission> soloMissions = MissionsDatabase.instance.Missions.FindAll(mission => mission.Solo);
            List<Mission> missionsSubset = soloMissions.Count > 0 ? soloMissions : MissionsDatabase.instance.Missions;
            missionToFind = missionsSubset[Random.Range(0, missionsSubset.Count)];
            if (missionToFind.ExcludeFromSpawn)
            {
                missionToFind = null;
            }
        }
        return missionToFind;
    }

    public static Mission GrabMissionByName(string name)
    {
        Mission missionToFind = MissionsDatabase.instance.Missions.Find(mission => mission.MissionName == name);
        return missionToFind;
    }

    public Quest GetAvailableQuest()
    {
        foreach (Quest quest in Quests)
        {
            if (quest.questState == QuestState.InProgress || quest.questState == QuestState.OnRoad)
            {
                return null;
            }

            if (quest.questState == QuestState.NotStarted)
            {
                return quest;
            }
        }
        return null;
    }

    public Quest GetCurrentQuest()
    {
        foreach (Quest quest in Quests)
        {
            if (quest.questState == QuestState.InProgress || quest.questState == QuestState.OnRoad || quest.questState == QuestState.NotStarted)
            {
                return quest;
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

[System.Serializable]
public class PostMissionSpawn
{
    public List<string> MissionsToSpawn;
    public bool MutuallyExclusive = true;
}

[System.Serializable]
public enum MissionRecurrenceType
{
    Simultaneous,
    Repeating, // Only one mission of this type can be active at a time
    Unique // after it's done or failed it will never be available again
}
