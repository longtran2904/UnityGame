using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Action/Shoot")]
public class ShootAction : EnemyAction
{
    public override void Act(Enemy enemy)
    {
        AudioManager.instance.PlaySfx(enemy.weapon.stat.sfx);
        Projectile projectile = ObjectPooler.instance.SpawnFromPool<Projectile>(enemy.weapon.stat.projectile, enemy.weapon.shootPos.position, Quaternion.Euler(0, 0, enemy.CaculateRotationToPlayer()));
        projectile.Init(enemy.damage, 0, 0, true, false);
    }
}