using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
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
    private int startOffset = 0;

    private Color targetColor = Color.white;
    private Color initialColor = Color.white;
    public Color darkColor = Color.red;
    public Color lightColor = Color.blue;
    public const float stepForce = 0.5f;
    private SpriteRenderer spriteRenderer;

    void Update()
    {
        if (!isRotating && !isFading)
            return;

        if (isRotating && controlRotation)
        {
            float step = Time.deltaTime * rotationSpeed;
            float newY = isOn ? (transform.localEulerAngles.y % 180) - step : (transform.localEulerAngles.y % 180) + step;

            transform.localEulerAngles = new Vector3(0.0f, newY, 0.0f);
            float currentEdge = isOn ? Mathf.Abs(transform.localEulerAngles.y) : Mathf.Abs(transform.localEulerAngles.y - initialY);
            if (currentEdge < step * 2)
            {
                transform.localEulerAngles = isOn ? Vector3.zero : new Vector3(0.0f, initialY, 0.0f);
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

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialColor = spriteRenderer.color;
        SetColor(initialColor);
        if (controlRotation || controlColor)
            Hide();
    }

    public void Prepare()
    {
        if (!controlRotation)
            return;

        Vector3 targetDirection = Camera.main.transform.position - transform.position;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, Mathf.PI, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
        initialY = transform.localEulerAngles.y + 90.0f;
        initialY = initialY % 360.0f;
        if (initialY < 0.0f)
            initialY += 180.0f;
        if (initialY > 180.0f)
            initialY -= 180.0f;
        transform.localEulerAngles = new Vector3(0.0f, initialY, 0.0f);
    }

    public void Show()
    {
        isOn = true;
        startOffset = XFromCenter;
        StartCoroutine(StartRotation());
        SetColor(initialColor);
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
        var currentColor = spriteRenderer.color;
        var lerpedColor = GetLerpedColor(currentColor, darkColor);
        spriteRenderer.color = lerpedColor;
        //Temp?
        initialColor = lerpedColor;
    }

    public void AddLightShade()
    {
        var currentColor = spriteRenderer.color;
        var lerpedColor = GetLerpedColor(currentColor, lightColor);
        spriteRenderer.color = lerpedColor;
        //Temp?
        initialColor = lerpedColor;
    }

    public void ResetShade()
    {
        spriteRenderer.color = initialColor;
    }

    private Color GetLerpedColor(Color start, Color finish)
    {
        return Color.Lerp(start, finish, stepForce);
    }

    IEnumerator StartRotation()
    {
        yield return new WaitForSeconds(waitBeforeRotating * startOffset);
        if (controlRotation)
            isRotating = true;
    }
}
