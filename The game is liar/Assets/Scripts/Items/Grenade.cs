using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : Item
{
    public int damage;
    public float range;
    public Sprite sprite;
    public GameObject explodeEffect;
    public PhysicsMaterial2D mat;

    public float throwForce;

    public List<GrenadeEffect> effects;

    protected GameObject grenadeObject;

    public virtual void Explode()
    {        
        foreach (var effect in effects)
        {
            effect.Explode(this, grenadeObject.transform.position);
        }
        ExplodeVFX();
        Destroy(grenadeObject);
    }

    public virtual void Throw(Vector2 startingPos, Vector2 dir)
    {
        CreateGrenade(startingPos);
        Rigidbody2D rb = grenadeObject.GetComponent<Rigidbody2D>();
        rb.gravityScale = 6;
        rb.velocity = dir * throwForce;
    }

    protected virtual void CreateGrenade(Vector2 startingPos)
    {
        grenadeObject = new GameObject(itemName, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D));
        grenadeObject.GetComponent<SpriteRenderer>().sprite = sprite;
        Vector2 offset = new Vector3(.5f, 0);
        grenadeObject.transform.position = startingPos + offset;
        grenadeObject.transform.localScale = new Vector3(3, 3);
        grenadeObject.GetComponent<BoxCollider2D>().size = new Vector2(0.12f, 0.12f);
        grenadeObject.GetComponent<BoxCollider2D>().sharedMaterial = mat;
        grenadeObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        grenadeObject.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    protected virtual void ExplodeVFX()
    {
        GameObject explodeObj = Instantiate(explodeEffect, grenadeObject.transform.position, Quaternion.identity);
        explodeObj.transform.localScale = new Vector3(6.25f, 6.25f) * range;
        Destroy(explodeObj, .25f);
    }
}
