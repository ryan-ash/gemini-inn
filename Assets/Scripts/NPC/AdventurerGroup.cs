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

public class QuestGroup
{
    [HideInInspector] public GroupState groupState = GroupState.Idle;
    [HideInInspector] public List<Adventurer> adventurers;
    [HideInInspector] public Quest quest;

    public string icon;
}

public class AdventurerGroup : MonoBehaviour
{
    [Header("Mapping")]
    public GameObject adventurerTableLights;

    [Header("Settings")]
    public float fadeDuration = 0.5f;
    public List<string> possibleGroupIcons;

    public string icon;

    [HideInInspector] public List<Adventurer> adventurers;

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

        PickRandomIcon();
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

    private void RecalculateGroupStats()
    {
        // some code for recalculating group stats
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
        questGroup.adventurers = adventurers;
        questGroup.quest = GameMode.instance.selectedQuest;
        questGroup.quest.questGroup = questGroup;
        questGroup.quest.questState = QuestState.OnRoad;
        questGroup.icon = icon;
        GameMode.instance.RegisterQuestGroup(questGroup);

        for (int i = 0; i < adventurers.Count; i++)
        {
            var adventurer = adventurers[i];
            adventurer.gameObject.SetActive(false);
            AdventurerManager.instance.ReleaseAdventurer(adventurer);
        }

        adventurers.Clear();

        UIGod.instance.ReleaseRemovedAdventurers();
        RecalculateGroupStats();
        PickRandomIcon();
    }
}
