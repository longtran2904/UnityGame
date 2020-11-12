using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Item : MonoBehaviour
{
    public string itemName;
    public Sprite icon;
    public string description;
    public float cooldownTime;

    [SerializeField] protected float duration;
    [SerializeField] protected int damage;
    [SerializeField] protected float range;
    [SerializeField] protected float throwForce;

    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Animator anim;
    protected Collider2D cd;

    protected abstract void Use(); // Only call this in ItemManager

    public virtual void Init()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        cd = GetComponent<Collider2D>();

        Use();
    }

    protected void Throw(float gravityScale = 6)
    {
        rb.gravityScale = gravityScale;
        rb.velocity = (Input.mousePosition - Player.player.transform.position).normalized * throwForce;
    }

    // Check only the bottom part of the object
    protected bool GroundCheck(float sizeY = 0.01f)
    {
        Vector2 size = new Vector2(0.1f, sizeY);
        Vector3 offset = new Vector2(0, sr.bounds.extents.y + size.y);
        ExtDebug.DrawBoxCastBox(transform.position - offset, size, Quaternion.identity, Vector2.down, size.y, Color.red);
        return Physics2D.BoxCast(transform.position - offset, size, 0, Vector2.down, size.y, LayerMask.GetMask("Ground"));
    }

    protected void SpawnVFX(GameObject effect, float lifeTime = 1)
    {
        GameObject explodeObj = Instantiate(effect, transform.position, Quaternion.identity);
        explodeObj.transform.localScale = Vector3.one * range;
        Destroy(explodeObj, lifeTime);
    }

    protected void SpawnVFX(GameObject effect, Vector3 scale, float lifeTime = 1)
    {
        GameObject explodeObj = Instantiate(effect, transform.position, Quaternion.identity);
        explodeObj.transform.localScale = scale * range;
        Destroy(explodeObj, lifeTime);
    }

    protected Enemies[] GetAllNearbyEnemies()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, LayerMask.GetMask("Enemy"));
        Enemies[] enemies = new Enemies[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            enemies[i] = colliders[i].GetComponent<Enemies>();
        }
        return enemies;
    }

    protected Enemies GetNearestEnemy(Enemies[] enemies)
    {
        int best = 0;
        float closestRange = Vector2.SqrMagnitude(enemies[0].transform.position - transform.position);
        for (int i = 1; i < enemies.Length; i++)
        {
            float range = Vector2.SqrMagnitude(enemies[i].transform.position - transform.position);
            if (range > closestRange)
            {
                best = i;
                closestRange = range;
            }
        }
        return enemies[best];
    }

    protected (Enemies enemy, float range) GetNearestEnemyAndRange(Enemies[] enemies)
    {
        int best = 0;
        float closestRange = Vector2.SqrMagnitude(enemies[0].transform.position - transform.position);
        for (int i = 1; i < enemies.Length; i++)
        {
            float range = Vector2.SqrMagnitude(enemies[i].transform.position - transform.position);
            if (range > closestRange)
            {
                best = i;
                closestRange = range;
            }
        }
        return (enemies[best], closestRange);
    }

    protected void DamageEnemiesInRange()
    {
        foreach (Enemies enemy in GetAllNearbyEnemies())
        {
            enemy.Hurt(damage, Vector2.zero, 0);
        }
    }

    protected void ToStaticObject()
    {
        rb.bodyType = RigidbodyType2D.Static;
        cd.isTrigger = true;
    }
}
