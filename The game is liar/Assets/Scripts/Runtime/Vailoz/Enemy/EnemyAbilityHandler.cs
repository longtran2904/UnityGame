using System.Collections;
using UnityEngine;

public partial class Enemy : MonoBehaviour
{
    void InitMovement()
    {
        switch (moveType)
        {
            case MoveType.Jump:
                {
                    jumpVelocity = MathUtils.MakeVector2(jump.jumpAngle, jump.jumpForce);
                    cliffCheck.enabled = false;
                    isInWall.enabled = false;
                } break;
            case MoveType.Run:
                {
                    if (run.target == TargetType.Random)
                        targetDir = MathUtils.RandomBool() ? Vector2.left : Vector2.right;
                    isInWall.enabled = false;
                } break;
            case MoveType.Fly:
                {
                    if (moveType == MoveType.Fly)
                    {
                        groundCheck.enabled = false;
                        cliffCheck.enabled = false;
                        wallCheck.enabled = false;
                    }
                } break;
        }
    }

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

    // TODO: Better jump
    // TODO: Animation handling
    void Move()
    {
        switch (moveState)
        {
            case MoveState.Wait:
                {
                    if (moveType == MoveType.Jump)
                    {
                        if (wallCheck)
                        {
                            targetDir.x *= -1;
                            jumpVelocity.x *= -1;
                            rb.velocity = new Vector2(rb.velocity.x * -1, rb.velocity.y);
                        }
                    }
                    timer += Time.deltaTime;
                    if (timer >= waitTime)
                    {
                        moveState = MoveState.Move;
                        timer = 0;
                    }
                    return;
                }
        }

        switch (moveType)
        {
            case MoveType.Run:
                {
                    switch (run.target)
                    {
                        case TargetType.Random:
                            {
                                if (cliffCheck || wallCheck)
                                {
                                    targetDir *= -1;
                                }
                            }
                            break;
                        case TargetType.Player:
                            {
                                targetDir = player.transform.position.x > transform.position.x ? Vector2.right : Vector2.left;
                                if (cliffCheck && transform.right.x == Mathf.Sign(player.transform.position.x - transform.position.x))
                                {
                                    targetDir = Vector2.zero;
                                }
                            }
                            break;
                    }
                    rb.velocity = targetDir * run.runSpeed;
                } break;
            case MoveType.Jump:
                {
                    if (groundCheck)
                    {
                        rb.velocity = jumpVelocity;
                        waitTime = jump.timeBtwJumps;
                        moveState = MoveState.Wait;
                    }
                } break;
            case MoveType.Fly:
                {
                    switch (fly.pattern)
                    {
                        case FlyPattern.Linear:
                            {
                                targetDir = (player.transform.position - transform.position).normalized;
                                rb.velocity = targetDir * fly.flySpeed;
                            } break;
                        case FlyPattern.Curve:
                            {
                                InitFlyCurve(player.transform.position); // TODO: add some timer rather then init every frame
                                Vector2 dir = (Vector3)MathUtils.CubicCurve(Time.deltaTime, fly.start, fly.curvePoint1, fly.curvePoint2, fly.end) - transform.position;
                                rb.velocity = dir.normalized * fly.flySpeed;
                            } break;
                    }
                } break;
        }

        void InitFlyCurve(Vector2 endPos)
        {
            fly.end = endPos;
            fly.start = transform.position;
            float halfDistance = (fly.end - fly.start).magnitude / 2;
            Vector2 offset = new Vector2(fly.aX.randomValue, fly.aY.randomValue * fly.yMutiplier.randomValue) * halfDistance;
            fly.curvePoint1 = fly.start + offset;
            offset = new Vector2(fly.bX.randomValue, fly.bY.randomValue * fly.yMutiplier.randomValue) * halfDistance;
            fly.curvePoint2 = fly.end - offset;
        }
    }

    void UseAbility()
    {
        if (chargeAttack.enabled && IsInRange(chargeAttack.value.distanceToCharge))
        {
            StartCoroutine(ChargeAttack(chargeAttack));
        }
        else if (explodeAbility.enabled && explodeAbility.value.activationType == ActivationType.InRange && IsInRange(explodeAbility.value.distanceToExplode) && !isInWall)
        {
            StartCoroutine(StartExploding(explodeAbility));
        }
        else if (teleportAbility.enabled
            && ((teleportAbility.value.distanceToTeleportX.enabled && !InRangeX(teleportAbility.value.distanceToTeleportX)) // canTeleportX
            || (teleportAbility.value.distanceToTeleportY.enabled && !InRangeY(teleportAbility.value.distanceToTeleportY))) // canTeleportY
            && player.controller.isGrounded)
        {
            StartCoroutine(Teleport(teleportAbility));
        }
        else if (shootAbility.enabled && Time.time >= shootCooldown)
        {
            StartCoroutine(Shoot(shootAbility));
        }
    }

    private IEnumerator Shoot(ShootAbility ability)
    {
        state = EnemyState.Stop;
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

    // TODO: Maybe switch to teleport to a tile from a tiles array
    IEnumerator Teleport(TeleportAbility ability)
    {
        Vector3 destination = player.transform.position;
        destination.y += (sr.bounds.extents.y - playerSr.bounds.extents.y) * player.transform.up.y;

        int dirX = (int)Mathf.Sign(transform.position.x - player.transform.position.x);
        float randomDistance = ability.distanceToPlayer.randomValue;
        float minDistance = ability.distanceToPlayer.min;
        
        bool switchDir = false;
        bool done = false;
        goto CheckPos;

        NextPos:
        done = true;
        CheckPos:
        Vector3 offset = new Vector3(randomDistance * dirX, 0);
        while (!IsPositionValid(destination + offset, Color.yellow, Color.green))
        {
            if (switchDir)
            {
                if (done) // The enemy can't find a valid position
                {
                    rb.velocity = Vector2.zero;
                    yield break;
                }

                dirX = -dirX;
                switchDir = false;
                goto NextPos;
            }

            offset.x = minDistance * dirX;
            switchDir = true;
        }

        if (ability.flipVertically && player.transform.up.y != transform.up.y)
        {
            transform.Rotate(new Vector3(180, 0, 0), Space.World);
        }
        ability.trail.enabled = true;
        transform.position = destination + offset;
        yield return null;
        ability.trail.enabled = false;

        bool IsPositionValid(Vector3 pos, Color ground, Color wall)
        {
            ExtDebug.DrawBox(pos - new Vector3(0, sr.bounds.extents.y * player.transform.up.y), new Vector2(sr.bounds.extents.x, .1f), Quaternion.identity, ground);
            ExtDebug.DrawBox(pos + new Vector3(0, .1f * player.transform.up.y), sr.bounds.extents, Quaternion.identity, wall);
            return Physics2D.BoxCast(pos - new Vector3(0, sr.bounds.extents.y * player.transform.up.y), new Vector2(sr.bounds.size.x, .1f), 0, Vector2.zero, 0, LayerMask.GetMask("Ground"))
                && !Physics2D.BoxCast(pos + new Vector3(0, .1f * player.transform.up.y), sr.bounds.size, 0, Vector2.zero, 0, LayerMask.GetMask("Ground"));
        }
    }

    /*
     * TODO:
     * 1. Update the player's position when dash or close to dash
     * 2. Stop charging when the player move away before some threshold
     */
    IEnumerator ChargeAttack(ChargeAttack charge)
    {
        state = EnemyState.Stop;
        rb.velocity = Vector2.zero;
        StartCoroutine(Flashing(charge.flashData, charge.chargeTime));
        yield return new WaitForSeconds(charge.chargeTime);
        rb.velocity = targetDir * charge.dashSpeed;
        yield return new WaitForSeconds(charge.dashTime);
        rb.velocity = Vector2.zero;
        state = EnemyState.Normal;
    }


    IEnumerator StartExploding(ExplodeAbility explodesion)
    {
        state = EnemyState.Invincible;
        rb.velocity = Vector2.zero;
        StartCoroutine(Flashing(explodesion.flashData, explodesion.explodeTime));
        yield return new WaitForSeconds(explodesion.explodeTime);
        Explode(explodesion);
        Die();
    }

    void Explode(ExplodeAbility explodesion)
    {
        audioManager.PlaySfx(explodesion.explodeSound);
        EZCameraShake.CameraShaker.Instance.ShakeOnce(8, 5, 0.1f, 0.5f);
        GameObject explodeVFX = Instantiate(explodesion.explodeParticle, transform.position, Quaternion.identity);
        explodeVFX.transform.localScale = Vector3.one * explodesion.explodeRange;
        Destroy(explodeVFX, .3f);
        if ((player.transform.position - transform.position).sqrMagnitude < explodesion.explodeRange * explodesion.explodeRange)
            player.Hurt(explodesion.explodeDamage);
    }

    IEnumerator Flashing(FlashAbility flash, float duration)
    {
        Material triggerMat = new Material(flash.triggerMat);
        triggerMat.color = flash.color;
        while (duration > 0)
        {
            float currentTime = Time.time;

            sr.material = triggerMat;
            yield return new WaitForSeconds(flash.timeBtwFlashes);

            sr.material = defMat;
            yield return new WaitForSeconds(flash.timeBtwFlashes);

            duration -= Time.time - currentTime;
        }
    }

    void Split(GameObject splitEnemy)
    {
        Vector3 offset = new Vector3(1f, 0, 0);
        Instantiate(splitEnemy, transform.position + offset, Quaternion.identity);
        Instantiate(splitEnemy, transform.position - offset, Quaternion.identity);
    }
}