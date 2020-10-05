using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item Effects/Fire")]
public class FireEffect : GrenadeEffect
{
    private float timer;

    public override void Explode(Grenade grenade, Vector2 pos)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, grenade.range, LayerMask.GetMask("Enemy"));
        Enemies[] enemies = new Enemies[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            enemies[i] = colliders[i].GetComponent<Enemies>();
        }
        AudioManager.instance.StartCoroutine(DamageOverTime((FireGrenade)grenade, enemies));
    }

    private IEnumerator DamageOverTime(FireGrenade grenade, Enemies[] enemies)
    {
        GameObject[] fireEffects = new GameObject[enemies.Length];
        timer = grenade.burnTime;
        for (int i = 0; i < fireEffects.Length; i++)
        {
            fireEffects[i] = Instantiate(grenade.fireParticle, enemies[i].transform.position, grenade.fireParticle.transform.rotation, enemies[i].transform);
        }
        while (timer > 0)
        {
            foreach (var enemy in enemies)
            {
                enemy.Hurt(grenade.damage);
            }
            timer -= grenade.timeBtwBurn;
            yield return new WaitForSeconds(grenade.timeBtwBurn);
        }
        foreach (var fire in fireEffects)
        {
            Destroy(fire);
        }
    }
}
