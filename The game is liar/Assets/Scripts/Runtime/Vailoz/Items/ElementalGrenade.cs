using System;
using UnityEngine;

public class ElementalGrenade : ElementalItem
{
    [SerializeField] protected GameObject explodeEffect;

    protected override void Use()
    {
        Throw();
    }

    private void FixedUpdate()
    {
        if (GroundCheck())
        {
            AddStateToEnemies(GetAllNearbyEnemies(), state);
            SpawnVFX(explodeEffect, .25f);
            Destroy(gameObject);
        }
    }
}
