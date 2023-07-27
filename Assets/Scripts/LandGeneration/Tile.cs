using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public BiomePreset selectedBiome;
    public bool controlRotation = false;
    public float waitBeforeRotating = 0.1f;
    public float rotationSpeed = 10.0f;
    public float fadeSpeed = 5.0f;
    public int X;
    public int Y;
    public int XFromEnd;
    public int XFromCenter;

    public bool controlColor = true;
    public Color hideColor = Color.clear;

    private bool isRotating = false;
    private bool isFading = false;
    private bool isOn = false;
    private float initialY = 0.0f;
    private float initialZ = 0.0f;
    private int startOffset = 0;

    private Color targetColor = Color.white;
    private Color modifiedColor = Color.white;
    public Color darkColor = Color.red;
    public Color lightColor = Color.blue;

    [HideInInspector] public int currentLightLevel = 0;


    void Start()
    {
        currentLightLevel = Map.instance.initialLightLevel;
        UpdateSpriteColor(CalculateLightLevelColor());
        SetColor(modifiedColor);
        if (controlRotation || controlColor)
            Hide();
    }

    void Update()
    {
        if (!isRotating && !isFading)
            return;

        if (isRotating && controlRotation)
        {
            float step = Time.deltaTime * rotationSpeed;
            float newY = isOn ? (transform.localEulerAngles.y % 180) - step : (transform.localEulerAngles.y % 180) + step;
            float newZ = isOn ? (transform.localEulerAngles.z % 180) - step : (transform.localEulerAngles.z % 180) + step;

            transform.localEulerAngles = new Vector3(0.0f, newY, newZ);
            float currentEdge = isOn ? Mathf.Abs(transform.localEulerAngles.y) : Mathf.Abs(transform.localEulerAngles.y - initialY);
            if (currentEdge < step * 2)
            {
                transform.localEulerAngles = isOn ? Vector3.zero : new Vector3(0.0f, initialY, initialZ);
                isRotating = false;
            }
        }

        if (isFading && controlColor)
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
        if (!controlRotation)
            return;

        Vector3 targetDirection = Camera.main.transform.position - transform.position;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, Mathf.PI, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
        initialY = transform.localEulerAngles.y + 90.0f;
        initialZ = transform.localEulerAngles.z + 90.0f;
        initialY = initialY % 360.0f;
        initialZ = initialZ % 360.0f;
        if (initialY < 0.0f)
            initialY += 180.0f;
        if (initialY > 180.0f)
            initialY -= 180.0f;
        if (initialZ < 0.0f)
            initialZ += 180.0f;
        if (initialZ > 180.0f)
            initialZ -= 180.0f;
        transform.localEulerAngles = new Vector3(0.0f, initialY, initialZ);
    }

    public void Show()
    {
        isOn = true;
        startOffset = XFromCenter;
        StartCoroutine(StartRotation());
        SetColor(modifiedColor);
    }

    public void Hide()
    {
        isOn = false;
        startOffset = Mathf.Min(X, XFromEnd);
        StartCoroutine(StartRotation());
        SetColor(hideColor);
    }

    public void SetColor(Color color)
    {
        targetColor = color;
        if (controlColor)
            isFading = true;
    }

    public void AddDarkShade()
    {
        if (currentLightLevel > 0)
        {
            Map.instance.UpdateAverageForTileShift(false);
            currentLightLevel -= 1;
        }
        UpdateSpriteColor(CalculateLightLevelColor());
    }

    public void AddLightShade()
    {
        if (currentLightLevel < Map.instance.lightLevels)
        {
            Map.instance.UpdateAverageForTileShift(true);
            currentLightLevel += 1;
        }
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
        if (controlRotation)
            isRotating = true;
    }
}
