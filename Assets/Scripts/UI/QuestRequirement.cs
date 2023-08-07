using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[ExecuteInEditMode]
public class QuestRequirement : MonoBehaviour
{
    public List<AbilityRequirementDecription> abilityRequirements;
    public List<StatRequirementDecription> statRequirements;

    [Header("Mapping")]
    public FontAwesome icon;
    public Slider slider;
    public Image sliderFill;

    [HideInInspector] public AbilityRequirementDecription abilityDescription;
    [HideInInspector] public StatRequirementDecription statDescription;

    private bool isAbility = false;

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void SelectRandomAbility()
    {
        isAbility = true;
        abilityDescription = abilityRequirements[Random.Range(0, abilityRequirements.Count)];
        UpdateConnectedFields();
    }

    public void SelectRandomStat()
    {
        isAbility = false;
        statDescription = statRequirements[Random.Range(0, statRequirements.Count)];
        UpdateConnectedFields();
        sliderFill.color = statDescription.color;
    }

    public void SelectAbility(AbilityType ability)
    {
        isAbility = true;
        abilityDescription = abilityRequirements.Find(Ab => Ab.abilityType == ability);
        UpdateConnectedFields();
    }

    public void SelectStat(StatType stat)
    {
        isAbility = false;
        statDescription = statRequirements.Find(St => St.statType == stat);
        UpdateConnectedFields();
        sliderFill.color = statDescription.color;
    }

    public void SetStatValue(float value)
    {
        slider.value = value;
    }

	void UpdateConnectedFields()
    {
        icon.ChangeIcon(isAbility ? abilityDescription.icon : statDescription.icon);
        icon.ChangeColor(isAbility ? abilityDescription.color : statDescription.color);
    }
}

[System.Serializable]
public class AbilityRequirementDecription
{
    public string name;
    public AbilityType abilityType;
    public string icon;
    public Color color;
}

[System.Serializable]
public class StatRequirementDecription
{
    public string name;
    public StatType statType;
    public string icon;
    public Color color;
}
