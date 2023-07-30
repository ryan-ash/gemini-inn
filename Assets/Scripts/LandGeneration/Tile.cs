using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public BiomePreset selectedBiome;
    public float waitBeforeRotating = 0.1f;
    public float rotationSpeed = 10.0f;
    public float fadeSpeed = 5.0f;
    public int X;
    public int Y;
    public int XFromEnd;
    public int XFromCenter;

    public bool hideThroughRotation = true;
    public bool hideThroughColor = true;
    public float hideYAngle = 90.0f;
    public Color hideColor = Color.clear;
    public Color darkColor = Color.red;
    public Color lightColor = Color.blue;

    private bool isRotating = false;
    private bool isFading = false;
    private bool isOn = false;
    private int startOffset = 0;

    private Color targetColor = Color.white;
    private Color modifiedColor = Color.white;

    [HideInInspector] public int currentLightLevel = 0;

    private Quaternion offRotation, targetRotation;


    void Start()
    {

    }

    void Update()
    {
        if (!isRotating && !isFading)
            return;

        if (isRotating && hideThroughRotation)
        {
            float step = Time.deltaTime * rotationSpeed;
            var targetRotation = isOn ? Quaternion.identity : offRotation;

            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, step);
            float angle = Quaternion.Angle(targetRotation, transform.localRotation);

            if (angle < 1.0f)
            {
                transform.localRotation = targetRotation;
                isRotating = false;
            }
        }

        if (isFading)
        {
            float step = Time.deltaTime * fadeSpeed;
            Color newColor = Color.Lerp(spriteRenderer.color, targetColor, step);
            spriteRenderer.color = newColor;
            if (spriteRenderer.color == targetColor)
            {
                isFading = false;
            }
        }
    }

    public void Prepare()
    {
        if (!hideThroughRotation)
            return;

        Vector3 targetDirection = Camera.main.transform.position - transform.position;
        targetDirection.Normalize();

        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, Mathf.PI, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
        transform.Rotate(Vector3.up, hideYAngle, Space.Self);
        offRotation = transform.localRotation;

        currentLightLevel = Map.instance.initialLightLevel;
        UpdateSpriteColor(CalculateLightLevelColor());
        SetColor(modifiedColor);
        if (hideThroughRotation || hideThroughColor)
            Hide();
    }

    public void Show()
    {
        isOn = true;
        startOffset = XFromCenter;
        StartCoroutine(StartRotation());
        if (hideThroughColor)
            SetColor(modifiedColor);
    }

    public void Hide()
    {
        isOn = false;
        startOffset = Mathf.Min(X, XFromEnd);
        StartCoroutine(StartRotation());
        if (hideThroughColor)
            SetColor(hideColor);
    }

    public void SetColor(Color color)
    {
        targetColor = color;
        isFading = true;
    }

    public void AddDarkShade()
    {
        if (currentLightLevel > 0)
        {
            Map.instance.UpdateAverageForTileShift(false);
            currentLightLevel -= Map.instance.lightLevelStep;
        }
        UpdateSpriteColor(CalculateLightLevelColor());
    }

    public void AddLightShade()
    {
        if (currentLightLevel < Map.instance.lightLevels)
        {
            Map.instance.UpdateAverageForTileShift(true);
            currentLightLevel += Map.instance.lightLevelStep;
        }
        UpdateSpriteColor(CalculateLightLevelColor());
    }

    public void SetShadeByAlpha(float alpha)
    {
        int newLightLevel = Mathf.RoundToInt(alpha * Map.instance.lightLevels);
        currentLightLevel = newLightLevel;
        UpdateSpriteColor(CalculateLightLevelColor());
    }

    private void UpdateSpriteColor(Color color)
    {
        if (isOn)
            SetColor(color);
        modifiedColor = color;
    }

    public void ResetShade()
    {
        currentLightLevel = Map.instance.initialLightLevel;
        UpdateSpriteColor(CalculateLightLevelColor());
    }

    private Color CalculateLightLevelColor()
    {
        return Color.Lerp(darkColor, lightColor, (float)currentLightLevel / (float)Map.instance.lightLevels);
    }

    IEnumerator StartRotation()
    {
        yield return new WaitForSeconds(waitBeforeRotating * startOffset);
        if (hideThroughRotation)
            isRotating = true;
    }
}
