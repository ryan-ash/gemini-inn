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

    private bool isAnimatingLight = false;
    private bool isLightUp = false;

    private Dictionary<Light, float> defaultIntensities = new Dictionary<Light, float>();

    // Start is called before the first frame update
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

    // Update is called once per frame
    void Update()
    {
        if (isAnimatingLight)
        {
            Light[] lights = adventurerTableLights.GetComponentsInChildren<Light>();
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                float step = Time.deltaTime * (1.0f / fadeDuration);
                float newIntensity = isLightUp ? Mathf.Clamp01(light.intensity + step) : Mathf.Clamp01(light.intensity - step);
                light.intensity = newIntensity;
                if (newIntensity <= 0.0f || newIntensity >= defaultIntensities[light])
                {
                    if (AllLightsFinishedAnimating())
                    {
                        isAnimatingLight = false;
                    }
                    light.intensity = isLightUp ? defaultIntensities[light] : 0.0f;
                }
            }
        }
    }

    public void AddAdventurer(Adventurer adventurer)
    {
        adventurers.Add(adventurer);
        if (groupState == GroupState.Empty)
        {
            groupState = GroupState.Idle;
            OnGroupStateUpdated();
        }
    }

    private bool AllLightsFinishedAnimating()
    {
        Light[] lights = adventurerTableLights.GetComponentsInChildren<Light>();
        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            if (isLightUp && light.intensity < defaultIntensities[light])
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
}
