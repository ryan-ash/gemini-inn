using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestInfo : MonoBehaviour
{
    public CanvasGroup infoCanvas;
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
    }

    public void HideInfo()
    {
        isInfoOpen = false;
        isInfoAnimating = true;
        openCollider.gameObject.SetActive(false);
    }

    void FillData()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        openCollider = infoCanvas.gameObject.GetComponent<BoxCollider>();
    }

    // Update is called once per frame
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
        }
    }
}
