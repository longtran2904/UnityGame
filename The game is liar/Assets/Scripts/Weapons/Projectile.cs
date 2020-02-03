using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed;

    private Rigidbody2D rb;

    private Enemies enemy;

    public float timer;

    [HideInInspector]
    public float damage;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
    }

    private void Update()
    {
        if (timer <= 0)
        {
            Destroy(this.gameObject);
        }
        else
        {
            timer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            enemy = collision.GetComponent<Enemies>();
            enemy.health -= damage;
            enemy.sr.material = enemy.matWhite;
            enemy.Invoke("ResetMaterial", .1f);
            Destroy(gameObject);
        }
        if (collision.tag == "Ground")
        {
            Destroy(gameObject);
        }
    }
}
