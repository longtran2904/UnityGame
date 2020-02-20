using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnragedBehaviour : StateMachineBehaviour
{
    public Projectile projectilePrefab;
    public float rotateSpeed;
    public float projectileSpeed;
    public int projectileDamage;

    public float timeBtwShots;
    private float timeBtwShotsValue;

    public float dyingTime;
    public float timeBtwFlash;
    public float flashTime;
    private float timeBtwFlashValue;
    private float flashTimeValue;

    public Material explodeMat;
    private Material defMat;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private GiantEyeBoss boss;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        boss = animator.GetComponent<GiantEyeBoss>();
        sr = animator.GetComponent<SpriteRenderer>();
        rb = animator.GetComponent<Rigidbody2D>();
        timeBtwShotsValue = timeBtwShots;
        timeBtwFlashValue = timeBtwFlash;
        flashTimeValue = flashTime;
        defMat = sr.material;
        rb.velocity = Vector2.zero;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (boss.health <= 0)
        {
            animator.speed = 0;
            if (dyingTime <= 0)
            {
                animator.speed = 1;
                animator.SetBool("isDied", true);
                return;
            }

            dyingTime -= Time.deltaTime;

            FinalExplode();

            return;
        }

        boss.isInvulnerable = false;

        if (Time.time >= timeBtwShotsValue)
        {
            EnragedAttack(animator);

            timeBtwShotsValue = Time.time + timeBtwShots;

            return;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    void FinalExplode()
    {
        if (flashTimeValue <= 0)
        {
            sr.material = defMat;
            flashTimeValue = flashTime;
        }

        if (sr.material.color == explodeMat.color)
        {
            flashTimeValue -= Time.deltaTime;
        }

        if (Time.time > timeBtwFlashValue)
        {
            sr.material = explodeMat;
            timeBtwFlashValue = Time.time + timeBtwFlash;
        }
    }

    public void EnragedAttack(Animator _anim)
    {
        for (int i = 0; i < boss.shootPos.Length; i++)
        {
            Vector2 difference = boss.shootPos[i].position - boss.enragedPos.position;

            float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

            Projectile projectile = Instantiate(projectilePrefab, boss.shootPos[i].position,
                Quaternion.Euler(0, 0, rotationZ)) as Projectile;

            projectile.isEnemy = true;

            projectile.speed = projectileSpeed;

            projectile.damage = projectileDamage;

            boss.shootPos[i].RotateAround(_anim.transform.position, Vector3.forward, rotateSpeed);
        }        
    }
}
