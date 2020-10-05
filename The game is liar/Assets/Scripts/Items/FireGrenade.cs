using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Grenade/Fire Grenade")]
public class FireGrenade : Grenade
{
    public float burnTime;
    public float timeBtwBurn;
    public GameObject fireParticle;

    public override void Explode()
    {
        AudioManager.instance.StartCoroutine(FireExplode());
    }

    public IEnumerator FireExplode()
    {
        while (!GroundCheck())
        {
            yield return new WaitForFixedUpdate();
        }
        base.Explode();
    }

    public bool GroundCheck()
    {
        Vector2 size = new Vector2(0.1f, 0.04f);
        Vector3 offset = new Vector2(0, grenadeObject.GetComponent<SpriteRenderer>().bounds.extents.y + size.y);
        ExtDebug.DrawBoxCastBox(grenadeObject.transform.position - offset, size, Quaternion.identity, Vector2.down, size.y, Color.red);
        return Physics2D.BoxCast(grenadeObject.transform.position - offset, size, 0, Vector2.down, size.y, LayerMask.GetMask("Ground"));
    }
}
