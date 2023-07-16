using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inn : MonoBehaviour
{
    public static Inn instance;

    [Header("Camera")]
    public Transform mapOpenedTransform;
    public float cameraMoveSpeed = 1.0f;
    public bool trackMouse = false;
    public Vector2 trackMouseAngleRange = new Vector2(20.0f, 10.0f);

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    private Vector3 targetCameraPosition;
    private Quaternion targetCameraRotation;
    private Quaternion modifiedCameraRotation;

    [HideInInspector] public bool isCameraMoving = false;

    private bool isMapOpened = false;

    void Start()
    {
        instance = this;

        originalCameraPosition = Camera.main.transform.position;
        originalCameraRotation = Camera.main.transform.rotation;
        targetCameraPosition = originalCameraPosition;
        targetCameraRotation = originalCameraRotation;
    }

    void Update()
    {
        if (!isCameraMoving && !trackMouse)
            return;

        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetCameraPosition, Time.deltaTime * cameraMoveSpeed);
        Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, modifiedCameraRotation, Time.deltaTime * cameraMoveSpeed);

        if (Vector3.Distance(Camera.main.transform.position, targetCameraPosition) < 0.01f && (Quaternion.Angle(Camera.main.transform.rotation, modifiedCameraRotation) < 0.01f && !trackMouse))
        {
            Camera.main.transform.position = targetCameraPosition;
            Camera.main.transform.rotation = modifiedCameraRotation;
            isCameraMoving = false;
        }

        if (trackMouse && !isMapOpened)
        {
            UpdateCameraRotation(originalCameraRotation);
        }
    }

    public void ShowMap()
    {
        UpdateCameraRotation(mapOpenedTransform.rotation, true);
        targetCameraPosition = mapOpenedTransform.position;
        isCameraMoving = true;
        AudioRevolver.Fire(AudioNames.BirdsSound);
        isMapOpened = true;
    }

    public void HideMap()
    {
        UpdateCameraRotation(originalCameraRotation);
        targetCameraPosition = originalCameraPosition;
        isCameraMoving = true;
        AudioRevolver.Fire(AudioNames.BirdsSound + "/Stop");
        isMapOpened = false;
        QuestInfo.HideAll();
    }

    private void UpdateCameraRotation(Quaternion targetRotation, bool ignoreMouse = false)
    {
        targetCameraRotation = targetRotation;
        modifiedCameraRotation = targetRotation;
        if (trackMouse && !ignoreMouse)
        {
            Vector3 mousePosition = Input.mousePosition;
            float x = mousePosition.x / Screen.width;
            float y = 1.0f - mousePosition.y / Screen.height;
            float xAngle = Mathf.Lerp(-trackMouseAngleRange.x, trackMouseAngleRange.x, x);
            float yAngle = Mathf.Lerp(-trackMouseAngleRange.y, trackMouseAngleRange.y, y);
            modifiedCameraRotation = Quaternion.Euler(yAngle, xAngle, 0.0f);
        }
        // isCameraMoving = true;
    }

    public bool IsCloserToTarget()
    {
        Vector3 targetPosition = isMapOpened ? mapOpenedTransform.position : originalCameraPosition;
        Vector3 originalPosition = isMapOpened ? originalCameraPosition : mapOpenedTransform.position;
        return Vector3.Distance(Camera.main.transform.position, targetPosition) < Vector3.Distance(Camera.main.transform.position, originalPosition);
    }
}
