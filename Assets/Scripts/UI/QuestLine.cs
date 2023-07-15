using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestLine : MonoBehaviour
{
    private Quest quest;

    [Header("Settings")]
    public string notStartedIcon = "fa-exclamation-triangle";
    public Color notStartedColor = Color.white;
    public string onRoadIcon = "fa-ellipsis-h";
    public Color onRoadColor = Color.white;
    public string inProgressIcon = "fa-ellipsis-h";
    public Color inProgressColor = Color.white;
    public string successIcon = "fa-check";
    public Color successColor = Color.green;
    public string failIcon = "fa-times";
    public Color failColor = Color.red;
    public float colorLerpSpeed = 5.0f;

    [Header("Mapping")]
    public Text questTitle;
    public FontAwesome questIcon;
    public Slider questProgress;
    public Image questProgressFill;
    private Color targetColor;

    void Start()
    {
        
    }

    void Update()
    {
        if (quest == null)
        {
            return;
        }

        RefreshQuest();

        switch (quest.questState)
        {
            case QuestState.OnRoad:
            case QuestState.InProgress:
                questProgress.value = quest.questTimer / quest.baseDuration;
                break;
            case QuestState.Success:
            case QuestState.Failure:
                questProgress.value = 1.0f;
                break;
            case QuestState.NotStarted:
                questProgress.value = 1.0f - quest.questTimer / quest.timeout;
                break;
        }

        if (quest.questState == QuestState.NotStarted && quest.timeout == 0.0f)
        {
            questProgress.value = 0.0f;
        }

        questProgressFill.color = Color.Lerp(questProgressFill.color, targetColor, Time.deltaTime * colorLerpSpeed);
        questTitle.color = Color.Lerp(questTitle.color, targetColor, Time.deltaTime * colorLerpSpeed);
    }

    public void SetQuest(Quest quest)
    {
        this.quest = quest;
        questTitle.text = quest.questName;
        RefreshQuest();
    }

    public void RefreshQuest()
    {
        string icon = onRoadIcon;
        switch (quest.questState)
        {
            case QuestState.NotStarted:
                icon = notStartedIcon;
                targetColor = notStartedColor;
                break;
            case QuestState.OnRoad:
                icon = onRoadIcon;
                targetColor = onRoadColor;
                break;
            case QuestState.InProgress:
                icon = inProgressIcon;
                targetColor = inProgressColor;
                break;
            case QuestState.Success:
                icon = successIcon;
                targetColor = successColor;
                break;
            case QuestState.Failure:
                icon = failIcon;
                targetColor = failColor;
                break;
        }
        questIcon.ChangeIcon(icon);
    }
}
