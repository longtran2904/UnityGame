using UnityEngine;

[CreateAssetMenu(menuName = "Enemy States/Shoot State")]
public class ShootState : EnemyState
{
    public string bulletName;
    public string bulletSound;
    public float timeBtwShots;
    private float timeBtwShotsValue;

    public override void Init(Enemies enemy)
    {
    }

    public override EnemyState UpdateState(Enemies enemy)
    {
        if ((Player.player.transform.position - enemy.transform.position).sqrMagnitude <= enemy.attackRange * enemy.attackRange)
        {
            if (Time.time >= timeBtwShotsValue)
            {
                AudioManager.instance.Play(bulletSound);
                Projectile projectile = ObjectPooler.instance.SpawnFromPool<Projectile>(bulletName, enemy.shootPos, Quaternion.Euler(0, 0, enemy.CaculateRotationToPlayer()));
                projectile.Init(enemy.damage, 0, 0, true, false);
                timeBtwShotsValue = timeBtwShots + Time.time;
            }
        }
        return null;
    }
}
