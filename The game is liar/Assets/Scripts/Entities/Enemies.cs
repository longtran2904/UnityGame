using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Maggot,
    Bat,
    Hunter
}

public class Enemies : MonoBehaviour
{

    public float health;
    public float damage;

    private Player player;

    public EnemyType enemyType;

    public Material matWhite;
    [HideInInspector]
    public Material matDefault;

    [HideInInspector]
    public SpriteRenderer sr;

    public GameObject explosionParitcle;

    private void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
        }
        matDefault = sr.material;
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            Death();
        }
    }

    void Death()
    {
        GameObject explosion = explosionParitcle;
        Instantiate(explosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public void ResetMaterial()
    {
        sr.material = matDefault;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && enemyType != EnemyType.Bat)
        {
            player = collision.gameObject.GetComponent<Player>();
            player.health -= damage;
        }
    }
}
