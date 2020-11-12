using System.Collections;
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

public class Weapon : MonoBehaviour
{
    public WeaponStat stat;

    Camera mainCamera;
    public float offset;

    public Projectile projectilePrefab;
    public Transform shotPos;
    private float timeBtwShots;
    float startTimeBtwShots;

    PlayerController player;
    private Projectile projectile;

    public GameObject muzzleFlash;
    public float muzzelFlashTime;
    private float muzzelFlashTimeValue;

    public int burstCount;
    private int shotsRemainingInBurst;
    bool triggerReleasedSinceLastShot = true;

    private int currentAmmo;
    private bool isReloading;

    public TextMeshProUGUI ammoText;
    public event Action reloadingDelegate;
    [HideInInspector] public bool canSwitch = true;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        player = GetComponentInParent<PlayerController>();
        muzzelFlashTimeValue = muzzelFlashTime;
        shotsRemainingInBurst = burstCount;
        currentAmmo = stat.ammo;
        startTimeBtwShots = 1 / stat.fireRate;
        ammoText = GameObject.Find("AmmoText") ? GameObject.Find("AmmoText").GetComponent<TextMeshProUGUI>() : null;
        if (ammoText)
        {
            ammoText.SetText("{0}/{1}", (float)currentAmmo, (float)stat.ammo);
        }
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

        if (!player)
        {
            return; // This gun is for an enemy so we don't need the code below
        }

        ammoText.SetText("{0}/{1}", currentAmmo, stat.ammo);

        if (isReloading)
        {
            return;
        }
        if (currentAmmo <= 0 && Input.GetMouseButton(0))
        {
            Reload();
            return;
        }
        else if (Input.GetKeyDown(KeyCode.R) && currentAmmo < stat.ammo)
        {
            Reload();
            return;
        }

        FlipWeapon(RotationToMousePosition());
        OnTriggerHold();
        OnTriggerReleased();
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
        if (player.top)
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
    public void ShootProjectile(string poolName, string sfx)
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
            SpawnAndSetupProjectiles(poolName);
            timeBtwShots = Time.time + startTimeBtwShots;
            muzzleFlash.SetActive(true);
            muzzelFlashTimeValue = muzzelFlashTime;
            CameraShaker.Instance.ShakeOnce(4, 1, 0.1f, .1f);
            AudioManager.instance.Play(sfx);
        }
    }

    public void ShootProjectileForEnemy(string poolName, string sfx)
    {
        if (Time.time > timeBtwShots)
        {
            SpawnAndSetupProjectiles(poolName, true);
            timeBtwShots = Time.time + startTimeBtwShots;
            muzzleFlash.SetActive(true);
            muzzelFlashTimeValue = muzzelFlashTime;
            AudioManager.instance.Play(sfx);
        }
    }

    private void SpawnAndSetupProjectiles(string poolName, bool isEnemy = false)
    {
        projectile = ObjectPooler.instance.SpawnFromPool<Projectile>(poolName, shotPos.transform.position, transform.rotation);
        bool isCritical = UnityEngine.Random.value < stat.critChance;
        projectile.Init(isCritical ? stat.critDamage : stat.damage, stat.knockback, 0, isEnemy, isCritical);
    }

    public void OnTriggerHold()
    {
        if (Input.GetMouseButton(0))
        {
            string bullet = "PlayerBullet";
            string sound = "PlayerShoot";
            if (stat.mode == FireMode.Single)
            {
                sound = "Shotgun";
            }
            ShootProjectile(bullet, sound);
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
