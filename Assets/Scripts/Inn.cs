using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inn : MonoBehaviour
{
    public static Inn instance;

    [Header("Camera")]
    public Transform mapOpenedTransform;
    public float cameraMoveSpeed = 1.0f;

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    private Vector3 targetCameraPosition;
    private Quaternion targetCameraRotation;

    [HideInInspector] public bool isCameraMoving = false;

    void Start()
    {
        instance = this;

        originalCameraPosition = Camera.main.transform.position;
        originalCameraRotation = Camera.main.transform.rotation;
    }

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
        AudioRevolver.Fire(AudioNames.BirdsSound);
    }

    public void HideMap()
    {
        targetCameraRotation = originalCameraRotation;
        targetCameraPosition = originalCameraPosition;
        isCameraMoving = true;
        AudioRevolver.Fire(AudioNames.BirdsSound + "/Stop");
    }
}
