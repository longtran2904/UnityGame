using System.Collections;
using UnityEngine;

public partial class Enemy : MonoBehaviour
{
#if false
    void InitAbility()
    {
        if (shootAbility.enabled)
        {
            shootCooldown = Time.time + shootAbility.value.cooldown;
            if (shootAbility.value.shootPattern == BulletPattern.Gun)
                CreateWeapon(ref shootAbility.value.gunData.weapon);
            else
            {
                BurstData burst = shootAbility.value.burstData;
                burst.positions = new Vector3[burst.numberOfBullets];
                MathUtils.GenerateCircleOutlineNonAlloc(Vector3.zero, burst.radius, 0, burst.positions);
                burst.rotations = new Quaternion[burst.numberOfBullets];
                for (int i = 0; i < burst.numberOfBullets; i++)
                {
                    burst.rotations[i] = Quaternion.LookRotation(burst.positions[i].normalized, Vector3.up);
                }
            }
        }
        if (teleportAbility.enabled)
            teleportAbility.value.trail = GetComponent<TrailRenderer>();
    }

    void CreateWeapon(ref Weapon weapon)
    {
        weapon = Instantiate(weapon, transform.position, Quaternion.identity);
        weapon.transform.parent = transform;
        weapon.transform.localPosition = weapon.posOffset;
        // NOTE: The enemy has the exact gun prefab like the player so need to remove unnecessary component
        //       This is just for temporary. Should I make different guns for enemy?
        Destroy(weapon.GetComponent<ActiveReload>());
    }

    private IEnumerator Shoot(ShootAbility ability)
    {
        state = EnemyState.Charge;
        float stopTime = (ability.gunData.timeBtwTurn + 1/ability.gunData.fireRate * ability.gunData.numberOfBulletsEachTurn) * ability.gunData.numberOfShootTurn;
        if (ability.shootPattern == BulletPattern.Gun)
        {
            StartCoroutine(ShootWithGun(ability.gunData));
        }
        else
        {
            // TODO: Calculate stop time
            StartCoroutine(BurstProjectiles(ability.burstData));
        }
        yield return new WaitForSeconds(stopTime);
        shootCooldown = Time.time + ability.cooldown;
        state = EnemyState.Normal;
    }

    private IEnumerator ShootWithGun(GunData gunData)
    {
        int numberOfBullets = gunData.numberOfBulletsEachTurn;
        while (gunData.numberOfShootTurn > 0)
        {
            while (numberOfBullets > 0)
            {
                audioManager.PlaySfx(gunData.sfx);
                Projectile projectile = pooler.SpawnFromPool<Projectile>(gunData.projectile,
                    gunData.weapon.shootPos.position, Quaternion.Euler(0, 0, CaculateRotationToPlayer()));
                projectile.Init(gunData.damage, 0, 0, true, false);
                numberOfBullets--;
                yield return new WaitForSeconds(1 / gunData.fireRate);
            }
            numberOfBullets = gunData.numberOfBulletsEachTurn;
            gunData.numberOfShootTurn--;
            yield return new WaitForSeconds(gunData.timeBtwTurn);
        }
    }

    private IEnumerator BurstProjectiles(BurstData burst)
    {
        audioManager.PlaySfx(burst.sfx);
        WaitForSeconds timeBtwWaves = new WaitForSeconds(burst.timeBtwWaves);
        for (int n = 0; n < burst.waves; n++)
        {
            for (int i = 0; i < burst.numberOfBullets; i++)
            {
                Projectile bullet = pooler.SpawnFromPool<Projectile>(burst.projectile, burst.positions[i] + transform.position, burst.rotations[i]);
                bullet.Init(burst.damage, 0, 0, true, false);
                bullet.SetVelocity(0);
            }
            yield return timeBtwWaves;
        }
    }
#endif
}