using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : ElementalItem
{
    [SerializeField] protected float timeBtwDamage;
    private float timeBtwDamageValue;
    bool hasTouchedGround;

    protected override void Use()
    {
        Throw();
    }

    void FixedUpdate()
    {
        if (GroundCheck() && !hasTouchedGround)
        {
            ToStaticObject();
            hasTouchedGround = true;
            Destroy(gameObject, duration);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            AddStateToEnemy(collision.GetComponent<Enemy>(), state);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && Time.time >= timeBtwDamageValue)
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            enemy.Hurt(damage);
            timeBtwDamageValue = Time.time + timeBtwDamage;
        }
    }
}
