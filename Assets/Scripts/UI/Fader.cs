using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    public Fader instance;

    public float fadeDuration = 0.5f;
    public bool isOn = true;

    private CanvasGroup canvas;
    private bool isFading = false;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        canvas = GetComponent<CanvasGroup>();
        canvas.alpha = isOn ? 1.0f : 0.0f;
        if (isOn)
            FadeOut();
    }

    // Update is called once per frame
    void Update()
    {
        if (isFading)
        {
            float step = Time.deltaTime * (1.0f / fadeDuration);
            float newAlpha = isOn ? Mathf.Clamp01(canvas.alpha + step) : Mathf.Clamp01(canvas.alpha - step);
            canvas.alpha = newAlpha;
            if (isOn && canvas.alpha == 1.0f || !isOn && canvas.alpha == 0.0f)
                isFading = false;
        }        
    }

    public void FadeIn()
    {
        isFading = true;
        isOn = true;
    }

    public void FadeOut()
    {
        isFading = true;
        isOn = false;
    }
}
