using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponStat stat;
    public Transform shootPos;
    public GameObject muzzleFlash;
    public float muzzelFlashTime;

    [HideInInspector] public int currentAmmo;
    public Vector2 posOffset;

    void Awake()
    {
        currentAmmo = stat.ammo;
        shootPos = transform.Find("ShootPos");
        muzzleFlash = transform.Find("MuzzleFlash").gameObject;
    }
}