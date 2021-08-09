using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAndRotateGun : MonoBehaviour
{
    public BoolReference onGround;

    private float timeBtwShots;
    private bool triggerReleasedSinceLastShot = true;

    private Weapon currentWeapon;
    private WeaponInventory inventory;

    private Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        inventory = GetComponentInParent<Player>().inventory;
        currentWeapon = inventory.GetCurrent();
        timeBtwShots = 1 / currentWeapon.stat.fireRate;
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseMenu.isGamePaused)
        {
            return;
        }

        FlipWeapon();
        OnTriggerHold();
        OnTriggerReleased();
    }

    void FlipWeapon()
    {
        Vector3 difference = mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        currentWeapon.transform.localRotation = Quaternion.Euler(0f, 0f, (difference.x > 0 ? rotZ : 180f - rotZ) * transform.up.y);
    }

    void ShootProjectile()
    {
        if (Time.time > timeBtwShots && currentWeapon.currentAmmo > 0)
        {
            if (currentWeapon.stat.mode == FireMode.Single)
            {
                if (!triggerReleasedSinceLastShot)
                {
                    return;
                }
            }
            currentWeapon.currentAmmo--;
            bool isCritical = Random.value < currentWeapon.stat.critChance;
            ObjectPooler.instance.SpawnFromPool<Projectile>(currentWeapon.stat.projectile, currentWeapon.shootPos.position, currentWeapon.transform.rotation).Init(
                isCritical ? currentWeapon.stat.critDamage : currentWeapon.stat.damage, currentWeapon.stat.knockback, 0, false, isCritical);
            timeBtwShots = Time.time + 1 / currentWeapon.stat.fireRate;
            StartCoroutine(MuzzleFlash());
            AudioManager.instance.PlaySfx(currentWeapon.stat.sfx);
            EZCameraShake.CameraShaker.Instance?.ShakeOnce(4, 1, 0.1f, .1f);
        }
    }

    IEnumerator MuzzleFlash()
    {
        currentWeapon.muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(currentWeapon.muzzelFlashTime);
        currentWeapon.muzzleFlash.SetActive(false);
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
        }
    }
}
