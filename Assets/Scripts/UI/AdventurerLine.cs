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
    }
}
