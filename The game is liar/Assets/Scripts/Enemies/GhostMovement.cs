//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class GhostMovement : EnemiesMovement
//{
//    [Range(0, 1)] public float teleportChance;
//    private bool canTeleport;

//    public float minDistance;
//    public float maxDistance;
//    public int numberOfBullets;
//    public int radius;
//    public int maxTry;

//    public float timeBtwShots;
//    private float timeBtwShotsValue;
//    public float delayBulletTime;
//    public Projectile bullet;
//    private Projectile[] bullets;

//    void Update()
//    {
//        if (Time.time > timeBtwShotsValue)
//        {
//            StartCoroutine(Shoot());
//            timeBtwShotsValue = Time.time + timeBtwShots;
//        }
//        else if (canTeleport && Random.value <= teleportChance)
//        {
//            TeleportToRandomPos();
//        }
//    }

//    IEnumerator Shoot()
//    {
//        canTeleport = false;
//        bullets = new Projectile[numberOfBullets];
//        int i = 0;
//        float normalSpeed = 0;
//        foreach (Vector2 pos in MathUtils.GeneratePointsOnCircle(transform.position, numberOfBullets, radius))
//        {
//            bullets[i] = ObjectPooler.instance.SpawnFromPool<Projectile>("RedBullet", pos, Quaternion.identity);
//            bullets[i].Init(enemy.damage, Vector2.zero, null, true, false);
//            normalSpeed = bullets[i].speed;
//            bullets[i].SetVelocity(0);
//            i++;
//        }

//        yield return new WaitForSeconds(delayBulletTime);

//        Vector2 dir = player.transform.position - transform.position;
//        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
//        foreach (var bullet in bullets)
//        {
//            bullet.speed = normalSpeed;
//            bullet.SetVelocity(normalSpeed, angle);
//        }

//        yield return new WaitForSeconds(timeBtwShots / 2);
//        canTeleport = true;
//    }

//    void TeleportToRandomPos()
//    {
//        int numberOfTryLeft = maxTry;
//        Vector2Int newPos = GetRandomPosition();
//        while (RoomManager.instance.allGroundTiles.Contains(newPos) && numberOfTryLeft > 0)
//        {
//            newPos = GetRandomPosition();
//            numberOfTryLeft--;
//        }
//        if (RoomManager.instance.allGroundTiles.Contains(newPos)) // We need this to check if newPos is actually valid or the number of tries left is zero
//            transform.position = (Vector2)newPos;
//    }

//    private Vector2Int GetRandomPosition()
//    {
//        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
//        Vector2 randomDir = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
//        float randomDistance = Random.Range(minDistance, maxDistance);
//        Vector2Int newPos = MathUtils.ToVector2Int((Vector2)transform.position + randomDir * randomDistance);
//        return newPos;
//    }
//}
