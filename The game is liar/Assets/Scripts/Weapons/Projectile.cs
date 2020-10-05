using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IPooledObject
{
    public float speed;

    private Rigidbody2D rb;

    private Enemies enemy;

    public float timer;

    [HideInInspector]
    public int damage;
    [HideInInspector]
    public Vector2 knockbackForce;

    [HideInInspector]
    public bool isCritical;
    public bool isEnemy;
    public bool canTouchGround;
    public bool canTouchPlayer;

    private Player player;

    [HideInInspector] public GameObject hitEffect;

    public void OnObjectSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
        StartCoroutine(GameUtils.Deactive(gameObject, timer));
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
        enemy = collision.GetComponent<Enemies>();
        Vector2 knockbackForce = (-transform.position + enemy.transform.position).normalized * this.knockbackForce;
        enemy.Hurt(damage, knockbackForce);
        DamagePopup.Create(collision.transform.position, damage, isCritical);
        gameObject.SetActive(false);
    }
}
