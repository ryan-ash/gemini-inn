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

    // Start is called before the first frame update
    void Start()
    {
        Vector3 targetDirection = Camera.main.transform.position - transform.position;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, Mathf.PI, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
        float initialY = transform.localEulerAngles.y + 90.0f;
        if (initialY < 0.0f)
            initialY += 180.0f;
        if (initialY > 180.0f)
            initialY -= 180.0f;
        transform.localEulerAngles = new Vector3(0.0f, initialY, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (isRotating && transform.localEulerAngles.y > 0.0f)
        {
            float step = Time.deltaTime * rotationSpeed;
            transform.localEulerAngles = new Vector3(0.0f, transform.localEulerAngles.y - step, 0.0f);
            if (Mathf.Abs(transform.localEulerAngles.y) < step * 2)
            {
                transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                isRotating = false;
            }
        }        
    }

    public void Prepare()
    {
        StartCoroutine(RotateToDefault());
    }

    IEnumerator RotateToDefault()
    {
        float startOffset = Mathf.Min(N, NFromEnd);
        yield return new WaitForSeconds(waitBeforeRotating * startOffset);
        isRotating = true;
    }
}
