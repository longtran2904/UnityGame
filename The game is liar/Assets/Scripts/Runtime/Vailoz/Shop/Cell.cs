using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour, IPooledObject
{
    public float speed;
    public IntReference addMoney;
    public IntReference playerMoney;
    public AudioManager audioManager;
    
    private Rigidbody2D rb;
    private Transform player;

    public void OnObjectSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(Random.Range(-1, 1), Random.value).normalized * speed;
    }

    // Call by game event listener
    public void MoveTowardPlayer()
    {
        if (player) return;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        Collider2D collider = GetComponent<Collider2D>();
        collider.isTrigger = true;
        collider.attachedRigidbody.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        if (player)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.velocity = dir * speed;
        }
        else
        {
            player = Physics2D.OverlapCircle(transform.position, 5f, LayerMask.GetMask("Player"))?.transform;
            if (player)
            {
                Collider2D collider = GetComponent<Collider2D>();
                collider.isTrigger = true;
                collider.attachedRigidbody.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.collider);
    }

    void HandleCollision(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            audioManager.PlaySfx("PickupCoin");
            playerMoney.value += addMoney.value;

            gameObject.SetActive(false);
            Collider2D collider = GetComponent<Collider2D>();
            collider.isTrigger = false;
            collider.attachedRigidbody.bodyType = RigidbodyType2D.Dynamic;
        }
    }
}
