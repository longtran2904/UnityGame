using UnityEngine;

[CreateAssetMenu(menuName = "Data/Weapon Stat")]
public class WeaponStat : ScriptableObject
{
    [Header("Stat")]
    public int damage;
    public int critDamage;
    public float critChance;
    public float fireRate;
    public int ammo;
    public float knockback;
    public int price;

    [Header("UI Info")]
    public Sprite icon;
    public string weaponName;
    public string description;

    [Header("Reload Info")]
    public float standardReload = 3.0f;
    public float activeReload = 2.25f;
    public float perfectReload = 1.8f;
    public float failedReload = 4.1f;

    public float timeBtwShots => 1f / fireRate;
}
