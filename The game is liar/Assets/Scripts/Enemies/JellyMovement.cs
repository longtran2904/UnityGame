using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyMovement : EnemiesMovement
{
    public GameObject hitEffect;
    public string bullet;
    public float timeBtwShots;
    private float timeBtwShotsValue;
    public GameObject shootPos;
    private Projectile projectile;
    public float rotOffset;
    bool touchingWall = false;
    bool isPlayerDied;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        timeBtwShotsValue = timeBtwShots;
    }

    protected override void OnPlayerDeathEvent()
    {
        base.OnPlayerDeathEvent();
        isPlayerDied = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerDied) return;
        MoveToPlayer();
        Attack();
    }

    void MoveToPlayer()
    {
        LookAtPlayer();
        if ((player.transform.position - transform.position).sqrMagnitude <= attackRange * attackRange && !touchingWall)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        rb.velocity = (player.transform.position - transform.position).normalized * speed * Time.deltaTime;
    }

    public void LookAtPlayer()
    {
        if (player.transform.position.x <= transform.position.x)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (player.transform.position.x > transform.position.x)
        {
            transform.eulerAngles = Vector3.zero;
        }
    }

    void Attack()
    {
        bool isInRange = (player.transform.position - transform.position).sqrMagnitude <= attackRange * attackRange;
        if (isInRange && !touchingWall)
        {
            Shoot("TurretShoot", bullet, Quaternion.Euler(0, 0, CaculateRotationToPlayer()));
        }
    }

    float CaculateRotationToPlayer()
    {
        Vector2 difference = -transform.position + player.transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        return rotationZ;
    }

    void Shoot(string _soundToPlay, string _bullet, Quaternion _rotation)
    {
        if (Time.time >= timeBtwShotsValue)
        {
            AudioManager.instance.Play(_soundToPlay);
            projectile = ObjectPooler.instance.SpawnFromPool<Projectile>(_bullet, shootPos.transform.position, _rotation);
            projectile.isEnemy = true;
            projectile.hitEffect = hitEffect;
            projectile.damage = enemy.damage;
            timeBtwShotsValue = timeBtwShots + Time.time;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            touchingWall = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            touchingWall = false;
        }
    }
}
