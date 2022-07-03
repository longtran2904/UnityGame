using System.Collections.Generic;
using UnityEngine;

public interface IPooledObject
{
    // NOTE: Every pooled objects need to have an OnObjectInit function because they will instantly get disabled after being instantiated.
    void OnObjectInit();
    void OnObjectSpawn();
}

public enum PoolType
{
    None,

    Enemy_Alien,
    Enemy_Drone,
    Enemy_Bat,
    Enemy_Maggot,
    Enemy_NoEye,
    Enemy_GiantEye,

    Cell,
    Meteor,
    DamagePopup,
    DamagePopup_Critical,
    Bullet_Normal,
    Bullet_Blood,
    Bullet_Bounce,

    VFX_Spawner,
    VFX_Destroyed_Enemy,
    VFX_Destroyed_Bullet,
    VFX_Destroyed_Bullet_Bounce,
    VFX_Destroyed_Glass,
    VFX_Destroyed_Wood,
    VFX_Destroyed_Paper,

    // Item

    Count
}

[System.Serializable]
public class Pool
{
    public PoolType type;
    public GameObject prefab;
    public int size;
}

// TODO: Currently, Instantiate only gets called in:
// 1.TextboxHandler for spawning a single textboxCanvs at the start
// 2.EnemySpawner for spawning enemies
// 3.WeaponInventory for spawning weapons
// 4.Item - related stuff
// 5.LevelInfoPostProcess for door objects
// 6.EqualDistribution for spawning points
// I can remove 5) when making my generator, 4) when reworking on the item system, 2) when reworking on the enemy system.
// Probably just leave out the remaining.
public static class ObjectPooler
{
    private static Dictionary<PoolType, GameObject[]> dictionary;

    public static void Init(GameObject obj, Pool[] pools)
    {
        dictionary = new Dictionary<PoolType, GameObject[]>((int)PoolType.Count)
        {
            [PoolType.None] = new GameObject[0]
        };
        foreach (Pool pool in pools)
        {
            dictionary[pool.type] = new GameObject[pool.size];
            for (int i = 0; i < pool.size; i++)
            {
                dictionary[pool.type][i] = Object.Instantiate(pool.prefab, Vector3.zero, Quaternion.identity, obj.transform);
                dictionary[pool.type][i].GetComponent<IPooledObject>().OnObjectInit();
                dictionary[pool.type][i].SetActive(false);
            }
        }
    }

    public static GameObject Spawn(PoolType type, Vector2 pos, Quaternion rot)
    {
        foreach (GameObject instance in dictionary[type])
        {
            if (!instance.activeSelf)
            {
                instance.transform.SetPositionAndRotation(pos, rot);
                instance.SetActive(true);
                instance.GetComponent<IPooledObject>().OnObjectSpawn();
                return instance;
            }
        }

        return null;
    }

    public static GameObject Spawn(PoolType type, Vector2 pos) => Spawn(type, pos, Quaternion.identity);
}
