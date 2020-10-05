using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyObject : MonoBehaviour, IPooledObject
{
    public float speed;

    private Rigidbody2D rb;
    private Collider2D player;

    public void OnObjectSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(Random.Range(-1, 1), Random.value).normalized * speed;
    }

    void Update()
    {
        if (!player)
            player = Physics2D.OverlapCircle(transform.position, 5f, LayerMask.GetMask("Player"));
        else
        {
            Vector2 dir = player.transform.position - transform.position;
            rb.velocity = dir * speed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            AudioManager.instance.Play("PickupCoin");
            collision.gameObject.GetComponent<Player>().money += 5;
            gameObject.SetActive(false);
        }
    }
}
