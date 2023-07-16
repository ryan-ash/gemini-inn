using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObject : MonoBehaviour
{
    public float fadeSpeed = 5.0f;
    public bool setValueOnStart = false;
    public bool setOn = false;
    public Color hideColor = Color.clear;

    private SpriteRenderer spriteRenderer;
    private Color targetColor = Color.white;
    private Color initialColor = Color.white;
    private bool isFading = false;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (setValueOnStart)
        {
            if (setOn)
                FadeIn(true);
            else
                FadeOut(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFading)
            return;

        float step = Time.deltaTime * fadeSpeed;
        Color newColor = Color.Lerp(spriteRenderer.color, targetColor, step);
        spriteRenderer.color = newColor;
        if (spriteRenderer.color == targetColor)
        {
            isFading = false;
        }
    }

    public void FadeIn(bool instant = false)
    {
        targetColor = initialColor;
        isFading = true;
        if (instant)
        {
            spriteRenderer.color = targetColor;
            isFading = false;
        }
    }

    public void FadeOut(bool instant = false)
    {
        targetColor = hideColor;
        isFading = true;
        if (instant)
        {
            spriteRenderer.color = targetColor;
            isFading = false;
        }
    }

    public static void HideAll()
    {
        foreach (MapObject mapObject in GameMode.instance.generatedMapObjects)
        {
            mapObject.FadeOut();
        }
    }

    public static void ShowAll()
    {
        foreach (MapObject mapObject in GameMode.instance.generatedMapObjects)
        {
            mapObject.FadeIn();
        }
    }
}
