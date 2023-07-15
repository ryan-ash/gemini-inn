using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Counter : MonoBehaviour
{
    public Text counter;
    public Color colorOnIncrease;
    public Color colorOnDecrease;
    public float fadeDuration = 0.5f;
    public Vector3 scaleOnUpdate = new Vector3(1.5f, 1.5f, 1.5f);

    private int currentCount = 0;
    private float fadeTimeLeft = 0.0f;
    private Color initialColor;
    private Color targetColor;
    private Vector3 initialScale;

    void Start()
    {
        initialColor = counter.color;
        targetColor = initialColor;
        initialScale = counter.transform.localScale;
    }

    void Update()
    {
        if (fadeTimeLeft > 0.0f)
        {
            fadeTimeLeft -= Time.deltaTime;
            if (fadeTimeLeft <= 0.0f)
            {
                counter.color = initialColor;
            }
            else
            {
                float t = 1.0f - fadeTimeLeft / fadeDuration;
                counter.color = Color.Lerp(targetColor, initialColor, t);
                counter.transform.localScale = Vector3.Lerp(scaleOnUpdate, initialScale, t);
            }
        }
    }

    public void UpdateCounter(int newCount)
    {
        if (newCount == currentCount)
            return;

        bool isIncrease = newCount > currentCount;
        currentCount = newCount;
        counter.text = newCount.ToString();

        targetColor = isIncrease ? colorOnIncrease : colorOnDecrease;
        counter.color = targetColor;
        counter.transform.localScale = scaleOnUpdate;
        fadeTimeLeft = fadeDuration;
    }
}
