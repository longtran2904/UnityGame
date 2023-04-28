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

#if UNITY_EDITOR
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
#endif

    public void InitAllWeapons(Transform parent)
    {
        for (int i = 0; i < items.Count; ++i)
        {
            Weapon gun = Instantiate(items[i]);//, parent.position, Quaternion.identity);
            gun.Init().Pickup(parent);
            gun.transform.gameObject.SetActive(i == currentIndex);
            items[i] = gun;

            GameDebug.Assert(!UnityEditor.PrefabUtility.IsPartOfAnyPrefab(gun), $"{gun.name}: {gun.GetHashCode()} is part of a prefab!");
        }
    }

    public void SpawnAndDropWeapon(int weapon, Vector3 pos, Vector2 dropDir, Transform holder)
    {
        Instantiate(items[weapon], pos, Quaternion.identity).Init().Drop(holder, dropDir);
    }

    public void SwapCurrent(Vector2 dropDir, Weapon weapon, Transform holder)
    {
        current.Drop(holder, dropDir);
        current = weapon;
        current.Pickup(holder);
    }

    public void SwitchCurrent(int newIndex, Transform holder)
    {
        if (newIndex != currentIndex)
        {
            current.gameObject.SetActive(false);
            currentIndex = newIndex;
            current.gameObject.SetActive(true);
            current.ResetPosToTransform(holder);
        }
    }
}
