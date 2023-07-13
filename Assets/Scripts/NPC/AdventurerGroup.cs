using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GroupState
{
    Empty,
    Idle,
    Negotiating,
    OnRoadToQuest,
    Successful,
    Dead,
    Mad
}

public class AdventurerGroup : MonoBehaviour
{
    [Header("Mapping")]
    public GameObject adventurerTableLights;

    [Header("Settings")]
    public float fadeDuration = 0.5f;

    [HideInInspector] public GroupState groupState = GroupState.Empty;
    [HideInInspector] public List<Adventurer> adventurers;
    [HideInInspector] public Quest quest;

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
        adventurers.Add(adventurer);
        adventurer.group = this;
        if (groupState == GroupState.Empty)
        {
            groupState = GroupState.Idle;
            OnGroupStateUpdated();
        }
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

    private void OnGroupStateUpdated()
    {
        RecalculateGroupStats();
        if (groupState == GroupState.Idle && adventurers.Count == 1)
        {
            LightUpAdventurerTable();
        }

        if (groupState == GroupState.OnRoadToQuest)
        {
            quest.questState = QuestState.OnRoad;
            GameMode.instance.OnQuestUpdated(quest);
        }
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
        groupState = GroupState.OnRoadToQuest;
        var quest = GameMode.instance.selectedQuest;
        OnGroupStateUpdated();
    }
}
