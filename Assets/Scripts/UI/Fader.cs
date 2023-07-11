using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    public static Fader instance;

    public float fadeDuration = 0.5f;
    public bool isOn = true;

    private CanvasGroup canvas;
    private bool isFading = false;
    private string scheduledCallback = "";

    void Start()
    {
        instance = this;

        canvas = GetComponent<CanvasGroup>();
        canvas.alpha = isOn ? 1.0f : 0.0f;
        if (isOn)
            FadeOut();
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
                    gameObject.SendMessageUpwards(scheduledCallback);
                    scheduledCallback = "";
                }
            }
        }
    }

    public void FadeIn(string callbackMessage = "")
    {
        isFading = true;
        isOn = true;
        scheduledCallback = callbackMessage;
    }

    public void FadeOut(string callbackMessage = "")
    {
        isFading = true;
        isOn = false;
        scheduledCallback = callbackMessage;
    }
}
