using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroupLine : MonoBehaviour
{
    [HideInInspector] public Adventurer adventurer;

    [Header("Settings")]
    public string maleIcon = "fa-male";
    public string femaleIcon = "fa-female";

    [Header("Mapping")]
    public TextWriter nameWriter;
    public Text teamSize;
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

    public void SetGroup(GroupStats groupStats, List<Adventurer> adventurers)
    {
        nameWriter.Write(groupStats.name);
        for (int i = 0; i < abilitiesContainer.childCount; i++)
            Destroy(abilitiesContainer.GetChild(i).gameObject);
        for (int i = 0; i < statsContainer.childCount; i++)
            Destroy(statsContainer.GetChild(i).gameObject);

        teamSize.text = adventurers.Count.ToString();

        foreach (Ability ability in groupStats.abilities)
        {
            GameObject abilityUI = Instantiate(adventurerAbility, abilitiesContainer);
            QuestRequirement questRequirement = abilityUI.GetComponent<QuestRequirement>();
            questRequirement.SelectAbility(ability.Type);
        }
        if (groupStats.abilities.Count == 0)
            abilitiesSeparator.SetActive(false);

        foreach (Stat stat in groupStats.stats)
        {
            GameObject statUI = Instantiate(adventurerStat, statsContainer);
            QuestRequirement questRequirement = statUI.GetComponent<QuestRequirement>();
            questRequirement.SelectStat(stat.Type);
            questRequirement.SetStatValue(stat.Value);
        }
    }
}
