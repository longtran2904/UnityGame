using UnityEngine;

public class Destructable : MonoBehaviour
{
    [Range(0, 1)] public float particleScaleMultipler = 1f;
    public PoolType[] particles;
    [MinMax(0, 10)] public RangedInt dropRange;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            foreach (PoolType particle in particles)
                ObjectPooler.Spawn(particle, transform.position);
            Drop();
            Destroy(gameObject);
        }
    }

    [EasyButtons.Button]
    void Drop()
    {
        if (Application.isPlaying)
        {
            int range = dropRange.randomValue;
            for (int i = 0; i < range; i++)
                ObjectPooler.Spawn(PoolType.Cell, transform.position + new Vector3(Random.Range(-.5f, .5f), Random.Range(-.5f, .5f)));
        }
    }
}
