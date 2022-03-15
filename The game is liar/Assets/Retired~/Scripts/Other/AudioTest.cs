using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTest : MonoBehaviour
{
    public int cellCount;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < cellCount; i++)
            ObjectPooler.instance.SpawnFromPool<Cell>("Money", transform.position + (Vector3)MathUtils.RandomVector2(Vector2.one * -5, Vector2.one * 5), Quaternion.identity).MoveTowardPlayer();
    }
}