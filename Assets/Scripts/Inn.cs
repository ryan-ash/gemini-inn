using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inn : MonoBehaviour
{
    [Header("Camera")]
    public Transform mapOpenedTransform;
    public float cameraMoveSpeed = 1.0f;

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    private Vector3 targetCameraPosition;
    private Quaternion targetCameraRotation;

    private bool isCameraMoving = false;

    // Start is called before the first frame update
    void Start()
    {
        originalCameraPosition = Camera.main.transform.position;
        originalCameraRotation = Camera.main.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isCameraMoving)
            return;

        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetCameraPosition, Time.deltaTime * cameraMoveSpeed);
        Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, targetCameraRotation, Time.deltaTime * cameraMoveSpeed);

        if (Vector3.Distance(Camera.main.transform.position, targetCameraPosition) < 0.01f && Quaternion.Angle(Camera.main.transform.rotation, targetCameraRotation) < 0.01f)
        {
            Camera.main.transform.position = targetCameraPosition;
            Camera.main.transform.rotation = targetCameraRotation;
            isCameraMoving = false;
        }
    }

    public void ShowMap()
    {
        targetCameraRotation = mapOpenedTransform.rotation;
        targetCameraPosition = mapOpenedTransform.position;
        isCameraMoving = true;
    }

    public void HideMap()
    {
        targetCameraRotation = originalCameraRotation;
        targetCameraPosition = originalCameraPosition;
        isCameraMoving = true;
    }
}
