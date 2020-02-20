using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Maggot,
    Bat,
    Turret,
    Alien
}

public class Enemies : MonoBehaviour
{

    public int health;
    public int damage;

    private EnemiesMovement movement;
    public EnemyType enemyType;
    private Player player;

    public Material matWhite;
    private Material matDefault;
    private SpriteRenderer sr;

    public GameObject explosionParitcle;

    public GameObject turretBullet;

    public float shootRange;

    public float timeBtwShots;
    private float timeBtwShotsValue;

    public GameObject shootPos;
    private Projectile projectile;

    public float rotOffset;

    private AudioManager audioManager;

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
    }

    void Death()
    {
        GameObject explosion = explosionParitcle;
        Instantiate(explosion, transform.position, Quaternion.identity);
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
        if (Vector3.Distance(player.transform.position, transform.position) <= shootRange)
        {
            transform.rotation = Quaternion.Euler(0, 0, rotZ);
            Shoot();
        }
    }

    void Shoot()
    {
        if (timeBtwShotsValue <= 0)
        {
            audioManager.Play("TurretShoot");

            Instantiate(turretBullet, shootPos.transform.position, transform.rotation);

            projectile = turretBullet.gameObject.GetComponent<Projectile>();

            projectile.damage = damage;

            timeBtwShotsValue = timeBtwShots;
        }
        else
        {
            timeBtwShotsValue -= Time.deltaTime;
        }
    }

    void MaggotAttack(Collider2D collision)
    {

        player = collision.gameObject.GetComponent<Player>();
        player.Hurt(damage);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && enemyType == EnemyType.Maggot)
        {
            MaggotAttack(collision);
        }
    }
}
