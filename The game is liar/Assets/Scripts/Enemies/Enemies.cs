using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Maggot,
    Bat,
    Turret,
    Alien,
    Jelly,
    NoEye
}

public class Enemies : MonoBehaviour
{
    public static int numberOfEnemiesAlive = 0;

    public int health;
    public int damage;
    public float dropRate;
    public Vector2Int moneyDropRange;
    protected EnemiesMovement movement;
    [HideInInspector] public EnemyType enemyType;
    protected Player player;

    public Material matWhite;
    private Material matDefault;
    private SpriteRenderer sr;

    public GameObject explosionParitcle;
    public GameObject spawnEffect;
    private GameObject spawnObject;
    public bool hasSpawnVFX = true;
    private bool componentEnable = true;
    
    protected virtual void Start()
    {
        Setup();
        SpawnEnemy();
        Destroy(spawnObject, 1);
    }

    void Setup()
    {
        movement = GetComponent<EnemiesMovement>();
        sr = GetComponentInChildren<SpriteRenderer>() ?? GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        matDefault = sr.material;
        EnemySpawner.numberOfEnemiesAlive++;
    }

    void SpawnEnemy()
    {
        numberOfEnemiesAlive += 1;
        if (hasSpawnVFX)
        {
            spawnObject = Instantiate(spawnEffect, transform.position, Quaternion.identity);
            DisableAllOtherBehaviours();
            DisableAllChildObjects();
        }
    }

    void DisableAllOtherBehaviours()
    {
        sr.enabled = false;
        GetComponent<Rigidbody2D>().gravityScale = 0;
        foreach (var component in GetComponents<Behaviour>())
        {
            if (component == this)
            {
                continue;
            }
            component.enabled = false;
        }
    }

    void DisableAllChildObjects()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (isSpawnEffectDone() && hasSpawnVFX)
        {
            EnableAllOtherBehaviours();
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            }
            if (health <= 0)
            {
                Death();
            }
        }
    }

    bool isSpawnEffectDone()
    {
        if (spawnObject == null)
        {
            return true;
        }
        return false;
    }

    void EnableAllOtherBehaviours()
    {
        if (componentEnable)
        {
            foreach (var component in GetComponents<Behaviour>())
            {
                component.enabled = true;
            }

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }

            sr.enabled = true;

            GetComponent<Rigidbody2D>().gravityScale = 1;

            componentEnable = false;
        }
    }

    public void Death()
    {
        GameObject explosion = explosionParitcle;
        Instantiate(explosion, transform.position, Quaternion.identity);
        numberOfEnemiesAlive -= 1;
        DropMoney(CalculateDropMoney());
        EnemySpawner.numberOfEnemiesAlive--;
        Destroy(gameObject);
    }

    int CalculateDropMoney()
    {
        bool canDrop = MathUtils.RandomBool(dropRate);
        if (canDrop)
        {
            return Random.Range(moneyDropRange.x, moneyDropRange.y + 1);
        }
        return 0;
    }

    void DropMoney(int dropMoney)
    {
        Vector3 offset = new Vector2(0, 1f);
        for (int i = 0; i < dropMoney; i++)
        {
            ObjectPooler.instance.SpawnFromPool("Money", transform.position + offset, Quaternion.identity);
        }
    }

    public void Hurt(int _damage, Vector2 _knockbackForce = new Vector2())
    {
        health -= _damage;
        sr.material = matWhite;
        Invoke("ResetMaterial", .1f);
        AudioManager.instance.Play("GetHit");
        movement.KnockBack(_knockbackForce);
    }

    public void ResetMaterial()
    {
        sr.material = matDefault;
    }

    public void DamagePlayerWhenCollide(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player.Hurt(damage);
        }
    }

    public void DamagePlayerWhenCollide(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            player.Hurt(damage);
        }
    }
}
