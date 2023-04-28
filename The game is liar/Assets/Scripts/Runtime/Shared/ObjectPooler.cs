using System.Collections.Generic;
using UnityEngine;

public interface IPooledObject
{
    // NOTE: Should there even be a different between Init and Spawn?
    // Maybe we should just move Init to Spawn and run it when spawning an object?
    void OnObjectInit();
    void OnObjectSpawn(GameObject defaultObject);
}

[System.AttributeUsage(System.AttributeTargets.Class|System.AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class SerializedPoolAttribute : System.Attribute { }

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
// I can remove 5) when make my own generator, 4) when rework on the item system, and 2) when rework on the enemy system
// Probably just leave out the remaining
public static class ObjectPooler
{
    private static Dictionary<PoolType, GameObject[]> dictionary;
    
    // NOTE: We can't use RuntimeInitializeOnLoadMethod attribute because Init needs multiple arguments from GameManager which is in a different assembly
    public static void Init(GameObject obj, Pool[] pools)
    {
        dictionary = new Dictionary<PoolType, GameObject[]>((int)PoolType.Count) { [PoolType.None] = new GameObject[0] };
        foreach (Pool pool in pools)
        {
            dictionary[pool.type] = new GameObject[pool.size + 1];
            for (int i = 0; i < pool.size + 1; i++)
            {
                dictionary[pool.type][i] = Object.Instantiate(pool.prefab, Vector3.zero, Quaternion.identity, obj.transform);
                dictionary[pool.type][i].SetActive(false);
                //if (i != 0) // NOTE: Do I need to init the original object?
                dictionary[pool.type][i].GetComponent<IPooledObject>().OnObjectInit();
            }
        }
    }
    
    static int SerializeType(object original, object current)
    {
        int depth = 0;
        System.Func<GameUtils.SerializeData, bool> isOriginalOrFromArray = data => data.field == null;
        
        GameUtils.SerializeType(new object[] { original, current },
                                data => GameUtils.IsSerializableType(data.type) && GameUtils.IsSerializableField(data.field, true),
                                data =>
                                {
                                    GameDebug.Log("Before: " + data.field + " Original: " + data.objs[0] + " Current: " + data.objs[1]);
                                    if (isOriginalOrFromArray(data))
                                        data.objs[1] = data.objs[0];
                                    else
                                        data.field.SetValue(data.parent.objs[1], data.objs[0]);
                                    GameDebug.Log("After:  " + data.field + " Original: " + data.objs[0] + " Current: " + data.field?.GetValue(data.parent.objs[1]));
                                    
                                    if (data.depth > depth)
                                        depth = data.depth;
                                }, data => GameUtils.IsSerializableField(data.field, true) && (data.objs[0] != null || data.objs[1] != null) &&
                                (data.parent != null ? !typeof(Object).IsAssignableFrom(data.type) : true),
                                (data, recursive) =>
                                {
                                    // TODO: Handle the case when only the original or current is null
                                    GameDebug.Assert(data.objs[0] != null && data.objs[1] != null, data.field);
                                    GameDebug.Log(data.field);
                                    GameUtils.SerializeData child = recursive();
                                    if (!isOriginalOrFromArray(data))
                                    {
                                        if (data.field.FieldType.IsArray && data.field.FieldType.GetElementType().IsValueType)
                                        ((System.Array)data.objs[1]).SetValue(child.objs[1], child.index);
                                        else if (data.field.FieldType.IsValueType)
                                            data.field.SetValue(data.parent.objs[1], data.objs[1]);
                                    }
                                });
        
        return depth;
    }
    
    private static GameObject Spawn_(PoolType type, Vector2 pos, Quaternion rot)
    {
        for (int i = 1; i < dictionary[type].Length; i++)
        {
            GameObject instance = dictionary[type][i];
            if (!instance.activeSelf)
            {
                instance.transform.SetPositionAndRotation(pos, rot);
                instance.SetActive(true);
                
                IPooledObject pooledObj = instance.GetComponent<IPooledObject>();
                System.Type objType = pooledObj.GetType();
                //if (objType.GetCustomAttribute<SerializedPoolAttribute>() != null)
                if (false)
                {
                    Component component = instance.GetComponent(objType);
                    Component originalComponent = dictionary[type][0].GetComponent(objType);
                    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                    
                    GameDebug.BeginDebug("Start Serialization Profiling", false, false);
                    GameDebug.Log("Start serializing!");
                    watch.Start();
                    
                    GameDebug.BeginDebug("Serializing", false, false);
                    int depth = SerializeType(originalComponent, component);
                    GameDebug.EndDebug();
                    
                    watch.Stop();
                    GameDebug.Log($"End serializing after: {depth} layer{(depth > 1 ? "s" : "")}. Elapsed Time: {watch.ElapsedMilliseconds}ms");
                    GameDebug.EndDebug();
                }
                
                // TODO: Assert that the default object isn't changing
                pooledObj.OnObjectSpawn(dictionary[type][0]);
                return instance;
            }
        }
        
        return null;
    }
    
    public static T Spawn<T>(PoolType type, Vector2 pos, Vector3 rot) => Spawn_(type, pos, Quaternion.Euler(rot)).GetComponent<T>();
    
    public static void Spawn(PoolType type, Vector2 pos) => Spawn_(type, pos, Quaternion.identity);
    
    public static T Spawn<T>(PoolType type, Vector2 pos) => Spawn_(type, pos, Quaternion.identity).GetComponent<T>();
}
