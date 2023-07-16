using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public BiomePreset selectedBiome;
    public float waitBeforeRotating = 0.1f;
    public float rotationSpeed = 10.0f;
    public int X;
    public int Y;
    public int XFromEnd;
    public int XFromCenter;

    private bool isRotating = false;
    private bool isOn = false;
    private float initialY = 0.0f;
    private int startOffset = 0;

    void Update()
    {
        if (!isRotating)
            return;

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

    public void Prepare()
    {
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
    }

    public void Hide()
    {
        isOn = false;
        startOffset = Mathf.Min(X, XFromEnd);
        StartCoroutine(StartRotation());
    }

    IEnumerator StartRotation()
    {
        yield return new WaitForSeconds(waitBeforeRotating * startOffset);
        isRotating = true;
    }
}
