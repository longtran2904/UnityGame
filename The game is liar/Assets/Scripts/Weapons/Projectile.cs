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

    public bool isEnemy;

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
        if (hitEffect)
        {
            hitEffect = Instantiate(hitEffect, transform.position, transform.rotation);

            Destroy(hitEffect, hitEffect.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
        }

        if (collision.tag == "Enemy" && !isEnemy)
        {
            enemy = collision.GetComponent<Enemies>();
            enemy.knockbackForce = (-transform.position + enemy.transform.position).normalized * knockbackForce;
            enemy.Hurt(damage);
            DamagePopup.Create(collision.transform.position, damage, false);

            gameObject.SetActive(false);
        }
        else if (collision.tag == "Player" && isEnemy)
        {
            player = collision.GetComponent<Player>();

            if (transform.position.x <= player.transform.position.x)
            {
                player.knockbackForce.x = knockbackForce.x;
            }
            else
            {
                player.knockbackForce.x = -knockbackForce.x;
            }

            player.knockbackForce.y = knockbackForce.y;

            player.Hurt(damage);

            gameObject.SetActive(false);
        }
        else if (collision.tag == "Boss" && !isEnemy)
        {
            collision.GetComponent<GiantEyeBoss>().GetHurt(damage);
            DamagePopup.Create(collision.transform.position, damage, false);

            gameObject.SetActive(false);
        }

        if (collision.tag == "Ground")
        {
            gameObject.SetActive(false);
        }
    }
}
