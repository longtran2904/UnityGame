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

    [HideInInspector] public bool touchingWall = false;

    private EnemiesMovement movement;

    [HideInInspector] public EnemyType enemyType;

    private Player player;

    public Material matWhite;
    private Material matDefault;
    private SpriteRenderer sr;

    public GameObject explosionParitcle;
    public GameObject spawnEffect;
    private GameObject spawnObject;

    private bool componentEnable = true;

    [HideInInspector] public GameObject hitEffect;

    [HideInInspector] public string bullet;

    [HideInInspector] public float shootRange;

    [HideInInspector] public float timeBtwShots;
    private float timeBtwShotsValue;

    [HideInInspector] public GameObject shootPos;
    private Projectile projectile;

    [HideInInspector] public float rotOffset;

    [HideInInspector]
    public Vector2 knockbackForce;

    private void Start()
    {
        movement = GetComponent<EnemiesMovement>();

        sr = GetComponentInChildren<SpriteRenderer>();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
        }

        matDefault = sr.material;

        timeBtwShotsValue = timeBtwShots;

        numberOfEnemiesAlive += 1;

        spawnObject = Instantiate(spawnEffect, transform.position, Quaternion.identity);

        Destroy(spawnObject, 1);

        foreach (var component in GetComponents<Behaviour>())
        {
            if (component == this)
            {
                continue;
            }

            component.enabled = false;
        }

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        sr.enabled = false;

        GetComponent<Rigidbody2D>().gravityScale = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnObject != null)
        {
            return;
        }

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

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        if (health <= 0)
        {
            Death();
        }

        if (enemyType == EnemyType.Turret)
        {
            Vector2 difference = -transform.position + player.transform.position;
            float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
            TurretAttack(rotationZ + rotOffset);
        }
        else if (enemyType == EnemyType.Jelly)
        {
            JellyAttack();
        }
    }

    void Death()
    {
        GameObject explosion = explosionParitcle;
        Instantiate(explosion, transform.position, Quaternion.identity);
        numberOfEnemiesAlive -= 1;
        Destroy(gameObject);
    }

    public void Hurt(int _damage)
    {
        health -= _damage;
        sr.material = matWhite;
        Invoke("ResetMaterial", .1f);
        AudioManager.instance.Play("GetHit");
        movement.KnockBack(knockbackForce);
    }

    public void ResetMaterial()
    {
        sr.material = matDefault;
    }

    void TurretAttack(float rotZ)
    {
        LayerMask ignoreLayermask = LayerMask.GetMask("Player");
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, (player.transform.position - transform.position), shootRange, ignoreLayermask);

        Debug.DrawRay(transform.position, (player.transform.position - transform.position).normalized * shootRange, Color.blue);

        if (Vector3.Distance(player.transform.position, transform.position) <= shootRange)
        {
            transform.rotation = Quaternion.Euler(0, 0, rotZ);

            if (hitInfo) Debug.Log(hitInfo.collider.name);

            if (hitInfo && hitInfo.collider.CompareTag("Player"))
            {
                Shoot("TurretShoot", "TurretBullet", transform.rotation);
            }
        }
    }

    void JellyAttack()
    {
        if ((player.transform.position - transform.position).sqrMagnitude <= movement.attackRange * movement.attackRange && !touchingWall)
        {
            Vector2 difference = -transform.position + player.transform.position;
            float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
            Shoot("TurretShoot", bullet, Quaternion.Euler(0, 0, rotationZ));
        }
    }

    void Shoot(string _soundToPlay, string _bullet, Quaternion _rotation)
    {
        if (Time.time >= timeBtwShotsValue)
        {
            AudioManager.instance.Play(_soundToPlay);

            projectile = ObjectPooler.instance.SpawnFromPool<Projectile>(_bullet, shootPos.transform.position, _rotation);

            projectile.isEnemy = true;

            projectile.hitEffect = hitEffect;

            projectile.damage = damage;

            timeBtwShotsValue = timeBtwShots + Time.time;
        }
    }

    void MaggotAttack(Collider2D collision)
    {
        player = collision.gameObject.GetComponent<Player>();
        player.Hurt(damage);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            MaggotAttack(collision);
        }

        if (collision.CompareTag("Ground"))
        {
            touchingWall = true;
        }
        else
        {
            touchingWall = false;
        }
    }
}
