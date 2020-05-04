using System.Collections;
using UnityEngine;
using EZCameraShake;

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

    private AudioManager audioManager;

    public int burstCount;
    private int shotsRemainingInBurst;
    bool triggerReleasedSinceLastShot = true;

    public int maxAmmo;
    private int currentAmmo;
    public float reloadTime;
    private bool isReloading;

    public float knockback;

    public GameObject hitEffect;

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
        audioManager = FindObjectOfType<AudioManager>();
        shotsRemainingInBurst = burstCount;
        currentAmmo = maxAmmo;
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

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
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

    IEnumerator Reload()
    {
        isReloading = true;

        Debug.Log("Reloading");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;

        isReloading = false;
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
    public void ShootProjectile()
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

            projectile = Instantiate(projectilePrefab, shotPos.transform.position, transform.rotation) as Projectile;
            projectile.damage = damage;
            projectile.knockbackForce = new Vector2(knockback, knockback);
            projectile.hitEffect = hitEffect;

            timeBtwShots = Time.time + startTimeBtwShots;

            muzzleFlash.SetActive(true);
            muzzelFlashTimeValue = muzzelFlashTime;

            CameraShaker.Instance.ShakeOnce(4, 1, 0.1f, .1f);

            audioManager.Play("PlayerShoot");
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
