using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragGrenade : Item
{
    [SerializeField] protected GameObject explodeEffect;

    protected override void Use()
    {
        Throw();
        StartCoroutine(Explode());
    }

    IEnumerator Explode()
    {
        yield return new WaitForSeconds(duration);
        DamageEnemiesInRange();
        SpawnVFX(explodeEffect, .25f);
        Destroy(gameObject);
    }
}
