using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Stat")]
public class WeaponStat : ScriptableObject
{
    public int damage;
    public int critDamage;
    public float critChance;
    public float fireRate;
    public FireMode mode;
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
}
