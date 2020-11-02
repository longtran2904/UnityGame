using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Projectile : MonoBehaviour, IPooledObject
{
    public float speed;
    public float timer;
    private Rigidbody2D rb;
    private Player player;

    private int damage;
    private Vector2 knockbackForce;
    private bool isCritical;
    private bool isEnemy;
    private GameObject hitEffect;

    // State for enemy
    private State state;
    private Material hurtMat;

    public bool canTouchGround; // can go through wall and grounds
    public bool canTouchPlayer; // go through player and still damage him
    
    public void OnObjectSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
        StartCoroutine(GameUtils.Deactive(gameObject, timer));
    }

    public void Init(int damage, Vector2 knockback, GameObject hitEffect, bool isEnemy, bool isCritical)
    {
        this.damage = damage;
        knockbackForce = knockback;
        this.hitEffect = hitEffect;
        this.isEnemy = isEnemy;
        this.isCritical = isCritical;
    }

    public void Init(int damage, Vector2 knockback, GameObject hitEffect, State state, Material hurtMat = null)
    {
        this.damage = damage;
        knockbackForce = knockback;
        this.hitEffect = hitEffect;
        this.state = state;
        this.hurtMat = hurtMat;
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
        player = collision.GetComponent<Player>();
        if (transform.position.x <= player.transform.position.x)
        {
            player.Hurt(damage, knockbackForce);
        }
        else
        {
            player.Hurt(damage, new Vector2(-knockbackForce.x, knockbackForce.y));
        }
        if (!canTouchPlayer)
        {
            gameObject.SetActive(false);
        }
    }

    private void HitEnemy(Collider2D collision)
    {
        Enemies enemy = collision.GetComponent<Enemies>();
        Vector2 knockbackForce = (-transform.position + enemy.transform.position).normalized * this.knockbackForce;
        enemy.Hurt(damage, knockbackForce, hurtMat);
        DamagePopup.Create(collision.transform.position, damage, isCritical);
        gameObject.SetActive(false);
        if (state != null)
            StateManager.AddStateToEnemy(enemy, state);
    }
}
