using UnityEngine;

public class PingPongMover : MonoBehaviour {
    public float moveTime = 3.0f;
    public LeanTweenType easeType = LeanTweenType.easeInOutQuad;
    public Transform finalTransform;
    public bool moveOnStart = false;
    public bool snapOnStart = false;

    private Vector3 initialPosition, initialScale;
    private Quaternion initialRotation;

    void Start() {
        initialPosition = transform.position;
        initialScale = transform.localScale;
        initialRotation = transform.rotation;
        if (snapOnStart) {
            SnapToTarget();
        } else if (moveOnStart) {
            Move();
        }
    }
    
    void Update() {

    }

    public void Move(string callbackMessage = "", bool sendToSelf = false) {
        // lerp whole transform, not just location

        LeanTween.value(gameObject, 0.0f, 1.0f, moveTime).setEase(easeType).setOnUpdate(
            (float value) => {
                transform.position = Vector3.Lerp(initialPosition, finalTransform.position, value);
                transform.rotation = Quaternion.Lerp(initialRotation, finalTransform.rotation, value);
                transform.localScale = Vector3.Lerp(initialScale, finalTransform.localScale, value);
            }
        ).setOnComplete(
            () => {
                if (callbackMessage != "") {
                    if (sendToSelf) {
                        gameObject.SendMessage(callbackMessage);
                    } else {
                        gameObject.SendMessageUpwards(callbackMessage);
                    }
                }
            }
        );
    }

    public void MoveBack(string callbackMessage = "", bool sendToSelf = false) {
        LeanTween.value(gameObject, 0.0f, 1.0f, moveTime).setEase(easeType).setOnUpdate(
            (float value) => {
                transform.position = Vector3.Lerp(finalTransform.position, initialPosition, value);
                transform.rotation = Quaternion.Lerp(finalTransform.rotation, initialRotation, value);
                transform.localScale = Vector3.Lerp(finalTransform.localScale, initialScale, value);
            }
        ).setOnComplete(
            () => {
                if (callbackMessage != "") {
                    if (sendToSelf) {
                        gameObject.SendMessage(callbackMessage);
                    } else {
                        gameObject.SendMessageUpwards(callbackMessage);
                    }
                }
            }
        );
    }

    public void SnapToTarget() {
        transform.position = finalTransform.position;
        transform.rotation = finalTransform.rotation;
        transform.localScale = finalTransform.localScale;
    }
}
