using System.Collections;
using UnityEngine;
using EZCameraShake;
using System;
using TMPro;

public class Weapon : MonoBehaviour
{
    public enum FireMode
    {
        Auto,
        Burst,
        Single
    }

    public FireMode fireMode;

    Camera mainCamera;

    public float offset;

    public Projectile projectilePrefab;
    public Transform shotPos;

    private float timeBtwShots;
    public float startTimeBtwShots;

    PlayerController player;

    public int damage;
    private Projectile projectile;

    public GameObject muzzleFlash;
    public float muzzelFlashTime;
    private float muzzelFlashTimeValue;

    public int burstCount;
    private int shotsRemainingInBurst;
    bool triggerReleasedSinceLastShot = true;

    public int maxAmmo;
    private int currentAmmo;
    [HideInInspector] public float reloadTime;
    private bool isReloading;

    public float knockback;

    public GameObject hitEffect;

    [Header("Reload Info")]
    public float standardReload = 3.0f;
    public float activeReload = 2.25f;
    public float perfectReload = 1.8f;
    public float failedReload = 4.1f;

    public TextMeshProUGUI ammoText;

    public event Action reloadingDelegate;

    [HideInInspector] public bool canSwitch = true;

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            mainCamera = Camera.main;
            player = GetComponentInParent<PlayerController>();
        }
        catch (System.Exception _ex)
        {
            Debug.Log($"Error sending data to server via TCP: {_ex}");
        }
        muzzelFlashTimeValue = muzzelFlashTime;
        shotsRemainingInBurst = burstCount;
        currentAmmo = maxAmmo;
        ammoText = GameObject.Find("AmmoText") ? GameObject.Find("AmmoText").GetComponent<TextMeshProUGUI>() : null;
        if (ammoText != null)
        {
            ammoText.SetText("{0}/{1}", (float)currentAmmo, (float)maxAmmo);
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

        if (isReloading)
        {
            return;
        }

        if (currentAmmo <= 0 && Input.GetMouseButton(0))
        {
            Reload();
            return;
        }
        else if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            Reload();
            return;
        }

        Vector3 difference = mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        if (player)
        {
            FlipWeapon(rotationZ);
        }

        OnTriggerHold();

        OnTriggerReleased();
    }

    void Reload()
    {
        isReloading = true;

        canSwitch = false;

        reloadingDelegate();
    }

    public void SetAmmo()
    {
        currentAmmo = maxAmmo;

        isReloading = false;

        canSwitch = true;
    }

    // flip the weapon correctly toward the mouse position
    void FlipWeapon(float rotZ)
    {
        if (player == null)
        {
            Debug.Log("Player is missing!");
            return;
        }

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
    public void ShootProjectile(string poolName, string music)
    {
        if (Time.time > timeBtwShots)
        {
            if (fireMode == FireMode.Burst)
            {
                if (shotsRemainingInBurst == 0)
                {
                    return;
                }
                shotsRemainingInBurst--;
            }
            else if (fireMode == FireMode.Single)
            {
                if (!triggerReleasedSinceLastShot)
                {
                    return;
                }
            }

            currentAmmo--;

            projectile = ObjectPooler.instance.SpawnFromPool<Projectile>(poolName, shotPos.transform.position, transform.rotation);
            projectile.damage = damage;
            projectile.knockbackForce = new Vector2(knockback, knockback);
            projectile.hitEffect = hitEffect;

            timeBtwShots = Time.time + startTimeBtwShots;

            muzzleFlash.SetActive(true);
            muzzelFlashTimeValue = muzzelFlashTime;

            if (ammoText != null)
                ammoText.SetText("{0}/{1}", (float)currentAmmo, (float)maxAmmo);

            CameraShaker.Instance.ShakeOnce(4, 1, 0.1f, .1f);

            AudioManager.instance.Play(music);
        }
    }

    public void OnTriggerHold()
    {
        if (Input.GetMouseButton(0))
        {
            string bullet = "PlayerBullet";
            string sound = "PlayerShoot";
            if (fireMode == FireMode.Single)
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
