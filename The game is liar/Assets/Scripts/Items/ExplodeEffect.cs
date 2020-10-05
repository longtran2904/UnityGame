using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item Effects/Explode")]
public class ExplodeEffect : GrenadeEffect
{
    public override void Explode(Grenade grenade, Vector2 pos)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, grenade.range, LayerMask.GetMask("Enemy"));
        foreach (var collider in colliders)
        {
            collider.GetComponent<Enemies>().Hurt(grenade.damage);
        }
    }
}
