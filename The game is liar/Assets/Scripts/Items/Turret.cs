using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : ElementalItem
{
    public Transform shootPos;
    public float timeBtwShots;
    private float timeBtwShotsValue;
    bool isGrounded;

    protected override void Use()
    {
        Throw();
    }

    void FixedUpdate()
    {
        if (GroundCheck(0.018f) && !isGrounded)
        {
            isGrounded = true;
            ToStaticObject();
            Destroy(gameObject, duration);
        }
    }

    void Update()
    {
        if (isGrounded)
        {
            Vector2 size = new Vector2(.01f, .3f);
            Vector2 offset = new Vector2(sr.bounds.extents.x * transform.right.x, 0.2f);
            RaycastHit2D hitInfo = Physics2D.Raycast((Vector2)transform.position + offset, transform.right, range, LayerMask.GetMask("Enemy"));
            ExtDebug.DrawBoxCastBox((Vector2)transform.position + offset, size/2, Quaternion.identity, transform.right, range, Color.red);
            Shoot(hitInfo);
        }
    }

    void Shoot(bool canShoot)
    {
        if (Time.time > timeBtwShotsValue)
        {
            if (canShoot)
            {
                Projectile bullet = ObjectPooler.instance.SpawnFromPool<Projectile>("TurretBullet", shootPos.position, Quaternion.identity);
                bullet.Init(damage, 0, 0, new State(StatusType.Bleed, duration, damage, state.timeBtwHits));
            }
            timeBtwShotsValue = Time.time + timeBtwShots;
        }
    }
}
