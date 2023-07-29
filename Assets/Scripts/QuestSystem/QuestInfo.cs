using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestInfo : MonoBehaviour
{
    [Header("Mapping")]
    public CanvasGroup infoCanvas;
    public Text questName;
    public Text questDescription;
    public Slider questSlider;
    public Image questSliderFill;
    public SpriteRenderer questMapCircle;

    [Header("Settings")]
    public float fadeDuration = 0.5f;
    public Color timeoutColor = Color.white;
    public Color roadColor = Color.cyan;
    public Color progressColor = Color.yellow;
    public Color successColor = Color.green;
    public Color failureColor = Color.red;

    private bool isInfoOpen = false;
    private bool isInfoAnimating = false;
    private BoxCollider openCollider;
    private BoxCollider closeCollider;
    private Animator animator;
    private bool isQuestTimeoutActive = false;
    private bool isQuestOnRoad = false;
    private bool isQuestInProgress = false;
    private bool isQuestOver = false;
    private bool isQuestSuccess = false;
    private Quest quest;

    public void SetQuest(Quest InQuest)
    {
        quest = InQuest;
        isQuestTimeoutActive = quest.timeout > 0.0f;
        if (isQuestTimeoutActive)
        {
            questSliderFill.color = timeoutColor;
            questSlider.value = 1.0f;
        }
        else
        {
            questSliderFill.color = progressColor;
            questSlider.value = 0.0f;
        }
    }

    public Quest GetQuest()
    {
        return quest;
    }

    public void ShowInfo()
    {
        HideAll();
        FillData();
        isInfoOpen = true;
        isInfoAnimating = true;
        openCollider.gameObject.SetActive(true); // why doesn't this work?..
        CursorSetter.SetHoverSpecialCursor();
    }

    public void HideInfo()
    {
        isInfoOpen = false;
        isInfoAnimating = true;
        CursorSetter.SetDefaultCursor();
    }

    public static void HideAll()
    {
        foreach (QuestInfo questInfo in GameMode.instance.generatedQuestInfos)
        {
            questInfo.HideInfo();
        }
    }

    public void SetOnRoad()
    {
        isQuestOnRoad = true;

        questName.color = roadColor;
        questMapCircle.gameObject.SetActive(true);
        questSliderFill.color = roadColor;
        questSlider.value = 0.0f;
    }

    public void SetInProgress()
    {
        isQuestOnRoad = false;
        isQuestInProgress = true;

        animator.SetBool("QuestInProgress", true);
        questName.color = progressColor;
        questSliderFill.color = progressColor;
        questMapCircle.color = progressColor;
        questSlider.value = 0.0f;
    }

    public void SetOver(bool isSuccess = false, bool isTimeout = false)
    {
        isQuestInProgress = false;
        isQuestOver = true;
        isQuestSuccess = isSuccess;

        animator.SetBool("QuestSuccess", isSuccess);
        animator.SetInteger("Variation", Random.Range(0, 3));
        animator.SetBool("QuestOver", true);
        questMapCircle.gameObject.SetActive(false);
        questSliderFill.color = isSuccess ? successColor : failureColor;
        questName.color = questSliderFill.color;
        questSlider.value = 1.0f;
        // AudioRevolver.Fire(isTimeout ? AudioNames.MugOnTable : isSuccess ? AudioNames.QuestCompleted : AudioNames.QuestFailed);
        AudioRevolver.Fire(isSuccess ? AudioNames.QuestCompleted : AudioNames.QuestFailed);
    }

    void UpdateQuestSlider()
    {
        // update quest slider
        questSlider.value = 
            isQuestOnRoad ? quest.questTimer / quest.roadDuration :
            isQuestInProgress ? quest.questTimer / quest.baseDuration : 
            isQuestOver ? 1.0f :
            isQuestTimeoutActive ? 1.0f - quest.questTimer / quest.timeout :
            0.0f;
    }

    void FillData()
    {
        questName.text = quest.questName;
        questDescription.text = quest.questDescription;
    }

    void Start()
    {
        openCollider = infoCanvas.gameObject.GetComponent<BoxCollider>();
        animator = GetComponentInChildren<Animator>();
        infoCanvas.alpha = 0.0f;
    }

    void Update()
    {
        if (GameMode.IsTimersRunning() && !isQuestOver)
        {
            UpdateQuestSlider();
        }

        if (!isInfoAnimating)
            return;

        float step = Time.deltaTime * (1.0f / fadeDuration);
        float newAlpha = isInfoOpen ? Mathf.Clamp01(infoCanvas.alpha + step) : Mathf.Clamp01(infoCanvas.alpha - step);
        infoCanvas.alpha = newAlpha;

        if (newAlpha <= 0.0f || newAlpha >= 1.0f)
        {
            infoCanvas.alpha = isInfoOpen ? 1.0f : 0.0f;
            isInfoAnimating = false;

            if (!isInfoOpen)
            {
                openCollider.gameObject.SetActive(false);
            }
        }
    }
}
