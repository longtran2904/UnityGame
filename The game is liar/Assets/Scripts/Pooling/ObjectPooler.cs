using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: Has the same problem with Application.isPlaying like the AudioManager
//       Hopefully it will be fixed in next update
[CreateAssetMenu(menuName = "Scriptable/ObjectPooler")]
public class ObjectPooler : ScriptableObject
{
    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    public static ObjectPooler instance;

    public void Init()
    {
        if (!instance)
        {
            instance = this;
            instance.poolDictionary = new Dictionary<string, Queue<GameObject>>();

            foreach (Pool pool in instance.pools)
            {
                Queue<GameObject> objectToPool = new Queue<GameObject>();
                for (int i = 0; i < pool.size; i++)
                {
                    GameObject gameObject = Instantiate(pool.prefab);
                    gameObject.SetActive(false);
                    objectToPool.Enqueue(gameObject);
                }
                instance.poolDictionary.Add(pool.tag, objectToPool);
            }
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            InternalDebug.LogWarning("Pool with tag: " + tag + " doesn't exists.");
            return null;
        }

        return SpawnAndDequeue(tag, position, rotation, parent);
    }

    public T SpawnFromPool<T>(string tag, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            InternalDebug.LogWarning("Pool with tag: " + tag + " doesn't exists.");
            return default(T);
        }

        return SpawnAndDequeue(tag, position, rotation, parent).GetComponent<T>();
    }

    private GameObject SpawnAndDequeue(string tag, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject objToSpawn = poolDictionary[tag].Dequeue();

        objToSpawn.SetActive(true);
        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;
        objToSpawn.transform.SetParent(parent);

        IPooledObject pooledObj = objToSpawn.GetComponent<IPooledObject>();
        pooledObj?.OnObjectSpawn();

        poolDictionary[tag].Enqueue(objToSpawn);
        return objToSpawn;
    }
}
