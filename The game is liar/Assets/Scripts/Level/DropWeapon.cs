using UnityEngine;
using System.Collections;

[System.Serializable]
public struct DropData
{
    public WeaponInventory inventory;
    [MinMax(0, 5)] public RangedInt dropIndex;
    [MinMax(0, 360)] public RangedFloat dropRotation;
    [MinMax(1, 20)] public RangedFloat dropForce;
}

public class DropWeapon : MonoBehaviour
{
    [HideInInspector]
    public Weapon template;

    public float speed;
    public float halfDistance;

    private Rigidbody2D rb;

    private IEnumerator UpAndDown(float halfDistance, float center)
    {
        Vector2 dir = Vector2.up;
        yield return new WaitForSeconds(Random.Range(.2f, 1.5f));
        while (true)
        {
            if (transform.position.y >= center + halfDistance)
                dir = Vector2.down;
            else if (transform.position.y <= center - halfDistance)
                dir = Vector2.up;
            rb.velocity = dir * speed;
            yield return null;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (rb.velocity == Vector2.zero)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            GetComponent<BoxCollider2D>().isTrigger = true;
            TextboxTrigger trigger = GetComponent<TextboxTrigger>();
            trigger.textboxType = TextboxType.WEAPON;
            trigger.hitGroundPos = transform.position + new Vector3(0, halfDistance);
            StartCoroutine(UpAndDown(halfDistance, transform.position.y + halfDistance));
        }    
    }
    public void Drop(DropData data, Vector3 dropPos)
    {
        Drop(data.inventory.items[data.dropIndex.randomValue], dropPos, MathUtils.MakeVector2(data.dropRotation.randomValue, data.dropForce.randomValue));
    }

    public void Drop(Weapon template, Vector3 dropPos, Vector2 dropDir)
    {
        DropWeapon drop = Instantiate(this, dropPos, Quaternion.identity);
        drop.template = template;
        drop.GetComponent<SpriteRenderer>().sprite = template.GetComponent<SpriteRenderer>().sprite;
        drop.rb = drop.GetComponent<Rigidbody2D>();
        drop.rb.velocity = dropDir;
        drop.gameObject.AddComponent<BoxCollider2D>();
    }
}
