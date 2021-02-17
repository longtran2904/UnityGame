using UnityEngine;

[CreateAssetMenu(menuName = "RuntimeSet/Weapon")]
public class WeaponInventory : RuntimeSet<Weapon>
{
    [HideInInspector] public int currentWeapon;
    [SerializeField] private int startWeaponIndex;

    protected override void OnEnable()
    {
        base.OnEnable();
        currentWeapon = startWeaponIndex;
    }

    public void AddAndSetCurrent(Weapon weapon)
    {
        if (!items.Contains(weapon))
        {
            items.Add(weapon);
            currentWeapon = items.IndexOf(weapon);
        }
    }

    public bool SetCurrent(Weapon weapon)
    {
        if (items.Contains(weapon))
        {
            currentWeapon = items.IndexOf(weapon);
            return true;
        }
        return false;
    }

    public Weapon GetCurrent()
    {
        return items[currentWeapon];
    }
}
