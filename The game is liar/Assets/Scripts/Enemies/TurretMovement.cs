//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class TurretMovement : EnemiesMovement
//{
//    public GameObject hitEffect;
//    public string bullet;
//    public float timeBtwShots;
//    private float timeBtwShotsValue;
//    public GameObject shootPos;
//    private Projectile projectile;
//    public float rotOffset;
//    bool isPlayerDied;

//    // Start is called before the first frame update
//    protected override void Start()
//    {
//        base.Start();
//        timeBtwShotsValue = timeBtwShots;
//    }

//    protected override void OnPlayerDeathEvent()
//    {
//        base.OnPlayerDeathEvent();
//        isPlayerDied = true;
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (isPlayerDied) return;
//        Attack(CaculateRotationToPlayer() + rotOffset);
//    }

//    void Attack(float rotZ)
//    {
//        LayerMask ignoreLayermask = LayerMask.GetMask("Player");
//        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, (player.transform.position - transform.position), attackRange, ignoreLayermask);
//        InternalDebug.DrawRay(transform.position, (player.transform.position - transform.position).normalized * attackRange, Color.blue);
//        if (Vector3.Distance(player.transform.position, transform.position) <= attackRange)
//        {
//            transform.rotation = Quaternion.Euler(0, 0, rotZ);
//            if (hitInfo && hitInfo.collider.CompareTag("Player"))
//            {
//                Shoot("TurretShoot", "TurretBullet", transform.rotation);
//            }
//        }
//    }

//    float CaculateRotationToPlayer()
//    {
//        Vector2 difference = -transform.position + player.transform.position;
//        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
//        return rotationZ;
//    }

//    void Shoot(string _soundToPlay, string _bullet, Quaternion _rotation)
//    {
//        if (Time.time >= timeBtwShotsValue)
//        {
//            AudioManager.instance.Play(_soundToPlay);
//            projectile = ObjectPooler.instance.SpawnFromPool<Projectile>(_bullet, shootPos.transform.position, _rotation);
//            projectile.Init(enemy.damage, Vector2.zero, hitEffect, true, false);
//            timeBtwShotsValue = timeBtwShots + Time.time;
//        }
//    }
//}
