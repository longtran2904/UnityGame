using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleBehaviour : StateMachineBehaviour
{

    public float timer;
    [SerializeField]
    private float timeValue;
    private JumpBehaviour jumpBehaviour;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timeValue = timer;
        jumpBehaviour = animator.GetBehaviour<JumpBehaviour>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (timeValue <= 0 && jumpBehaviour && jumpBehaviour.jumpCounts < jumpBehaviour.maxJumps )
        {
            animator.ResetTrigger("Idle");
            animator.SetTrigger("Jump");
            Debug.Log("SetTriggerJump");
            Debug.Log("Jump counts: " + jumpBehaviour.jumpCounts);
        }
        else if (timeValue <= 0 && jumpBehaviour.jumpCounts >= jumpBehaviour.maxJumps)
        {
            animator.ResetTrigger("Idle");
            animator.ResetTrigger("Jump");
            animator.SetTrigger("Dash");
            Debug.Log("Set Trigger Dash");
            Debug.Log("Jump counts: " + jumpBehaviour.jumpCounts);
        }
        else
        {
            timeValue -= Time.deltaTime;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timeValue = timer; 
    }

}
