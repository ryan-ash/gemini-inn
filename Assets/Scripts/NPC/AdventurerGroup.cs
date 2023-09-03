using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GroupState
{
    Idle,
    Negotiating,
    OnRoadToQuest,
    Successful,
    Dead,
    Mad
}

public enum NameAbbreviationStyle
{
    FirstLetter,
    FirstTwoLetters,
    FirstThreeLetters,
    RandomLetters
}

public class QuestGroup
{
    [HideInInspector] public GroupState groupState = GroupState.Idle;
    [HideInInspector] public List<Adventurer> adventurers = new List<Adventurer>();
    [HideInInspector] public Quest quest;
    [HideInInspector] public GroupStats groupStats;

    public string icon;
}

public class GroupStats
{
    public string name;
    public List<Ability> abilities;
    public List<Stat> stats;
}

public class AdventurerGroup : MonoBehaviour
{
    [Header("Mapping")]
    public GameObject adventurerTableLights;

    [Header("Settings")]
    public float fadeDuration = 0.5f;
    public List<string> possibleGroupIcons;

    public string icon;
    public NameAbbreviationStyle nameAbbreviationStyle = NameAbbreviationStyle.FirstLetter;

    [HideInInspector] public List<Adventurer> adventurers = new List<Adventurer>();
    [HideInInspector] public GroupStats groupStats;

    private bool isAnimatingLight = false;
    private bool isLightUp = false;
    private bool isFocus = false;

    private Dictionary<Light, float> defaultIntensities = new Dictionary<Light, float>();

    void Start()
    {
        Light[] lights = adventurerTableLights.GetComponentsInChildren<Light>();
        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            defaultIntensities.Add(light, light.intensity);
            light.intensity = 0.0f;
        }

        Reinitialize();
    }

    void Update()
    {
        if (isAnimatingLight)
        {
            Light[] lights = adventurerTableLights.GetComponentsInChildren<Light>();
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                float step = Time.deltaTime * (1.0f / fadeDuration);
                float defaultIntensity = defaultIntensities[light];
                float targetIntensity = 
                    isFocus && isLightUp ? defaultIntensity * 2.0f : 
                    isFocus ? defaultIntensity * 0.5f : 
                    isLightUp ? defaultIntensity : 0.0f;
                light.intensity = targetIntensity;
                isAnimatingLight = false;
                // float newIntensity = isLightUp ? Mathf.Clamp01(light.intensity + step) : Mathf.Clamp01(light.intensity - step);
                // light.intensity = newIntensity;
                // if (newIntensity <= 0.0f || newIntensity >= targetIntensity)
                // {
                //     // if (AllLightsFinishedAnimating(targetIntensity))
                //     // {
                //         isAnimatingLight = false;
                //     // }
                //     light.intensity = isLightUp ? targetIntensity : 0.0f;
                // }
            }
        }
    }

    public void AddAdventurer(Adventurer adventurer)
    {
        if (adventurers.Count == 0)
        {
            LightUpAdventurerTable();
        }
        adventurers.Add(adventurer);
        RecalculateGroupStats();
    }

    private bool AllLightsFinishedAnimating(float targetIntensity)
    {
        Light[] lights = adventurerTableLights.GetComponentsInChildren<Light>();
        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            if (isLightUp && light.intensity < targetIntensity)
                return false;
            if (!isLightUp && light.intensity > 0.0f)
                return false;
        }
        return true;
    }

    public string PickRandomIcon()
    {
        int index = Random.Range(0, possibleGroupIcons.Count);
        string newIcon = possibleGroupIcons[index];
        icon = newIcon;
        // update icon in widgets if visible
        return icon;
    }

    public void PickRandomNameAbbreviationStyle()
    {
        int index = Random.Range(0, 4);
        nameAbbreviationStyle = (NameAbbreviationStyle)index;
    }

    public void Reinitialize()
    {
        adventurers.Clear();
        groupStats = new GroupStats();
        PickRandomIcon();
        PickRandomNameAbbreviationStyle();
    }

    private void RecalculateGroupStats()
    {
        groupStats.abilities = new List<Ability>();
        groupStats.stats = new List<Stat>();
        RecalculateName();

        Dictionary<StatType, int> aggregatedStats = new Dictionary<StatType, int>();
        for (int i = 0; i < adventurers.Count; i++)
        {
            var adventurer = adventurers[i];
            for (int j = 0; j < adventurer.abilities.Count; j++)
            {
                var ability = adventurer.abilities[j];
                Ability existingAbility = groupStats.abilities.Find(ab => ab.Type == ability.Type);
                if (existingAbility == null)
                {
                    existingAbility = new Ability();
                    existingAbility.Type = ability.Type;
                    existingAbility.Level = ability.Level;
                    groupStats.abilities.Add(existingAbility);
                }
                else
                {
                    existingAbility.Level += ability.Level;
                }
            }

            for (int j = 0; j < adventurer.stats.Count; j++)
            {
                var stat = adventurer.stats[j];
                if (!aggregatedStats.ContainsKey(stat.Type))
                {
                    aggregatedStats.Add(stat.Type, stat.Value);
                }
                else
                {
                    aggregatedStats[stat.Type] += stat.Value;
                }
            }
        }
        foreach (var stat in aggregatedStats)
        {
            Stat newStat = new Stat();
            newStat.Type = stat.Key;
            newStat.Value = stat.Value / adventurers.Count;
            groupStats.stats.Add(newStat);
        }
    }

    private void RecalculateName()
    {
        string newName = "";
        for (int i = 0; i < adventurers.Count; i++)
        {
            var adventurer = adventurers[i];
            if (nameAbbreviationStyle == NameAbbreviationStyle.FirstLetter)
            {
                newName += adventurer.adventurerName[0];
            }
            else if (nameAbbreviationStyle == NameAbbreviationStyle.FirstTwoLetters)
            {
                newName += adventurer.adventurerName[0];
                newName += adventurer.adventurerName[1];
            }
            else if (nameAbbreviationStyle == NameAbbreviationStyle.FirstThreeLetters)
            {
                newName += adventurer.adventurerName[0];
                newName += adventurer.adventurerName[1];
                newName += adventurer.adventurerName[2];
            }
            else if (nameAbbreviationStyle == NameAbbreviationStyle.RandomLetters)
            {
                int index = Random.Range(0, adventurer.adventurerName.Length);
                newName += adventurer.adventurerName[index];
            }
        }
        groupStats.name = newName;
    }

    public void LightUpAdventurerTable()
    {
        isAnimatingLight = true;
        isLightUp = true;
    }

    public void LightDownAdventurerTable()
    {
        isAnimatingLight = true;
        isLightUp = false;
    }

    public void FocusAdventurerTable()
    {
        isAnimatingLight = true;
        isFocus = true;
    }

    public void UnfocusAdventurerTable()
    {
        isAnimatingLight = true;
        isFocus = false;
    }

    public void AcceptQuest()
    {
        QuestGroup questGroup = new QuestGroup();
        questGroup.groupState = GroupState.OnRoadToQuest;
        questGroup.groupStats = groupStats;
        foreach (var adventurer in adventurers)
        {
            questGroup.adventurers.Add(adventurer);
        }
        questGroup.quest = GameMode.instance.selectedQuest;
        questGroup.quest.questGroup = questGroup;
        questGroup.icon = icon;
        GameMode.instance.RegisterQuestGroup(questGroup);
        GameMode.instance.UpdateQuestState(questGroup.quest.mission, questGroup.quest, QuestState.OnRoad);

        for (int i = 0; i < adventurers.Count; i++)
        {
            var adventurer = adventurers[i];
            adventurer.gameObject.SetActive(false);
            AdventurerManager.instance.ReleaseAdventurer(adventurer);
        }

        Reinitialize();
        UIGod.instance.ReleaseRemovedAdventurers();
    }
}
