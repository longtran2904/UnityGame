﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private Animator anim;
    private GameObject weaponHolder;
    private Player player;

    public Transform attackPoint;
    public LayerMask enemyLayers;

    public float attackRange;
    public int damage;
    public float knockbackForce;
    public float attackRate;
    private float nextAttackTime;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        weaponHolder = transform.GetChild(0).gameObject;
        player = GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > nextAttackTime)
        {
            weaponHolder.SetActive(true);
            if (Input.GetKeyDown(KeyCode.V) && (player.inventory.GetCurrent().GetComponent<ActiveReload>()?.isReloading ?? true)) // Can't attack when player is reloading
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
        weaponHolder.SetActive(false);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (var hitEnemy in hitEnemies)
        {
            hitEnemy.gameObject.GetComponent<Enemy>().Hurt(damage);
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
