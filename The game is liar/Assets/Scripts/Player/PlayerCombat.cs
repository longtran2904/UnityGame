using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private Animator anim;

    public Transform attackPoint;
    public LayerMask enemyLayers;

    public float attackRange;
    public int damage;
    public float attackRate;
    private float nextAttackTime;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > nextAttackTime)
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                Attack();
                nextAttackTime = Time.time + 1 / attackRate;
            }
        }
    }

    void Attack()
    {
        // Play animation
        anim.SetTrigger("Attack");

        // Detect enemies in attack range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Damage each enemies
        foreach (var hitEnemy in hitEnemies)
        {
            hitEnemy.gameObject.GetComponent<Enemies>().Hurt(damage);
        }
    }

    private void OnDrawGizmos()
    {
        if (attackPoint != null)
        {
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
