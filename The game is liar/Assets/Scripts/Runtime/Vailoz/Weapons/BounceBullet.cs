using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceBullet : Projectile
{
    public ChangeColor colorWhenHit;
    private Vector2 startVelocity;
    private SpriteRenderer sr;

    private void Start()
    {
        colorWhenHit = Instantiate(colorWhenHit);
    }

    public override void OnObjectSpawn()
    {
        base.OnObjectSpawn();
        startVelocity = rb.velocity;
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hitEffect) Instantiate(hitEffect, transform.position, Quaternion.identity);

        if (collision.collider.CompareTag("BounceMat"))
        {
            Vector2 reflect = Vector2.Reflect(startVelocity, collision.GetContact(0).normal);
            rb.velocity = reflect;
            startVelocity = reflect;
            transform.right = reflect;
            colorWhenHit.Change(sr);
        }
        else if (collision.collider.CompareTag("Ground") || collision.collider.CompareTag("Player"))
        {
            audioManager.PlaySfx("HitWall");
            gameObject.SetActive(false);
        }
    }
}
