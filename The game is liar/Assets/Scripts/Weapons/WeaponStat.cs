using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Weapon Stat")]
public class WeaponStat : ScriptableObject
{
    public int damage;
    public int critDamage;
    public float critChance;
    public float fireRate;
    public int ammo;
    public float knockback;
    public int price;

    [Header("Dependencies Info")]
    public string sfx = "PlayerShoot";
    public string projectile = "PlayerBullet";

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
