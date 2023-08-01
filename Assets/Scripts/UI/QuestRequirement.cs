using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[ExecuteInEditMode]
public class QuestRequirement : MonoBehaviour
{
    public List<AbilityRequirementDecription> abilityRequirements;
    public List<StatRequirementDecription> statRequirements;

    [HideInInspector] public AbilityRequirementDecription abilityDescription;
    [HideInInspector] public StatRequirementDecription statDescription;

    private bool isAbility = false;
    private FontAwesome fontAwesome;
    private FontAwesome icon {
        get {
            if (fontAwesome == null) {
                fontAwesome = GetComponent<FontAwesome>();
            }
            return fontAwesome;
        }
    }

    void Start()
    {
        // SelectRandomAbility(); // for now
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
    }

	void UpdateConnectedFields()
    {
        icon.ChangeIcon(abilityDescription.icon);
        icon.ChangeColor(abilityDescription.color);
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
