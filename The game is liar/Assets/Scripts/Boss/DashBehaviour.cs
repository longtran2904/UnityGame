using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashBehaviour : StateMachineBehaviour
{

    private Rigidbody2D rb;

    public float dashSpeed;

    public float timer;
    [SerializeField]
    private float timeValue;

    private int dashCounts;
    public int maxDash;
    private JumpBehaviour jumpBehaviour;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb = animator.GetComponent<Rigidbody2D>();
        jumpBehaviour = animator.GetBehaviour<JumpBehaviour>();
        rb.velocity = -animator.transform.right * dashSpeed;
        dashCounts += 1;
        jumpBehaviour.jumpCounts = 0;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if ((dashCounts >= maxDash) && timeValue <= 0)
        {
            jumpBehaviour.spikeCanFalling = true;
            animator.ResetTrigger("Idle");
            animator.ResetTrigger("Dash");            
            animator.SetTrigger("Jump");
            dashCounts = 0;
            Debug.Log("d");
        }
        else if (timeValue <= 0 && dashCounts < maxDash)
        {
            animator.ResetTrigger("Dash");
            animator.ResetTrigger("Jump");
            animator.SetTrigger("Idle");
        }
        else
        {
            timeValue -= Time.deltaTime;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb.velocity = Vector2.zero;
        timeValue = timer;
    }
}
