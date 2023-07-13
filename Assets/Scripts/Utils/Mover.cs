using UnityEngine;

public class Mover : MonoBehaviour {
    public float moveSpeed = 10.0f;
    public Vector3 moveDirection = Vector3.up;
    public bool startMoving = true;

    private bool isMoving = false;

    void Start() {
        if (startMoving) {
            StartMoving();
        }
    }
    
    void Update() {
        if (isMoving) {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }

    public void StartMoving() {
        isMoving = true;
    }

    public void StopMoving() {
        isMoving = false;
    }
}
