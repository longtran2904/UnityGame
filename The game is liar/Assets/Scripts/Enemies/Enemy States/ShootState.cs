using UnityEngine;
/*
[CreateAssetMenu(menuName = "Enemy/States/Shoot")]
public class ShootState : EnemyState
{
    private float timeBtwShots;

    public override void Init(Enemies enemy)
    {
        timeBtwShots = Time.time + 1 / enemy.weapon.stat.fireRate;
        base.Init(enemy);
    }

    public override EnemyState UpdateState(Enemies enemy)
    {
        if (Time.time >= timeBtwShots)
        {
            AudioManager.instance.PlaySfx(enemy.weapon.stat.sfx);
            Projectile projectile = ObjectPooler.instance.SpawnFromPool<Projectile>(enemy.weapon.stat.projectile, enemy.weapon.shootPos.position, Quaternion.Euler(0, 0, enemy.CaculateRotationToPlayer()));
            projectile.Init(enemy.damage, 0, 0, true, false);
            timeBtwShots = Time.time + 1 / enemy.weapon.stat.fireRate;
        }
        return base.UpdateState(enemy);
    }
}
*/

public class ShootAction : EnemyAction
{
    public override void Act(Enemy enemy)
    {
        AudioManager.instance.PlaySfx(enemy.weapon.stat.sfx);
        Projectile projectile = ObjectPooler.instance.SpawnFromPool<Projectile>(enemy.weapon.stat.projectile, enemy.weapon.shootPos.position, Quaternion.Euler(0, 0, enemy.CaculateRotationToPlayer()));
        projectile.Init(enemy.damage, 0, 0, true, false);
    }
}