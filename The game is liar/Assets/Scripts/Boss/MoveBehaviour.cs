using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBehaviour : StateMachineBehaviour
{
    public float speed;
    public float dashSpeed;
    public int chargeDamage;

    public float attackTime;
    private float attackTimeValue;
    bool isAttack = false;

    public float dashTime;
    private float dashTimeValue;

    public float chargeTime;
    public float timeBtwFlash;
    public float flashTime;
    private float chargeTimeValue;
    private float timeBtwFlashValue;
    private float flashTimeValue;

    public Material chargeMat;
    private Material defMat;

    public Vector2 leftAndUpLimit;
    public Vector2 rightAndBottomLimit;

    Transform player;
    Rigidbody2D rb;
    SpriteRenderer sr;

    GiantEyeBoss boss;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = animator.GetComponent<Rigidbody2D>();
        sr = animator.GetComponent<SpriteRenderer>();
        defMat = sr.material;
        boss = animator.GetComponent<GiantEyeBoss>();
        dashTimeValue = dashTime;
        attackTimeValue = attackTime;
        chargeTimeValue = chargeTime;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (boss.health <= boss.enragedHealth)
        {
            Enraged(animator);
            return;
        }

        if (Time.time > attackTimeValue)
        {
            ChargeAttack();

            return;
        }
        else if (isAttack)
        {
            if (dashTimeValue <= 0)
            {
                isAttack = false;
            }

            dashTimeValue -= Time.deltaTime;

            ClampPosition();

            return;
        }

        boss.LookAtPlayer();

        Vector2 newPos = Vector2.MoveTowards(animator.transform.position, player.position, speed * Time.fixedDeltaTime);

        rb.MovePosition(newPos);

        ClampPosition();
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    void ChargeAttack()
    {
        if (chargeTimeValue <= 0)
        {
            sr.material = defMat;
            Attack();
            return;
        }

        chargeTimeValue -= Time.deltaTime;

        rb.velocity = Vector2.zero;

        if (sr.material.color == chargeMat.color)
        {
            flashTimeValue -= Time.deltaTime;
        }

        if (flashTimeValue <= 0)
        {
            sr.material = defMat;
            flashTimeValue = flashTime;
        }

        if (Time.time >= timeBtwFlashValue)
        {
            sr.material = chargeMat;
            timeBtwFlashValue = Time.time + timeBtwFlash;
        }
    }

    void Attack()
    {
        Vector2 target = player.transform.position - rb.transform.position;

        rb.velocity = target.normalized * dashSpeed;

        attackTimeValue = attackTime + Time.time;

        isAttack = true;

        dashTimeValue = dashTime;

        chargeTimeValue = chargeTime;

        boss.damage = chargeDamage;

        ClampPosition();
    }

    public void Enraged(Animator _anim)
    {
        if (rb.transform.position == boss.enragedPos.position)
        {
            boss.isInvulnerable = false;
            _anim.SetBool("isEnraged", true);
            Debug.Log("Change to enraged animation");
        }

        rb.MovePosition(Vector2.MoveTowards(_anim.transform.position, boss.enragedPos.position, speed * Time.fixedDeltaTime));

        Debug.Log("Moving to enraged position with velocity: " + rb.velocity);
        Debug.Log("enraged position is: " + boss.enragedPos.position);

        boss.isInvulnerable = true;
    }

    void ClampPosition()
    {
        rb.transform.position = new Vector3(
            Mathf.Clamp(rb.transform.position.x, leftAndUpLimit.x, rightAndBottomLimit.x),
            Mathf.Clamp(rb.transform.position.y, rightAndBottomLimit.y, leftAndUpLimit.y),
            0
            );
    }
}
