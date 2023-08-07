using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdventurerLine : MonoBehaviour
{
    [HideInInspector] public Adventurer adventurer;

    [Header("Settings")]
    public string maleIcon = "fa-male";
    public string femaleIcon = "fa-female";

    [Header("Mapping")]
    public TextWriter nameWriter;
    public FontAwesome statusIcon;
    public Transform abilitiesContainer;
    public Transform statsContainer;
    public GameObject abilitiesSeparator;

    [Header("Prefabs")]
    public GameObject adventurerAbility;
    public GameObject adventurerStat;

    void Start()
    {
        
    }

    void Update()
    {

    }

    public void SetAdventurer(Adventurer adventurer)
    {
        this.adventurer = adventurer;
        nameWriter.Write(adventurer.adventurerName);
        statusIcon.ChangeIcon(adventurer.femaleGender ? femaleIcon : maleIcon);

        foreach (Ability ability in adventurer.Abilities)
        {
            GameObject abilityUI = Instantiate(adventurerAbility, abilitiesContainer);
            QuestRequirement questRequirement = abilityUI.GetComponent<QuestRequirement>();
            questRequirement.SelectAbility(ability.Type);
        }
        if (adventurer.Abilities.Count == 0)
            abilitiesSeparator.SetActive(false);

        foreach (Stat stat in adventurer.Stats)
        {
            GameObject statUI = Instantiate(adventurerStat, statsContainer);
            QuestRequirement questRequirement = statUI.GetComponent<QuestRequirement>();
            questRequirement.SelectStat(stat.Type);
            questRequirement.SetStatValue(stat.Value);
        }
    }
}
