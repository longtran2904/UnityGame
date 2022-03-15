using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public ObjectPooler pooler;

    void Awake()
    {
        if (!ObjectPooler.instance)
        {
            DontDestroyOnLoad(gameObject);
            pooler.Init(); 
        }
    }
}
