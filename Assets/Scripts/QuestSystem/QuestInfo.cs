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
    public Transform abilitiesHolder;
    public Transform statsHolder;

    [Header("Settings")]
    public float fadeDuration = 0.5f;
    public float moveDuration = 0.5f;
    public float moveBackDuration = 0.5f;
    public float waitBeforeHidingAfterMoveBack = 1.0f;
    public LeanTweenType moveEaseType = LeanTweenType.easeInOutSine;
    public LeanTweenType moveBackEaseType = LeanTweenType.easeInOutSine;
    public Color timeoutColor = Color.white;
    public Color roadColor = Color.cyan;
    public Color progressColor = Color.yellow;
    public Color successColor = Color.green;
    public Color failureColor = Color.red;
    public GameObject questRequirementPrefab;
    public GameObject statRequirementPrefab;

    private bool isInfoOpen = false;
    private bool isInfoAnimating = false;
    private bool isInfoPinned = false;
    private bool isQuestTimeoutActive = false;
    private bool isQuestOnRoad = false;
    private bool isQuestInProgress = false;
    private bool isQuestOver = false;
    private bool isQuestSuccess = false;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    private Animator animator;
    private BoxCollider openCollider;
    private BoxCollider closeCollider;
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

        foreach (AbilityModifier abilityModifier in quest.AbilityModifiers)
        {
            GameObject ability = Instantiate(questRequirementPrefab, abilitiesHolder);
            QuestRequirement questRequirement = ability.GetComponent<QuestRequirement>();
            questRequirement.SelectAbility(abilityModifier.Type);
        }

        foreach (StatModifier statModifier in quest.StatModifiers)
        {
            GameObject stat = Instantiate(statRequirementPrefab, statsHolder);
            QuestRequirement questRequirement = stat.GetComponent<QuestRequirement>();
            questRequirement.SelectStat(statModifier.Type);
        }

        FillData();
    }

    public Quest GetQuest()
    {
        return quest;
    }

    public void ShowInfo()
    {
        HideAll(this);
        isInfoOpen = true;
        isInfoAnimating = true;
        openCollider.gameObject.SetActive(true); // why doesn't this work?..
        CursorSetter.SetHoverSpecialCursor();
    }

    public void HideInfo()
    {
        if (isInfoPinned)
            return;

        isInfoOpen = false;
        isInfoAnimating = true;
        CursorSetter.SetDefaultCursor();
    }

    public void Pin()
    {
        isInfoPinned = true;
        LeanTween.value(gameObject, 0.0f, 1.0f, moveDuration).setEase(moveEaseType).setOnUpdate((float value) => {
            infoCanvas.transform.position = Vector3.Lerp(initialPosition, Inn.instance.questInfoTransform.position, value);
            infoCanvas.transform.rotation = Quaternion.Lerp(initialRotation, Inn.instance.questInfoTransform.rotation, value);
            infoCanvas.transform.localScale = Vector3.Lerp(initialScale, Inn.instance.questInfoTransform.localScale, value);
        });
    }

    public void Unpin()
    {
        LeanTween.value(gameObject, 1.0f, 0.0f, moveBackDuration).setEase(moveBackEaseType).setOnUpdate((float value) => {
            infoCanvas.transform.position = Vector3.Lerp(initialPosition, Inn.instance.questInfoTransform.position, value);
            infoCanvas.transform.rotation = Quaternion.Lerp(initialRotation, Inn.instance.questInfoTransform.rotation, value);
            infoCanvas.transform.localScale = Vector3.Lerp(initialScale, Inn.instance.questInfoTransform.localScale, value);
        }).setOnComplete(() => {
            isInfoPinned = false;
            Wait.Run(waitBeforeHidingAfterMoveBack, () => {
                HideInfo();
            });
        });
    }

    public bool IsPinned()
    {
        return isInfoPinned;
    }

    public static void HideAll(QuestInfo except = null)
    {
        foreach (QuestInfo questInfo in GameMode.instance.generatedQuestInfos)
        {
            if (questInfo != except)
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

        initialPosition = infoCanvas.transform.position;
        initialRotation = infoCanvas.transform.rotation;
        initialScale = infoCanvas.transform.localScale;
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
