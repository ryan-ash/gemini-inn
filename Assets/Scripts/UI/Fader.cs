using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    public float fadeDuration = 0.5f;
    public bool switchOnStart = true;
    public bool isOn = true;
    public bool canEverBeInteractable = true;

    private CanvasGroup canvasRef = null;
    private CanvasGroup canvas {
        get {
            if (canvasRef == null)
                canvasRef = GetComponent<CanvasGroup>();
            return canvasRef;
        }
    }
    private bool isFading = false;
    private bool sendScheduledCallback = false;
    private bool sendScheduledCallbackToSelf = false;
    private string scheduledCallback = "";

    void Start()
    {
        if (switchOnStart)
            Switch(isOn);
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
            if (sendScheduledCallbackToSelf)
                gameObject.SendMessage(scheduledCallback);
            else
                gameObject.SendMessageUpwards(scheduledCallback);
            scheduledCallback = "";
            sendScheduledCallback = false;
        }
    }

    public void Switch(bool isOn)
    {
        this.isOn = isOn;
        canvas.alpha = isOn ? 1.0f : 0.0f;
        if (canEverBeInteractable)
        {
            canvas.blocksRaycasts = isOn;
            canvas.interactable = isOn;
        }
        else
        {
            canvas.blocksRaycasts = false;
            canvas.interactable = false;
        }
    }

    public void FadeIn(string callbackMessage = "", bool sendToSelf = false)
    {
        isFading = true;
        isOn = true;
        scheduledCallback = callbackMessage;
        sendScheduledCallbackToSelf = sendToSelf;
        if (canEverBeInteractable)
        {
            canvas.blocksRaycasts = true;
            canvas.interactable = true;
        }
    }

    public void FadeOut(string callbackMessage = "", bool sendToSelf = false)
    {
        isFading = true;
        isOn = false;
        scheduledCallback = callbackMessage;
        sendScheduledCallbackToSelf = sendToSelf;
        canvas.blocksRaycasts = false;
        canvas.interactable = false;
    }
}
