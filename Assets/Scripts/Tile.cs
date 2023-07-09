using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public BiomePreset selectedBiome;
    public float waitBeforeRotating = 0.1f;
    public float rotationSpeed = 10.0f;
    public int N;
    public int NFromEnd;

    private bool isRotating = false;
    private bool isOn = false;
    private float initialY = 0.0f;

    // Update is called once per frame
    void Update()
    {
        if (isRotating /* && transform.localEulerAngles.y > 0.0f*/)
        {
            float step = Time.deltaTime * rotationSpeed;
            float newY = isOn ? transform.localEulerAngles.y - step : transform.localEulerAngles.y + step;
            transform.localEulerAngles = new Vector3(0.0f, newY, 0.0f);

            float currentEdge = isOn ? Mathf.Abs(transform.localEulerAngles.y) : Mathf.Abs(transform.localEulerAngles.y - initialY);
            if (currentEdge < step * 2)
            {
                transform.localEulerAngles = isOn ? Vector3.zero : new Vector3(0.0f, initialY, 0.0f);
                isRotating = false;
            }
        }
    }

    public void Prepare()
    {
        Vector3 targetDirection = Camera.main.transform.position - transform.position;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, Mathf.PI, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
        initialY = transform.localEulerAngles.y + 90.0f;
        if (initialY < 0.0f)
            initialY += 180.0f;
        if (initialY > 180.0f)
            initialY -= 180.0f;
        transform.localEulerAngles = new Vector3(0.0f, initialY, 0.0f);
    }

    public void Show()
    {
        isOn = true;
        StartCoroutine(StartRotation());
    }

    public void Hide()
    {
        isOn = false;
        StartCoroutine(StartRotation());
    }

    IEnumerator StartRotation()
    {
        float startOffset = Mathf.Min(N, NFromEnd);
        yield return new WaitForSeconds(waitBeforeRotating * startOffset);
        isRotating = true;
    }
}
