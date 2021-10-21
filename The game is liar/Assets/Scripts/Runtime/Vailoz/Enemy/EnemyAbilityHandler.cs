using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * NOTE: The enemy system consists of two things: Move data & Ability data
 * 
 * Here's how the system update every frame:
 * 1. Check if health <= 0, then execute any abilities that can be executed and then die
 * 2. Rotate the enemy if need to
 * 3. Move according to the move data
 * 4. Use ability according to the ability data
 * 
 * Here's how the move function work:
 * 1. Set up the correct direction
 * 2. Move with the correct type/pattern
 * 
 * Here's how the use ability function work:
 * 1. Loop through all the abilities each frame until can execute one ability
 * 2. If it's a one frame ability, execute it and go back to step 1
 * 3. If it's a multiple frames ability, start the coroutine and wait until it's finished, then go back to step 1
 */

public partial class Enemy : MonoBehaviour
{
    void InitMovement()
    {
        switch (moveType)
        {
            case MoveType.Jump:
                {
                    jumpVelocity = MathUtils.MakeVector2(jump.jumpAngle, jump.jumpForce);
                }
                break;
        }
    }

    void InitAbility()
    {
        if (shootAbility.enabled)
        {
            if (shootAbility.value.shootPattern == BulletPattern.Gun)
                CreateWeapon(ref shootAbility.value.gunData.weapon);
            else
            {
                shootAbility.value.patternData.bullets = new Projectile[shootAbility.value.patternData.numberOfBullets];
                shootAbility.value.patternData.bulletsPos = new Vector2[shootAbility.value.patternData.numberOfBullets];
            }
        }
        if (teleportAbility.enabled)
            teleportAbility.value.trail = GetComponent<TrailRenderer>();
        if (jumpAttack.enabled)
        {
            jumpAttackVelocity = MathUtils.MakeVector2(jumpAttack.value.jumpData.jumpAngle, jumpAttack.value.jumpData.jumpForce);
        }
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

    void Move()
    {
        switch (state)
        {
            case MoveState.Move:
                {
                    if (canStop)
                    {
                        if (timer >= moveTime)
                        {
                            rb.velocity = Vector2.zero;
                            state = MoveState.Wait;
                            timer = 0;
                            return;
                        }
                        timer += Time.deltaTime;
                    }
                } break;
            case MoveState.Wait:
                {
                    if (timer >= waitTime)
                    {
                        state = MoveState.Move;
                        timer = 0;
                        haveUsedAbility = false;
                        return;
                    }
                    timer += Time.deltaTime;
                    return;
                }
            case MoveState.Stop:
                return;
        }

        switch (targetType)
        {
            case TargetType.Random:
                {
                    if (state == MoveState.Move && timer == 0)
                    {
                        if (moveType != MoveType.Fly)
                        {
                            targetDir = MathUtils.RandomBool() ? Vector2.right : Vector2.left;
                            InternalDebug.Log("Target Direction: " + targetDir);
                        }
                        else
                        {
                            if (fly.type == FlyPattern.Linear)
                            {
                                targetDir = MathUtils.RandomVector2().normalized;
                            }
                            else
                            {
                                InitFlyCurve(Vector2.zero); // TODO: Need some ways to get a random position in a room
                            }
                        }
                    }
                }
                break;
            case TargetType.Player:
                {
                    if (moveType != MoveType.Fly)
                    {
                        targetDir = player.transform.position.x > transform.position.x ? Vector2.right : Vector2.left;
                        if (onPlatform && cliffCheck)
                        {
                            if (transform.right.x == Mathf.Sign(player.transform.position.x - transform.position.x))
                                targetDir = Vector2.zero;
                        }
                    }
                    else
                    {
                        if (fly.type == FlyPattern.Linear)
                            targetDir = (player.transform.position - transform.position).normalized;
                        else
                            InitFlyCurve(player.transform.position);
                    }
                } break;
        }

        if (targetType != TargetType.Player && moveType != MoveType.Fly)
        {
            if ((onPlatform && cliffCheck) || wallCheck)
            {
                targetDir *= -1;
            }
        }

        switch (moveType)
        {
            case MoveType.Run:
                {
                    rb.velocity = targetDir * run.runSpeed;
                } break;
            case MoveType.Jump:
                {
                    rb.velocity = jumpVelocity * targetDir.x;
                } break;
            case MoveType.Fly:
                {
                    switch (fly.type)
                    {
                        case FlyPattern.Linear:
                            {
                                rb.velocity = targetDir * fly.flySpeed;
                            } break;
                        case FlyPattern.Quadratic:
                            {
                                Vector2 dir = (Vector3)MathUtils.QuadraticCurve(Time.deltaTime, fly.start, fly.curvePoint, fly.end) - transform.position;
                                rb.velocity = dir.normalized * fly.flySpeed; 
                            } break;
                        case FlyPattern.Cubic:
                            {
                                Vector2 dir = (Vector3)MathUtils.CubicCurve(Time.deltaTime, fly.start, fly.curvePoint, fly.secondCurvePoint, fly.end) - transform.position;
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
            fly.curvePoint = fly.start + offset;
            if (fly.type == FlyPattern.Cubic)
            {
                offset = new Vector2(fly.bX.randomValue, fly.bY.randomValue * fly.yMutiplier.randomValue) * halfDistance;
                fly.secondCurvePoint = fly.end - offset;
            }
        }
    }

    void UseAbility()
    {
        if (currentAbility != MultipleFramesAbility.None || (onlyUseAbilityWhenWait && (state != MoveState.Wait || (state == MoveState.Wait && haveUsedAbility))))
        {
            return;
        }

        if (chargeAttack.enabled && IsInRange(chargeAttack.value.distanceToCharge))
        {
            StartAbility(MultipleFramesAbility.ChargeAttack);
            StartCoroutine(ChargeAttack(chargeAttack));
        }
        else if (jumpAttack.enabled && IsInRange(jumpAttack.value.distanceToJump))
        {
            StartAbility(MultipleFramesAbility.JumpAttack);
            StartCoroutine(JumpAttack(jumpAttack));
        }
        else if (explodeAbility.enabled && explodeAbility.value.activationType == ActivationType.InRange && IsInRange(explodeAbility.value.distanceToExplode))
        {
            StartAbility(MultipleFramesAbility.Explode);
            StartCoroutine(StartExploding(explodeAbility));
        }
        else if (teleportAbility.enabled
            && ((teleportAbility.value.DistanceToTeleportX.enabled && !InRangeX(teleportAbility.value.DistanceToTeleportX))
            || (teleportAbility.value.DistanceToTeleportY.enabled && !InRangeY(teleportAbility.value.DistanceToTeleportY)))
            && !player.controller.isJumping)
        {
            //bool canTeleportX = teleportAbility.value.DistanceToTeleportX.enabled && !InRangeX(teleportAbility.value.DistanceToTeleportX);
            //bool canTeleportY = teleportAbility.value.DistanceToTeleportY.enabled && !InRangeY(teleportAbility.value.DistanceToTeleportY);
            //if ((canTeleportX || canTeleportY) && !player.controller.isJumping)
                StartCoroutine(Teleport(teleportAbility));
        }
        else if (shootAbility.enabled)
        {
            StartAbility(MultipleFramesAbility.Shoot);
            Shoot(shootAbility);
        }

        void StartAbility(MultipleFramesAbility ability)
        {
            currentAbility = ability;
            haveUsedAbility = true;
        }
    }

    IEnumerator JumpAttack(JumpAttack jump)
    {
        rb.velocity = jumpAttackVelocity * targetDir.x;
        collidePlayer += () => player.Hurt(jump.jumpDamage);
        while (!groundCheck)
        {
            yield return null;
        }
        currentAbility = MultipleFramesAbility.None;
    }

    private void Shoot(ShootAbility ability)
    {
        if (ability.shootPattern == BulletPattern.Gun)
        {
            StartCoroutine(ShootWithGun(ability));
        }
        else
        {
            shootAbility.value.patternData.bulletHolder = Instantiate(shootAbility.value.patternData.bulletHolder, transform.position, Quaternion.identity);
            switch (ability.shootPattern)
            {
                case BulletPattern.Circle:
                    MathUtils.GenerateCircleOutlineNonAlloc(transform.position, ability.patternData.radius, 0, ability.patternData.bulletsPos);
                    break;
            }
            ability.patternData.bulletHolder.StartCoroutine(ShootShape(ability.gunData, ability.patternData, ability.patternData.bulletsPos));
        }
    }

    private IEnumerator ShootWithGun(ShootAbility shooter)
    {
        int numberOfBullets = shooter.gunData.numberOfBulletsEachTurn;
        while (shooter.gunData.numberOfShootTurn > 0)
        {
            while (numberOfBullets > 0)
            {
                AudioManager.instance.PlaySfx(shooter.gunData.sfx);
                Projectile projectile = ObjectPooler.instance.SpawnFromPool<Projectile>(shooter.gunData.projectile,
                    shooter.gunData.weapon.shootPos.position, Quaternion.Euler(0, 0, CaculateRotationToPlayer()));
                projectile.Init(shooter.gunData.damage, 0, 0, true, false);
                numberOfBullets--;
                yield return new WaitForSeconds(1 / shooter.gunData.fireRate);
            }
            numberOfBullets = shooter.gunData.numberOfBulletsEachTurn;
            shooter.gunData.numberOfShootTurn--;
            yield return new WaitForSeconds(shooter.gunData.timeBtwTurn);
        }
        currentAbility = MultipleFramesAbility.None;
    }

    private IEnumerator ShootShape(GunData bullet, ShootPatternData patternData, Vector2[] bulletPos)
    {
        int i = 0;
        foreach (Vector2 pos in bulletPos)
        {
            patternData.bullets[i] = ObjectPooler.instance.SpawnFromPool<Projectile>(bullet.projectile, pos, Quaternion.identity, patternData.bulletHolder.transform);
            patternData.bullets[i].Init(bullet.damage, 0, 0, true, false);
            patternData.bullets[i].SetVelocity(0);
            i++;
        }

        AudioManager.instance.PlaySfx(bullet.sfx);
        Vector2 dir = (player.transform.position - transform.position).normalized;
        yield return new WaitForSeconds(patternData.delayBulletTime);

        patternData.bulletHolder.Move(dir, patternData.bullets[0].speed);
        float timeToStop = Time.time + patternData.bullets[0].timer;
        if (patternData.rotate)
            while (Time.time < timeToStop)
            {
                patternData.bulletHolder.transform.Rotate(new Vector3(0, 0, patternData.rotateSpeed * Time.deltaTime * (patternData.clockwise ? 1 : -1)));
                yield return null;
            }
        Destroy(patternData.bulletHolder.gameObject);
        currentAbility = MultipleFramesAbility.None;
    }

    // TODO: Currently, there are situations that the enemy teleport to mid air.
    //       We need to check if it is grounded, if not then move to the opposite side of player, if it still isn't grounded, then just don't teleport and wait=
    IEnumerator Teleport(TeleportAbility ability)
    {
        Vector3 destination = player.transform.position;
        destination.x += Mathf.Sign(transform.position.x - player.transform.position.x) * ability.distanceToPlayer;
        if (ability.flipVertically && player.transform.up.y != transform.up.y)
        {
            transform.Rotate(new Vector3(180, 0, 0), Space.World);
        }
        destination.y += (sr.bounds.extents.y - playerSr.bounds.extents.y) * transform.up.y;

        ability.trail.enabled = true;
        transform.position = destination;
        yield return null;
        ability.trail.enabled = false;
    }

    IEnumerator ChargeAttack(ChargeAttack charge)
    {
        MoveState currentState = state;
        rb.velocity = Vector2.zero;
        state = MoveState.Stop;
        StartCoroutine(Flashing(charge.flashData, charge.chargeTime));
        yield return new WaitForSeconds(charge.chargeTime);
        rb.velocity = targetDir * charge.dashSpeed;
        yield return new WaitForSeconds(charge.dashTime);
        currentAbility = MultipleFramesAbility.None;
        state = currentState;
    }


    IEnumerator StartExploding(ExplodeAbility explodesion)
    {
        rb.velocity = Vector2.zero;
        state = MoveState.Stop;
        StartCoroutine(Flashing(explodesion.flashData, explodesion.explodeTime));
        yield return new WaitForSeconds(explodesion.explodeTime);
        Explode(explodesion);
        Die();
    }

    void Explode(ExplodeAbility explodesion)
    {
        AudioManager.instance.PlaySfx(explodesion.explodeSound);
        EZCameraShake.CameraShaker.Instance.ShakeOnce(8, 5, 0.1f, 0.5f);
        GameObject explodeVFX = Instantiate(explodesion.explodeParticle, transform.position, Quaternion.identity);
        explodeVFX.transform.localScale = new Vector3(6.25f, 6.25f, 0) * explodesion.explodeRange;
        Destroy(explodeVFX, .3f);
        if ((player.transform.position - transform.position).sqrMagnitude < explodesion.explodeRange * explodesion.explodeRange)
            player.Hurt(explodesion.explodeDamage);
    }

    IEnumerator Flashing(FlashAbility flash, float duration)
    {
        flash.triggerMat.color = flash.color;
        while (duration > 0)
        {
            float currentTime = Time.time;

            sr.material = flash.triggerMat;
            yield return new WaitForSeconds(flash.flashTime);

            sr.material = defMat;
            yield return new WaitForSeconds(flash.timeBtwFlashes);

            duration -= Time.time - currentTime;
        }
    }

    void Split(GameObject splitEnemy)
    {
        Vector3 offset = new Vector3(.5f, 0, 0);
        Instantiate(splitEnemy, transform.position + offset, Quaternion.identity);
        Instantiate(splitEnemy, transform.position + offset, Quaternion.identity);
    }
}
