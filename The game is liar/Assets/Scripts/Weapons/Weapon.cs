using UnityEngine;
using EZCameraShake;
using System;
using TMPro;

public enum FireMode
{
    Auto,
    Burst,
    Single
}
/*
 * Reload: WeaponStat, slider, reload bar, perfect and active size
 * Shoot: WeaponStat, ShootPos, muzzleFlash
 * UI: ammo, sprite
 * Rotate: onGround
 */
public class Weapon : MonoBehaviour
{
    public WeaponStat stat;

    Camera mainCamera;
    public float offset;

    private Transform shootPos;
    private float timeBtwShots;
    private float startTimeBtwShots;

    public BoolReference onGround;

    private GameObject muzzleFlash;
    public float muzzelFlashTime;
    private float muzzelFlashTimeValue;

    public int burstCount;
    private int shotsRemainingInBurst;
    bool triggerReleasedSinceLastShot = true;

    private int currentAmmo;
    private bool isReloading;

    [HideInInspector] public TextMeshProUGUI ammoText;
    public event Action reloadingDelegate;
    [HideInInspector] public bool canSwitch = true;
    public bool canReload = true;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        muzzelFlashTimeValue = muzzelFlashTime;
        shotsRemainingInBurst = burstCount;
        currentAmmo = stat.ammo;
        startTimeBtwShots = 1 / stat.fireRate;
        shootPos = transform.Find("ShootPos");
        muzzleFlash = transform.Find("MuzzleFlash").gameObject;
        ammoText = GameObject.Find("AmmoText")?.GetComponent<TextMeshProUGUI>();
        ammoText?.SetText("{0}/{1}", currentAmmo, stat.ammo);
    }

    private void OnEnable()
    {
        isReloading = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseMenu.isGamePaused)
        {
            return;
        }

        MuzzleFlash();

        if (onGround == null)
        {
            return; // This gun is for an enemy so we don't need the code below
        }

        ammoText?.SetText("{0}/{1}", currentAmmo, stat.ammo);

        if (isReloading)
        {
            return;
        }

        if (currentAmmo <= 0 && Input.GetMouseButton(0) && canReload)
        {
            Reload();
        }
        else if (Input.GetKeyDown(KeyCode.R) && currentAmmo < stat.ammo && canReload)
        {
            Reload();
        }
        else
        {
            FlipWeapon(RotationToMousePosition());
            OnTriggerHold();
            OnTriggerReleased();
        }
    }

    void Reload()
    {
        isReloading = true;
        canSwitch = false;
        reloadingDelegate();
    }

    float RotationToMousePosition()
    {
        Vector3 difference = mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        return rotationZ;
    }

    public void SetAmmo()
    {
        currentAmmo = stat.ammo;
        isReloading = false;
        canSwitch = true;
    }

    // flip the weapon correctly toward the mouse position
    void FlipWeapon(float rotZ)
    {
        // Normal
        if (onGround.value)
        {
            if (rotZ <= 90 && rotZ >= -90)
            {
                transform.rotation = Quaternion.Euler(180f, 0f, -(rotZ + offset));
            }
            else if (rotZ > 90 || rotZ < -90)
            {
                transform.rotation = Quaternion.Euler(180f, 180f, (rotZ + offset - 180));
            }
        }
        else
        {
            if (rotZ <= 90 && rotZ >= -90)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, rotZ + offset);
            }
            else if (rotZ > 90 || rotZ < -90)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, -(rotZ + offset - 180));
            }
        }
    }

    void MuzzleFlash()
    {
        if (muzzelFlashTimeValue <= 0)
        {
            muzzleFlash.SetActive(false);
            muzzelFlashTimeValue = muzzelFlashTime;
        }
        if (muzzleFlash.activeSelf == true)
        {
            muzzelFlashTimeValue -= Time.deltaTime;
        }
    }

    // shoot projectile toward the mouse position
    void ShootProjectile()
    {
        if (Time.time > timeBtwShots)
        {
            if (stat.mode == FireMode.Burst)
            {
                if (shotsRemainingInBurst == 0)
                {
                    return;
                }
                shotsRemainingInBurst--;
            }
            else if (stat.mode == FireMode.Single)
            {
                if (!triggerReleasedSinceLastShot)
                {
                    return;
                }
            }
            currentAmmo--;
            bool isCritical = UnityEngine.Random.value < stat.critChance;
            ObjectPooler.instance.SpawnFromPool<Projectile>(stat.projectile, shootPos.transform.position, transform.rotation).Init(
                isCritical ? stat.critDamage : stat.damage, stat.knockback, 0, false, isCritical);
            timeBtwShots = Time.time + startTimeBtwShots;
            muzzleFlash.SetActive(true);
            muzzelFlashTimeValue = muzzelFlashTime;
            AudioManager.instance.Play(stat.sfx);
            CameraShaker.Instance.ShakeOnce(4, 1, 0.1f, .1f);
        }
    }

    public void OnTriggerHold()
    {
        if (Input.GetMouseButton(0))
        {
            ShootProjectile();
            triggerReleasedSinceLastShot = false;
        }
    }

    public void OnTriggerReleased()
    {
        if (Input.GetMouseButtonUp(0))
        {
            triggerReleasedSinceLastShot = true;
            shotsRemainingInBurst = burstCount;
        }
    }
}
