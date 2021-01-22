using UnityEngine;

[CreateAssetMenu(menuName = "RuntimeSet/Weapon")]
public class WeaponInventory : RuntimeSet<Weapon>
{
    public int currentWeapon;

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
