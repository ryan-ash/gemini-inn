using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedCharacterController : MonoBehaviour
{
    private PingPongMover moverInstance;
    private PingPongMover mover {
        get {
            if (!moverInstance) {
                moverInstance = GetComponent<PingPongMover>();
            }
            return moverInstance;
        }
    }

    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetIntoPosition() {
        mover.Move("Idle", true);
        Front();
        Move();
    }

    public void GoBack() {
        mover.MoveBack();
        Back();
    }

    public void Idle() {
        animator.SetBool("Moving", false);
    }

    public void Move() {
        animator.SetBool("Moving", true);
    }

    public void Front() {
        animator.SetBool("Back", false);
    }

    public void Back() {
        animator.SetBool("Back", true);
    }
}
