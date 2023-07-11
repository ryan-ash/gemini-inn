using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestEvent : MonoBehaviour
{
    public static List<QuestEvent> allQuestEvents = new List<QuestEvent>();

    [Header("Mapping")]
    public Text questNameText;
    public Text eventText;

    [HideInInspector] public Quest quest;

    private string generatedEventText = "";
    
    void Start()
    {
        allQuestEvents.Add(this);
    }

    public void SetQuest(Quest inQuest)
    {
        quest = inQuest;
        questNameText.text = quest.questName;

        switch(quest.questState)
        {
            case QuestState.OnRoad:
                generatedEventText = "GROUP embarked on a journey; let's hope they don't starve";
                break;
            case QuestState.InProgress:
                generatedEventText = "GROUP reached the quest location; let's hope they don't die";
                break;
            case QuestState.Success:
                generatedEventText = "GROUP has succeded in their quest; they return home victorious! dam...";
                break;
            case QuestState.Failure:
                generatedEventText = "GROUP has failed their quest; phew, that was close...";
                break;
        }

        eventText.text = generatedEventText;
    }

    public void ShowEvent()
    {

    }
}
