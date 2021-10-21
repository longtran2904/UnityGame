using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpBehaviour : StateMachineBehaviour
{

    public float timer;
    [SerializeField]
    private float timeValue;

    private Transform playerPos;

    public float speed;

    private Rigidbody2D rb;

    public int jumpCounts;
    public int maxJumps;
    public bool spikeCanFalling;
    public bool spikeFalling;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timeValue = timer;
        playerPos = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        rb = animator.GetComponent<Rigidbody2D>();

        if (spikeCanFalling)
        {            
            timeValue = 2.1f;
            InternalDebug.Log(timeValue);
            spikeFalling = true;
            spikeCanFalling = false;
            InternalDebug.Log(spikeCanFalling);
        }
        else
        {
            Vector2 target = new Vector2(playerPos.position.x, animator.gameObject.transform.position.y);
            rb.velocity = -animator.transform.right * speed;
            jumpCounts += 1;
        }       
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (timeValue <= 0)
        {
            animator.ResetTrigger("Jump");
            animator.SetTrigger("Idle");
            InternalDebug.Log("Set trigger idle");
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
        if (timeValue >= 1)
        timeValue = timer;
    }

}
