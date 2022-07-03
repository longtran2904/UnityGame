using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "RuntimeSet/Weapon")]
public class WeaponInventory : ScriptableObject
{
    [HideInInspector] public List<Weapon> items;
    [SerializeField] private List<Weapon> defaultItems;

    [HideInInspector] public int currentIndex;
    [SerializeField] private int startWeaponIndex;

    public Weapon current { get => items[currentIndex]; set => items[currentIndex] = value; }

    void OnEnable()
    {
        if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            items?.Clear();
            if (defaultItems != null)
                items = new List<Weapon>(defaultItems);
            currentIndex = startWeaponIndex;
        }
    }

    public void InitAllWeapons(Transform parent)
    {
        for (int i = 0; i < items.Count; ++i)
        {
            Weapon gun = Instantiate(items[i], parent.position, Quaternion.identity);
            gun.Init().Pickup(parent, i);
            gun.transform.gameObject.SetActive(i == currentIndex);
            items[i] = gun;

#if UNITY_EDITOR
            Debug.Assert(!UnityEditor.PrefabUtility.IsPartOfAnyPrefab(gun), $"{gun.name}: {gun.GetHashCode()} is part of a prefab!");
#endif
        }
    }

    public void SpawnAndDropWeapon(int weapon, Vector3 pos, Vector2 dropDir)
    {
        Instantiate(items[weapon], pos, Quaternion.identity).Init().Drop(dropDir);
    }

    public void SwapCurrent(Vector2 dropDir, Weapon weapon, Transform parent)
    {
        current.Drop(dropDir);
        current = weapon;
        current.Pickup(parent, currentIndex);
    }
}
