using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Maggot,
    Bat,
    Turret,
    Alien,
    Jelly
}

public class Enemies : MonoBehaviour
{

    public static int numberOfEnemiesAlive = 0;

    public int health;
    public int damage;

    [HideInInspector] public bool touchingWall = false;

    private EnemiesMovement movement;

    private AudioManager audioManager;

    [HideInInspector] public EnemyType enemyType;

    private Player player;

    public Material matWhite;
    private Material matDefault;
    private SpriteRenderer sr;

    public GameObject explosionParitcle;

    [HideInInspector] public GameObject hitEffect;

    [HideInInspector] public Projectile bullet;

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
        audioManager = FindObjectOfType<AudioManager>();
        numberOfEnemiesAlive += 1;
    }

    // Update is called once per frame
    void Update()
    {
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
        audioManager.Play("GetHit");
        movement.KnockBack(knockbackForce);
    }

    public void ResetMaterial()
    {
        sr.material = matDefault;
    }

    void TurretAttack(float rotZ)
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, (player.transform.position - transform.position), shootRange);

        Debug.DrawRay(transform.position, (player.transform.position - transform.position).normalized * shootRange, Color.blue);

        if (Vector3.Distance(player.transform.position, transform.position) <= shootRange)
        {
            transform.rotation = Quaternion.Euler(0, 0, rotZ);

            if (hitInfo && hitInfo.collider.CompareTag("Player"))
            {
                Shoot("TurretShoot", bullet, transform.rotation);
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

    void Shoot(string _soundToPlay, Projectile _bullet, Quaternion _rotation)
    {
        if (Time.time >= timeBtwShotsValue)
        {
            audioManager.Play(_soundToPlay);

            projectile = Instantiate(_bullet, shootPos.transform.position, _rotation) as Projectile;

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
