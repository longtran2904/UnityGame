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

    private void OnEnable()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += state => { if (state == UnityEditor.PlayModeStateChange.EnteredPlayMode) Init(); };
#else
        if (Application.isPlaying) Init();
#endif
    }

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

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, Transform parent = null, bool worldSpae = true)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            InternalDebug.LogWarning("Pool with tag: " + tag + " doesn't exists.");
            return null;
        }

        return SpawnAndDequeue(tag, position, rotation, parent, worldSpae);
    }

    public T SpawnFromPool<T>(string tag, Vector3 position, Quaternion rotation, Transform parent = null, bool worldSpace = true) where T : Component
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            InternalDebug.LogWarning("Pool with tag: " + tag + " doesn't exists.");
            return default(T);
        }

        return SpawnAndDequeue(tag, position, rotation, parent, worldSpace).GetComponent<T>();
    }

    private GameObject SpawnAndDequeue(string tag, Vector3 position, Quaternion rotation, Transform parent, bool worldSpace)
    {
        GameObject objToSpawn = poolDictionary[tag].Dequeue();

        objToSpawn.SetActive(true);
        objToSpawn.transform.SetParent(parent);
        if (worldSpace) objToSpawn.transform.position = position;
        else objToSpawn.transform.localPosition = position;
        objToSpawn.transform.rotation = rotation;

        IPooledObject pooledObj = objToSpawn.GetComponent<IPooledObject>();
        pooledObj?.OnObjectSpawn();

        poolDictionary[tag].Enqueue(objToSpawn);
        return objToSpawn;
    }
}
