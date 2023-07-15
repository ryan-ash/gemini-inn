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
    public GameObject questInProgress;

    [Header("Settings")]
    public float fadeDuration = 0.5f;
    public Color timeoutColor = Color.white;
    public Color progressColor = Color.yellow;
    public Color successColor = Color.green;
    public Color failureColor = Color.red;

    private bool isInfoOpen = false;
    private bool isInfoAnimating = false;
    private BoxCollider openCollider;
    private BoxCollider closeCollider;
    private Animator animator;
    private bool isQuestTimeoutActive = false;
    private bool isQuestInProgress = false;
    private bool isQuestOver = false;
    private bool isQuestSuccess = false;
    private float questTimer = 0.0f;
    private float questTimeout = 0.0f;
    private float questDuration = 0.0f;
    private Quest quest;

    public void SetQuest(Quest InQuest)
    {
        quest = InQuest;
        questTimeout = quest.timeout;
        questDuration = quest.baseDuration;
        questTimer = 0.0f;
        isQuestTimeoutActive = questTimeout > 0.0f;
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

    public void SetInProgress()
    {
        animator.SetBool("QuestInProgress", true);
        questName.color = Color.yellow;
        questInProgress.SetActive(true);
        isQuestInProgress = true;
        questSliderFill.color = progressColor;
        questSlider.value = 0.0f;
        questTimer = 0.0f;
    }

    public void SetOver(bool isSuccess = false, bool isTimeout = false)
    {
        animator.SetBool("QuestOver", true);
        questName.color = Color.gray;
        questInProgress.SetActive(false);
        isQuestInProgress = false;
        isQuestOver = true;
        isQuestSuccess = isSuccess;
        questSliderFill.color = isSuccess ? successColor : failureColor;
        questSlider.value = 1.0f;

        GameMode.instance.UpdateQuestState(quest.mission, quest, isSuccess ? QuestState.Success : QuestState.Failure);
        // update audio logic later
        AudioRevolver.Fire(isTimeout ? AudioNames.MugOnTable : isSuccess ? AudioNames.QuestCompleted : AudioNames.QuestFailed);
    }

    void UpdateQuestSlider()
    {
        // update quest slider
        questSlider.value = 
            isQuestInProgress ? questTimer / questDuration : 
            isQuestOver ? 1.0f :
            isQuestTimeoutActive ? 1.0f - questTimer / questTimeout :
            0.0f;
    }

    void UpdateQuestState()
    {
        if (isQuestInProgress && questTimer >= questDuration)
        {
            // apply quest success rate logic & all
            SetOver(true);
        }
        else if (isQuestTimeoutActive && questTimer >= questTimeout)
        {
            SetOver(quest.successOnTimeout, true);
        }
    }

    void FillData()
    {
        questName.text = quest.questName;
        questDescription.text = quest.questDescription;
    }

    void Start()
    {
        openCollider = infoCanvas.gameObject.GetComponent<BoxCollider>();
        animator = GetComponent<Animator>();
        infoCanvas.alpha = 0.0f;
    }

    void Update()
    {
        // move timer logic to game mode
        if (GameMode.IsTimersRunning() && !isQuestOver)
        {
            questTimer += Time.deltaTime;
            UpdateQuestSlider();
            UpdateQuestState();
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
