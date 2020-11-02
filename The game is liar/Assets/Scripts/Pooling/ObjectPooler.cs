using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    public static ObjectPooler instance;

    private void Awake()
    {
        if (instance != null)
        {
            InternalDebug.Log("Instance already exists, Destroying object!");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectToPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject gameObject = Instantiate(pool.prefab);
                gameObject.SetActive(false);
                objectToPool.Enqueue(gameObject);
            }

            poolDictionary.Add(pool.tag, objectToPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            InternalDebug.LogWarning("Pool with tag: " + tag + " doesn't exists.");
            return null;
        }

        GameObject objToSpawn = poolDictionary[tag].Dequeue();

        objToSpawn.SetActive(true);
        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;

        IPooledObject pooledObj = objToSpawn.GetComponent<IPooledObject>();
        if (pooledObj != null)
        {
            pooledObj.OnObjectSpawn();
        }

        poolDictionary[tag].Enqueue(objToSpawn);

        return objToSpawn;
    }

    public T SpawnFromPool<T>(string tag, Vector3 position, Quaternion rotation) where T : Component
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            InternalDebug.LogWarning("Pool with tag: " + tag + " doesn't exists.");
            return default(T);
        }

        GameObject objToSpawn = poolDictionary[tag].Dequeue();

        objToSpawn.SetActive(true);
        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;

        IPooledObject pooledObj = objToSpawn.GetComponent<IPooledObject>();
        if (pooledObj != null)
        {
            pooledObj.OnObjectSpawn();
        }

        poolDictionary[tag].Enqueue(objToSpawn);

        return objToSpawn.GetComponent<T>();
    }
}
