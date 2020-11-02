using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour
{
    [Range(0, 1)] public float particleScaleMultipler = 1f;
    public GameObject destroyParticle;
    public GameObject remainParticle;
    public DropRange dropRange;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (destroyParticle) Instantiate(destroyParticle, transform.position, Quaternion.identity).transform.localScale *= particleScaleMultipler;
            if (remainParticle) Instantiate(remainParticle, transform.position, Quaternion.identity).transform.localScale *= particleScaleMultipler;
            for (int i = 0; i < dropRange.GetRandom(); i++)
            {
                Vector3 offset = new Vector3(Random.Range(-.5f, .5f), Random.Range(-.5f, .5f));
                ObjectPooler.instance.SpawnFromPool("Money", transform.position + offset, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
