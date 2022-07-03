using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour
{
    [Header("Switch 'n Shoot")]
    public WeaponInventory inventory;
    public bool loop;

    private float timeBtwShots;
    private float holdTime;

    [Header("Reload")]
    public Slider slider;
    public Image handle;
    public RectTransform perfectRange;
    public Color failedColor;
    public Color perfectColor;
    public RangedFloat perfectPos;

    struct ReloadData
    {
        public Color handleColor;
        public RectTransform rect;
        public float reloadRange;

        public ReloadData(Color handleColor, RectTransform t)
        {
            this.handleColor = handleColor;
            rect = t;
            reloadRange = rect.sizeDelta.x;
        }
    }
    private ReloadData failedReload;
    private ReloadData perfectReload;

    [Header("Lag Movement")]
    public float lagDelta;
    public float speed;

    enum GunLagState
    {
        None,
        Jumping,
        Dropping,
        InAirNotFlipped,
        InAir,
        LagDownward,
        LagBackward,
    }
    private GunLagState lagState;
    private float playerDir;
    private PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        inventory.InitAllWeapons(transform);
        failedReload = new ReloadData(failedColor, (RectTransform)slider.transform);
        perfectReload = new ReloadData(perfectColor, perfectRange);
        player = GetComponentInParent<PlayerController>();
    }

    // NOTE: LateUpdate is for the lag gun to flip right after the player flips while jumping and for resetting the reload bar's rotation.
    void LateUpdate()
    {
        {
            bool isPlayerGrounded = player?.groundCheck ?? GetComponentInParent<Entity>().HasProperty(EntityProperty.IsGrounded);
            Weapon current = inventory.current;
            switch (lagState)
            {
                case GunLagState.None:
                    {
                        if (GameInput.GetInput(InputType.Jump))
                            lagState = GunLagState.Jumping;
                        else if (!isPlayerGrounded)
                            lagState = GunLagState.Dropping;
                        else
                            break;
                        playerDir = transform.up.y;
                        current.transform.localRotation = Quaternion.identity;
                    } return;
                case GunLagState.Jumping:
                    {
                        if (Done())
                            lagState = GunLagState.InAirNotFlipped;
                        else if (ShouldFlip())
                        {
                            lagState = GunLagState.Dropping;
                            Flip();
                        }
                        else
                            LagGun(-1);
                    } return;
                case GunLagState.Dropping:
                    {
                        if (Done())
                            lagState = GunLagState.InAir;
                        else
                            LagGun(1);
                    } return;
                case GunLagState.InAirNotFlipped:
                    {
                        if (ShouldFlip())
                        {
                            Flip();
                            lagState = GunLagState.InAir;
                        }
                    } return;
                case GunLagState.InAir:
                    {
                        if (isPlayerGrounded)
                        {
                            current.transform.localPosition = current.posOffset + new Vector2(0, lagDelta);
                            lagState = GunLagState.LagDownward;
                        }
                    } return;
                case GunLagState.LagDownward:
                    {
                        LagGun(-1);
                        if (Done())
                            lagState = GunLagState.LagBackward;
                    } return;
                case GunLagState.LagBackward:
                    {
                        LagGun(1);
                        if (current.transform.localPosition.y >= current.posOffset.y)
                        {
                            current.transform.localPosition = current.posOffset;
                            lagState = GunLagState.None;
                        }
                    } return;

                    bool Done() => Mathf.Abs(current.transform.localPosition.y - current.posOffset.y) >= lagDelta;
                    bool ShouldFlip() => playerDir != transform.up.y;

                    void LagGun(float dir)
                    {
                        current.transform.localPosition += new Vector3(0, speed * Time.deltaTime) * dir;
                    }

                    void Flip()
                    {
                        current.transform.localPosition = -current.transform.localPosition;
                        playerDir = transform.up.y;
                    }
            }
        }

        // Switch gun
        {
            int scrollInput = (int)GameInput.GetMouseWheel();
            int newWeapon = MathUtils.LoopIndex(inventory.currentIndex + scrollInput, inventory.items.Count, loop);

            if (newWeapon != inventory.currentIndex)
            {
                transform.GetChild(inventory.currentIndex).gameObject.SetActive(false);
                transform.GetChild(newWeapon).gameObject.SetActive(true);
                inventory.currentIndex = newWeapon;
            }
        }

        Weapon currentWeapon = inventory.current;

        // Rotate gun
        {
            Vector2 difference = GameInput.GetDirToMouse(transform.position);
            float rotZ = difference == Vector2.zero ? 0 : Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
            currentWeapon.transform.localRotation = Quaternion.Euler(0f, 0f, (difference.x >= 0 ? rotZ : 180f - rotZ) * transform.up.y);
        }

        bool ShouldShoot(Weapon weapon) => weapon.currentAmmo > 0 && GameInput.GetInput(InputType.Shoot);
        bool ShouldReload(Weapon weapon) => (weapon.currentAmmo == 0 && GameInput.GetInput(InputType.Shoot)) ||
            (weapon.currentAmmo < weapon.stat.ammo && GameInput.GetInput(InputType.Reload));

        if (ShouldShoot(currentWeapon))
        {
            ShootProjectile(ref timeBtwShots, ref holdTime, currentWeapon);

            void ShootProjectile(ref float timeBtwShots, ref float holdTime, Weapon currentWeapon)
            {
                if (Time.time > timeBtwShots)
                {
                    currentWeapon.currentAmmo--;

                    bool isCritical = Random.value < currentWeapon.stat.critChance;
                    float rand = holdTime > 0 ? Mathf.PerlinNoise(0, holdTime) * 2f - 1f : 0;
                    GameObject bullet = ObjectPooler.Spawn(PoolType.Bullet_Normal, currentWeapon.shootPos.position, currentWeapon.transform.rotation * Quaternion.Euler(0, 0, 15f * rand));
                    bullet.GetComponent<Entity>().InitBullet(currentWeapon.stat, isCritical, false);

                    float dTime = 1f / currentWeapon.stat.fireRate;
                    timeBtwShots = Time.time + dTime;
                    holdTime += dTime;

                    StartCoroutine(ShootEffect(currentWeapon));
                    AudioManager.PlayAudio(AudioType.Player_Shoot);
                    CameraSystem.instance.Shake(ShakeMode.GunKnockback, -currentWeapon.transform.right);
                }

                IEnumerator ShootEffect(Weapon currentWeapon)
                {
                    Weapon weapon = currentWeapon;
                    Debug.Assert(weapon.transform.localPosition == (Vector3)weapon.posOffset, $"Pos: {weapon.transform.localPosition:F5} Offset: {weapon.posOffset:F5}");
                    Vector3 kickback = Vector3.one * .15f;
                    weapon.transform.localPosition -= kickback;
                    weapon.muzzleFlash.SetActive(true);

                    float duration = weapon.muzzelFlashTime + Time.time;
                    while (Time.time <= duration)
                    {
                        // NOTE: When the player switches to a different weapon
                        if (!weapon.gameObject.activeSelf)
                            break;
                        yield return null;
                    }

                    weapon.muzzleFlash.SetActive(false);
                    weapon.transform.localPosition = weapon.posOffset;
                }
            }
        }
        else if (ShouldReload(currentWeapon))
        {
            StartCoroutine(Reloading(slider, handle, currentWeapon, failedReload, perfectReload, perfectPos));

            IEnumerator Reloading(Slider slider, Image handle, Weapon weapon, ReloadData failed, ReloadData perfect, RangedFloat perfectPos)
            {
                EnableReloading(failed, false);
                float maxTime = slider.maxValue = weapon.stat.standardReload;
                float perfectX = perfectPos.randomValue;
                perfect.rect.anchoredPosition = new Vector2(perfectX * failed.reloadRange, 0);

                yield return null;

                for (float t = 0; t < 1; t += Time.deltaTime / maxTime)
                {
                    float value = Mathf.Lerp(0, maxTime, t);
                    slider.value = value;

                    if (GameInput.GetRawInput(InputType.Reload))
                    {
                        if (MathUtils.RangeInRange(perfectX, perfect.reloadRange / failed.reloadRange, t, handle.rectTransform.sizeDelta.x / failed.reloadRange))
                        {
                            handle.color = perfect.handleColor;
                            maxTime = weapon.stat.perfectReload;
                        }
                        else
                        {
                            handle.color = failed.handleColor;
                            maxTime = weapon.stat.failedReload;
                        }
                        break;
                    }
                    yield return null;
                }

                yield return new WaitForSeconds(Mathf.Clamp(maxTime - slider.value, 0, maxTime));
                handle.color = Color.white;
                weapon.currentAmmo = weapon.stat.ammo;
                EnableReloading(failed, true);

                void EnableReloading(ReloadData failed, bool enable)
                {
                    failed.rect.gameObject.SetActive(!enable);
                    GameInput.EnableInput(InputType.Shoot, enable);
                    GameInput.EnableInput(InputType.Reload, enable);
                    GameInput.EnableInput(InputType.Interact, enable);
                    GameInput.EnableMouseInput(enable, 0);
                }
            }
        }
        else
            holdTime = 0;

        slider.transform.rotation = Quaternion.identity;
    }
}
