using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    public float fadeDuration = 0.5f;
    public bool isOn = true;

    private CanvasGroup canvas;
    private bool isFading = false;
    private bool sendScheduledCallback = false;
    private string scheduledCallback = "";

    void Start()
    {
        canvas = GetComponent<CanvasGroup>();
        canvas.alpha = isOn ? 1.0f : 0.0f;
        canvas.interactable = isOn;
        canvas.blocksRaycasts = isOn;
    }

    void Update()
    {
        if (isFading)
        {
            float step = Time.deltaTime * (1.0f / fadeDuration);
            float newAlpha = isOn ? Mathf.Clamp01(canvas.alpha + step) : Mathf.Clamp01(canvas.alpha - step);
            canvas.alpha = newAlpha;
            if (isOn && canvas.alpha == 1.0f || !isOn && canvas.alpha == 0.0f)
            {
                isFading = false;
                if (scheduledCallback != "")
                {
                    sendScheduledCallback = true;
                }
            }
        }
        else if (sendScheduledCallback)
        {
            gameObject.SendMessageUpwards(scheduledCallback);
            scheduledCallback = "";
            sendScheduledCallback = false;
        }
    }

    public void FadeIn(string callbackMessage = "")
    {
        isFading = true;
        isOn = true;
        scheduledCallback = callbackMessage;
        canvas.blocksRaycasts = true;
        canvas.interactable = true;
    }

    public void FadeOut(string callbackMessage = "")
    {
        isFading = true;
        isOn = false;
        scheduledCallback = callbackMessage;
        canvas.blocksRaycasts = false;
        canvas.interactable = false;
    }
}
