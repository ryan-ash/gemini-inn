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

    [Header("Settings")]
    public float fadeDuration = 0.5f;
    [HideInInspector] public Quest quest;

    private bool isInfoOpen = false;
    private bool isInfoAnimating = false;
    private BoxCollider openCollider;
    private BoxCollider closeCollider;

    public void ShowInfo()
    {
        FillData();
        isInfoOpen = true;
        isInfoAnimating = true;
        openCollider.gameObject.SetActive(true); // why doesn't this work?..
        CursorSetter.SetHoverCursor();
    }

    public void HideInfo()
    {
        isInfoOpen = false;
        isInfoAnimating = true;
        CursorSetter.SetDefaultCursor();
    }

    void FillData()
    {
        questName.text = quest.questName;
        questDescription.text = quest.questDescription;
    }

    void Start()
    {
        openCollider = infoCanvas.gameObject.GetComponent<BoxCollider>();
        infoCanvas.alpha = 0.0f;
    }

    void Update()
    {
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
