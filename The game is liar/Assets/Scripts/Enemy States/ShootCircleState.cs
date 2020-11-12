using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Enemy States/Shoot Circle State")]
public class ShootCircleState : EnemyState
{
    public int numberOfBullets;
    public int radius;
    public float delayBulletTime;
    public float timeBtwShots;
    float timeBtwShotsValue;

    public override void Init(Enemies enemy)
    {
        timeBtwShotsValue = timeBtwShots;
    }

    public override EnemyState UpdateState(Enemies enemy)
    {
        if (Time.time > timeBtwShotsValue)
        {
            enemy.StartCoroutine(Shoot(enemy));
            timeBtwShotsValue = Time.time + timeBtwShots;
        }
        return null;
    }

    IEnumerator Shoot(Enemies enemy)
    {
        Projectile[] bullets = new Projectile[numberOfBullets];
        int i = 0;
        float normalSpeed = 0;
        foreach (Vector2 pos in MathUtils.GeneratePointsOnCircle(enemy.transform.position, numberOfBullets, radius))
        {
            bullets[i] = ObjectPooler.instance.SpawnFromPool<Projectile>("RedBullet", pos, Quaternion.identity);
            bullets[i].Init(enemy.damage, 0, 0, true, false);
            normalSpeed = bullets[i].speed;
            bullets[i].SetVelocity(0);
            i++;
        }

        yield return new WaitForSeconds(delayBulletTime);

        Vector2 dir = Player.player.transform.position - enemy.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        foreach (var bullet in bullets)
        {
            bullet.speed = normalSpeed;
            bullet.SetVelocity(normalSpeed, angle);
        }

        yield return new WaitForSeconds(timeBtwShots / 2);
    }
}
