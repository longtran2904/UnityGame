using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Grenade/Frag Grenade")]
public class FragGrenade : Grenade
{
    public float explodeTime;

    public override void Explode()
    {
        AudioManager.instance.StartCoroutine(FragExplode());
    }

    public IEnumerator FragExplode()
    {
        yield return new WaitForSeconds(explodeTime);
        base.Explode();        
    }
}
