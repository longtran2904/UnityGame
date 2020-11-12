using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Projectile : MonoBehaviour, IPooledObject
{
    public float speed;
    public float timer;
    private Rigidbody2D rb;

    private int damage;
    private float knockbackForce;
    private float knockbackTime;
    private bool isCritical;
    private bool isEnemy;
    public GameObject hitEffect;

    // State for enemy
    private State state;

    public bool canTouchGround; // can go through wall and grounds
    public bool canTouchPlayer; // go through player and still damage him
    
    public void OnObjectSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
        StartCoroutine(GameUtils.Deactive(gameObject, timer));
    }

    public void Init(int damage, float knockbackForce, float knockbackTime, bool isEnemy, bool isCritical)
    {
        this.damage = damage;
        this.knockbackForce = knockbackForce;
        this.knockbackTime = knockbackTime;
        this.isEnemy = isEnemy;
        this.isCritical = isCritical;
    }

    public void Init(int damage, float knockbackForce, float knockbackTime, State state)
    {
        this.damage = damage;
        this.knockbackForce = knockbackForce;
        this.knockbackTime = knockbackTime;
        this.state = state;
    }

    public void SetVelocity(float speed)
    {
        rb.velocity = speed * transform.right;
    }

    public void SetVelocity(float speed, float angle)
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
        SetVelocity(speed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        SpawnHitEffect();

        if (collision.tag == "Enemy" && !isEnemy)
        {
            HitEnemy(collision);
        }
        else if (collision.tag == "Player" && isEnemy)
        {
            HitPlayer(collision);
        }
        else if (collision.tag == "Boss" && !isEnemy)
        {
            HitBoss(collision);
        }
        if (collision.tag == "Ground" && !canTouchGround)
        {
            gameObject.SetActive(false);
        }
    }

    private void SpawnHitEffect()
    {
        if (hitEffect)
        {
            hitEffect = Instantiate(hitEffect, transform.position, transform.rotation);
            Destroy(hitEffect, hitEffect.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
        }
    }

    private void HitBoss(Collider2D collision)
    {
        collision.GetComponent<Boss>().GetHurt(damage);
        DamagePopup.Create(collision.transform.position, damage, isCritical);
        gameObject.SetActive(false);
    }

    private void HitPlayer(Collider2D collision)
    {
        collision.GetComponent<Player>().Hurt(damage, knockbackForce * (collision.transform.position - transform.position).normalized);
        if (!canTouchPlayer)
        {
            gameObject.SetActive(false);
        }
    }

    private void HitEnemy(Collider2D collision)
    {
        Enemies enemy = collision.GetComponent<Enemies>();
        Vector2 knockbackForce = (-transform.position + enemy.transform.position).normalized * this.knockbackForce;
        enemy.Hurt(damage, knockbackForce, knockbackTime);
        DamagePopup.Create(collision.transform.position, damage, isCritical);
        gameObject.SetActive(false);
        if (state != null)
            StateManager.AddStateToEnemy(enemy, state);
    }
}
