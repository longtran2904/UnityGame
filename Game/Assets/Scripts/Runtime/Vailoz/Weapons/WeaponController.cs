using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour
{
    [Header("Switch 'n Shoot")]
    public WeaponInventory inventory;
    public bool loop;
    public SpringData spring;
    
    [Header("Shoot VFX")]
    public float muzzleFlashTime;
    public float knockbackForce;
    
    [Header("Reload VFX")]
    public Slider slider;
    public Image handle;
    public RectTransform perfectBar;
    public Color failedColor;
    public Color perfectColor;
    public RangedFloat perfectPos;
    
    private float upDir;
    private float groundTime;
    private bool isReloading;
    private float timeBtwShots;
    private float holdTime;
    private Vector2 knockbackOffset;
    
    struct ReloadData
    {
        public Image handle;
        public Color color;
        public float time;
        
        public static float UpdateReload(ReloadData data)
        {
            data.handle.color = data.color;
            return data.time;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        inventory.InitAllWeapons(GameManager.player.transform);
        upDir = inventory.current.transform.up.y;
        spring.Init(inventory.current.transform.position.y);
    }
    
    void LateUpdate()
    {
        slider.transform.rotation = Quaternion.identity; // NOTE: LateUpdate is for reseting the reload bar's rotation
        Transform holder = GameManager.player.transform;
        
        /*
         * Docs: There're two ways to implement this:
         * 1. The gun still be a child to the player, and we will modify its local position
         *      - Pros: Only need to change local y value. We don't need to care about where the player's facing.
         *      - Cons: Flipping is hard to do. We need to change the gun's position.
         * 2. The gun will be fatherless (like me ):), and we will modify its actual position
         *      - Pros: Don't need special code to handle flipping.
         *      - Cons: Need to manually modify its position which is different depending on where the player's facing. We can also have precision errors.
         * In the end, I chose the second way.
         */
        {
            GameDebug.BeginDebug("Lag Gun", true, true);
            
            Transform gun = inventory.current.transform;
            Vector2 holdOffset = inventory.current.posOffset;
            SpriteRenderer weaponSr = gun.GetComponent<SpriteRenderer>();
            bool isGrounded = GameManager.player.HasProperty(EntityProperty.IsGrounded);
            
            if (isGrounded)
                upDir = holder.up.y;
            Vector2 targetPos = GameUtils.GetDirectionalPos(holder, holdOffset, upDir);
            targetPos += knockbackOffset;
            
            Vector2 newPos = targetPos;
            newPos.y = MathUtils.SecondOrder(Time.deltaTime, targetPos.y, gun.position.y, spring);
            gun.SetPositionAndRotation(newPos, holder.rotation);
            
            GameDebug.DrawBox(targetPos, Vector2.one * .25f, Color.yellow);
            GameDebug.DrawBox(weaponSr.bounds.center, weaponSr.bounds.size, Color.red);
            GameDebug.EndDebug();
            
            if (!isGrounded)
                groundTime = 0;
            else
                groundTime += Time.deltaTime;
            
            if (groundTime < .4f)
                return;
            else
                GameDebug.Assert(MathUtils.IsApproximate(spring.dy, 0, 0.09999f) || MathUtils.IsApproximate(newPos.y, targetPos.y, 0.09999f),
                                 $"Isn't approximated: dy: {spring.dy}, target: {targetPos.y}, newPos: {newPos.y}, difference: {Mathf.Abs(targetPos.y - newPos.y)}");
        }
        
        if (isReloading)
            return;
        
        // Switch gun
        {
            int newWeapon = MathUtils.LoopIndex(inventory.currentIndex + (int)GameInput.GetMouseWheel(), inventory.items.Count, loop);
            inventory.SwitchCurrent(newWeapon, holder);
        }
        
        Transform weaponTransform = inventory.current.transform;
        WeaponStat stat = inventory.current.stat;
        int currentAmmo = GetAmmo();
        
        // Rotate gun
        weaponTransform.rotation = MathUtils.LookRotation(GameInput.GetDirToMouse(transform.position), holder.forward);
        
        // Shoot gun
        if (currentAmmo > 0 && GameInput.GetInput(InputType.Shoot) && Time.time > timeBtwShots)
        {
            {
                SetAmmo(currentAmmo - 1);
                timeBtwShots = Time.time + stat.timeBtwShots;
                holdTime += stat.timeBtwShots;
                
                bool isCritical = Random.value < stat.critChance;
                float rot = holdTime > 0 ? (Mathf.PerlinNoise(0, holdTime) * 2f - 1f) * 15f : 0;
                Shoot(stat, rot, isCritical);
            }
            
            // TODO: Make the EntityVFX system handle this
            {
                AudioManager.PlayAudio(AudioType.Player_Shoot);
                CameraSystem.instance.Shake(ShakeMode.GunKnockback, -weaponTransform.right);
                
                Debug.Assert(knockbackOffset == Vector2.zero, $"Pos: {weaponTransform.position:F5} Offset: {knockbackOffset:F5}");
                Debug.Assert(muzzleFlashTime <= stat.timeBtwShots, $"Flash time: {muzzleFlashTime}, Shoot time: {stat.timeBtwShots}"); // TODO: Handle this
                
                Vector2 knockback = -weaponTransform.right * knockbackForce;
                Knockback(knockback, true);
                this.InvokeAfter(muzzleFlashTime, () => Knockback(-knockback, false), () => weaponTransform.gameObject.activeSelf && !isReloading);
                Debug.Break();
                
                void Knockback(Vector2 knockback, bool start)
                {
                    weaponTransform.position += (Vector3)knockback;
                    spring.prevX += knockback.y;
                    knockbackOffset += knockback;
                    EnableMuzzleFlash(start);
                }
            }
        }
        // Reload gun
        else if ((currentAmmo == 0 && GameInput.GetInput(InputType.Shoot)) || (currentAmmo < stat.ammo && GameInput.GetInput(InputType.Reload)))
        {
            float startPerfectX = perfectPos.randomValue;
            float failedRange = ((RectTransform)slider.transform).sizeDelta.x;
            float perfectRange = perfectBar.sizeDelta.x;
            
            slider.value = 0;
            slider.maxValue = stat.standardReload;
            perfectBar.anchoredPosition = new Vector2(startPerfectX * failedRange, 0);
            
            StartCoroutine(Reloading(slider, stat.standardReload,
                                     new ReloadData { handle = handle, color = failedColor, time = stat.failedReload },
                                     new ReloadData { handle = handle, color = perfectColor, time = stat.perfectReload },
                                     new ReloadData { handle = handle, color = Color.white },
                                     t => MathUtils.RangeInRange(startPerfectX, perfectRange / failedRange, t, handle.rectTransform.sizeDelta.x / failedRange),
                                     enable =>
                                     {
                                         isReloading = enable;
                                         slider.gameObject.SetActive(enable);
                                         GameInput.EnableInput(InputType.Interact, !enable);
                                         SetAmmo(enable ? 0 : stat.ammo);
                                     }));
            
            // TODO: Make the EntityVFX or GameUI system handle this
            static IEnumerator Reloading(Slider slider, float maxTime, ReloadData failed, ReloadData perfect, ReloadData finish,
                                         System.Func<float, bool> isPerfect, System.Action<bool> enableReloading)
            {
                enableReloading(true);
                yield return null;
                
                float t = 0;
                float startMaxTime = maxTime;
                while (t < 1)
                {
                    yield return null;
                    t += Time.deltaTime / maxTime;
                    if (maxTime == startMaxTime)
                    {
                        slider.value = Mathf.Lerp(0, maxTime, t);
                        if (GameInput.GetInput(InputType.Reload))
                        {
                            maxTime = ReloadData.UpdateReload(isPerfect(t) ? perfect : failed);
                            t = Mathf.InverseLerp(0, maxTime, slider.value);
                        }
                    }
                }
                
                ReloadData.UpdateReload(finish);
                enableReloading(false);
            }
        }
        else if (!GameInput.GetInput(InputType.Shoot))
            holdTime = 0;
    }
    
    int GetAmmo() => inventory.current.currentAmmo;
    
    void SetAmmo(int ammo) => inventory.current.currentAmmo = ammo;
    
    void EnableMuzzleFlash(bool enable) => inventory.current.muzzleFlash.SetActive(enable);
    
    void Shoot(WeaponStat stat, float rotZ, bool isCritical)
    {
        Vector2 pos = inventory.current.shootPos.position;
        Vector3 rot = transform.eulerAngles + Vector3.forward * rotZ;
        ObjectPooler.Spawn<Entity>(PoolType.Bullet_Normal, pos, rot).InitBullet(isCritical ? stat.critDamage : stat.damage, isCritical, false);
    }
}
