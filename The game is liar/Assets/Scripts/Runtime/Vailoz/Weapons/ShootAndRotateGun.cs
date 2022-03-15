using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAndRotateGun : MonoBehaviour
{
    public AudioManager audioManager;

    private float holdTime;
    private float timeBtwShots;

    private Weapon currentWeapon;
    private WeaponInventory inventory;

    public float lagDistance;
    public float recoverLagTime;
    private bool hasLagBehind;

    private PlayerController player;
    private Coroutine gunLerping;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<PlayerController>();
        inventory = GetComponentInParent<Player>().inventory;
        currentWeapon = inventory.GetCurrent();
        timeBtwShots = 1 / currentWeapon.stat.fireRate;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        currentWeapon = inventory.GetCurrent();
        if (gunLerping == null && !hasLagBehind)
        {
            Vector2 difference = GameInput.GetDirToMouse(transform.position);
            float rotZ = difference == Vector2.zero ? 0 : Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
            currentWeapon.transform.localRotation = Quaternion.Euler(0f, 0f, (difference.x >= 0 ? rotZ : 180f - rotZ) * transform.up.y);
        }
        else
        {
            currentWeapon.transform.rotation = transform.rotation;
            return;
        }

        if (!player.groundCheck) // The hasLagBehind has already been checked above
            StartCoroutine(LagGunBehind());

        if (GameInput.GetInput(InputType.Shoot))
            ShootProjectile();
        else
            holdTime = 0;
    }


    void ShootProjectile()
    {
        if (Time.time > timeBtwShots && currentWeapon.currentAmmo > 0)
        {
            currentWeapon.currentAmmo--;

            bool isCritical = Random.value < currentWeapon.stat.critChance;
            float rand = Mathf.PerlinNoise(0, holdTime) * 2f - 1f;
            float rotZ = holdTime > 0 ? 15f * rand : 0;
            ObjectPooler.instance.SpawnFromPool<Projectile>(currentWeapon.stat.projectile, currentWeapon.shootPos.position,
                currentWeapon.transform.rotation * Quaternion.Euler(0, 0, rotZ)).Init(isCritical ? currentWeapon.stat.critDamage : currentWeapon.stat.damage,
                currentWeapon.stat.knockback, 0, false, isCritical);

            timeBtwShots = Time.time + 1f / currentWeapon.stat.fireRate;
            holdTime += timeBtwShots - Time.time;

            StartCoroutine(ShootEffect());
            audioManager.PlayAudio(AudioType.Player_Shoot);

            CameraShake.instance?.Knockback(-currentWeapon.transform.right * .4f, 20f);
        }
    }

    IEnumerator ShootEffect()
    {
        Weapon weapon = currentWeapon;
        Debug.Assert(weapon.transform.localPosition == (Vector3)weapon.posOffset);
        Vector3 kickback = Vector3.one * .15f;
        weapon.transform.localPosition -= kickback;
        weapon.muzzleFlash.SetActive(true);

        float duration = weapon.muzzelFlashTime + Time.time;
        while (Time.time <= duration)
        {
            if (!weapon.gameObject.activeSelf)
                break;
            yield return null;
        }

        weapon.muzzleFlash.SetActive(false);
        weapon.transform.localPosition += kickback;
    }

    IEnumerator LagGunBehind()
    {
        Debug.Assert(lagDistance > Mathf.Abs(currentWeapon.posOffset.y), "The gun's posOffset is greater than the lagDistance");

        hasLagBehind = true;
        float worldPosY = currentWeapon.transform.position.y;
        bool onFloor = transform.up.y > 0;

        while (Mathf.Abs(transform.position.y - worldPosY) < lagDistance)
        {
            if (player.groundCheck)
            {
                hasLagBehind = false;
                currentWeapon.transform.localPosition = currentWeapon.posOffset;
                yield break;
            }

            currentWeapon.transform.position = new Vector3(currentWeapon.transform.position.x, worldPosY);
            yield return null;
        }

        if (onFloor == (transform.up.y > 0))
        {
            while (onFloor == (transform.up.y > 0))
            {
                if (player.groundCheck)
                    goto END; // This is for falling.
                yield return null;
            }
            // This is for flipping while lagging.
            currentWeapon.transform.localPosition = new Vector3(currentWeapon.transform.localPosition.x, -currentWeapon.transform.localPosition.y);
        }
        // else case is for flipping _then_ lagging.

        while (!player.groundCheck)
            yield return null;

        END:
        gunLerping = StartCoroutine(LerpPos(lagDistance, -lagDistance, true));
        hasLagBehind = false;

        IEnumerator LerpPos(float startY, float endY, bool reverse)
        {
            float t = 0;
            while (t < recoverLagTime)
            {
                if (!player.groundCheck)
                {
                    gunLerping = null;
                    yield break;
                }

                currentWeapon.transform.localPosition = new Vector3(currentWeapon.posOffset.x, Mathf.Lerp(startY, endY, t / recoverLagTime));
                t += Time.deltaTime;
                yield return null;
            }

            if (reverse)
            {
                gunLerping = StartCoroutine(LerpPos(endY, currentWeapon.posOffset.y, false));
            }
            else
            {
                gunLerping = null;
                currentWeapon.transform.localPosition = currentWeapon.posOffset;
            }
        }
    }
}
