#define NEW_VFX
#define NEW_AI
using System.Collections;
using UnityEngine;

public enum EntityProperty
{
    CanJump,
    CanBeHurt,
    DamageWhenCollide,
    DieWhenCollide,
    DieAfterMoveTime,
    DieAfterEffect,
    SpawnCellWhenDie,
    SpawnDamagePopup,
    ClampToMoveRegion,
    StartAtMinMoveRegion,
    IsCritical,
    UsePooling,
    CustomInit, // TODO: Maybe collapse this and UsePooling into one
    AddMoneyWhenCollide,
    FallingOnSpawn,
    
    // Don't show in the inspector
    IsGrounded,
    IsReloading,
    AtEndOfMoveRegion,
    
    Count
}

public class Entity : MonoBehaviour, IPooledObject
{
#region Ability
    /*
    * RUNNING
    * - Turn speed
    * - Acceleration
    * - Decceleration
    * - Max speed
    * JUMPING
    * - Duration
    * - Jump height
    * - Down gravity
    * - Air acceleration (what about air decceleration?)
    * - Air control (movement in air/can the player change direction in air?/The equivalent of turn speed but in air)
    * - Air brake (does the player still move forward when he stop pressing?)
    * CAMERA
    * - Damping (X/Y/Jump)
    * - Lookahead
    * - Zoom
    * ASSISTS
    * - Coyote time
    * - Jump buffer
    * - Terminal velocity
    * - Rounded corners
    * JUICE
    * - Particles (run/jump/land)
    * - Squash and stretch (jump/land)
    * - Trail
    * - Lean (angle and speed)
    */
    
    // Move horizontally based on input
    // Jump
    // Flip gravity based on input
    // dash when near
    // explode when near/died
    // Teleport when player is far away
    // Move towards the player, teleport when hit a cliff
    
    public enum AbilityType
    {
        None,
        Move,
        Teleport,
        Explode,
        Jump,
    }
    
    public enum AbilityFlag
    {
        Interuptible,
        AwayFromPlayer,
        LockOnPlayerForEternity,
        
        ExecuteWhenLowHealth,
        ExecuteWhenInRange,
        ExecuteWhenInRangeY,
        ExecuteWhenOutOfMoveRegion,
        OrCombine,
        
        CanExecute,
    }
    
    [System.Serializable]
    public class EntityAbility
    {
        public Property<AbilityFlag> flags;
        public EntityVFX vfx;
        public AbilityType type;
        
        public int healthToExecute;
        public float distanceToExecute;
        public float distanceToExecuteY;
        public float cooldownTime;
        public float interuptibleTime;
        
        public float chargeTime;
        public float duration;
        public float range;
        public int damage;
    }
    
    public bool CanUseAbility(EntityAbility ability)
    {
        Property<AbilityFlag> flags = ability.flags;
        Vector2 pos = GameManager.player?.transform.position ?? (transform.position + Vector3.one);
        
        bool lowHealth  = flags.HasProperty(AbilityFlag.ExecuteWhenLowHealth) == (health < ability.healthToExecute);
        bool isInRange  = flags.HasProperty(AbilityFlag.ExecuteWhenInRange) == IsInRange(ability.distanceToExecute, pos);
        bool isInRangeY = flags.HasProperty(AbilityFlag.ExecuteWhenInRangeY) == IsInRangeY(ability.distanceToExecuteY, pos);
        bool moveRegion = flags.HasProperty(AbilityFlag.ExecuteWhenOutOfMoveRegion) == HasProperty(EntityProperty.AtEndOfMoveRegion);
        
        bool result = Check(ability.flags.HasProperty(AbilityFlag.OrCombine), lowHealth, isInRange, isInRangeY, moveRegion);
        return result;
        
        static bool Check(bool condition, params bool[] values)
        {
            foreach (bool b in values)
                if (b == condition)
                    return condition;
            return !condition;
        }
    }
    
    public IEnumerator UseAbility(EntityAbility ability, MoveType moveType, TargetType targetType, float speed)
    {
        ability.flags.SetProperty(AbilityFlag.CanExecute, false);
        float cooldownTime = 0;
        this.moveType = MoveType.None;
        float timer = 0;
        ability.vfx.canStop = () => ability.flags.HasProperty(AbilityFlag.CanExecute);
#if !NEW_VFX
        PlayVFX(ability.vfx);
#endif
        while (timer < ability.interuptibleTime)
        {
            if (ability.flags.HasProperty(AbilityFlag.Interuptible))
                if (!CanUseAbility(ability))
                    goto END;
            yield return null;
            timer += Time.deltaTime;
        }
        
        this.targetType = TargetType.None;
        yield return new WaitForSeconds(ability.chargeTime - ability.interuptibleTime);
        
        switch (ability.type)
        {
            case AbilityType.Move:
            {
                this.moveType = moveType;
                this.speed = ability.range / ability.duration;
                this.targetType = targetType;
                damage = ability.damage;
                if (ability.flags.HasProperty(AbilityFlag.AwayFromPlayer))
                    targetDir = MathUtils.Sign(targetPos - (Vector2)transform.position) * new Vector2(-1, 1);
                if (ability.flags.HasProperty(AbilityFlag.LockOnPlayerForEternity))
                {
                    StartFalling(false);
                    yield break;
                }
            } break;
            
            case AbilityType.Teleport:
            {
                Debug.Break();
                float playerUp = GameManager.player.transform.up.y;
                Vector3 destination;
                {
                    destination = GameManager.player.transform.position;
                    destination.y += (spriteExtents.y - GameManager.player.spriteExtents.y) * playerUp;
                    float distance = ability.range * Mathf.Sign(GameManager.player.transform.position.x - transform.position.x);
                    
                    // TODO: Maybe teleport opposite to where the player is heading or teleport to nearby platform
                    if (!IsPosValid(distance))
                        if (!IsPosValid(-distance))
                            goto END;
                    
                    bool IsPosValid(float offsetX)
                    {
                        Vector3 offset = new Vector3(offsetX, 0);
                        bool onGround = GameUtils.BoxCast(destination + offset - new Vector3(0, spriteExtents.y * playerUp),
                                                          new Vector2(spriteExtents.x, .1f), Color.yellow);
                        bool insideWall = GameUtils.BoxCast(destination + offset + new Vector3(0, .1f * playerUp), data.sr.bounds.size, Color.green);
                        if (onGround && !insideWall)
                        {
                            destination.x += offsetX;
                            return true;
                        }
                        return false;
                    }
                }
                
                if (playerUp != transform.up.y)
                    transform.Rotate(180, 0, 0);
                
                yield return null;
                transform.position = destination;
                CalculateMoveRegion();
            } break;
            
            case AbilityType.Explode:
            {
                // NOTE: If the enemy die then it will be handled by the vfx system
                if (IsInRange(ability.range, GameManager.player.transform.position))
                    GameManager.player.Hurt(ability.damage);
            } break;
            
            case AbilityType.Jump:
            {
                
            } break;
        }
        
        yield return new WaitForSeconds(ability.duration);
        cooldownTime = ability.cooldownTime;
        
        END:
        this.speed = speed;
        this.targetType = targetType;
        this.moveType = moveType;
        
        yield return null;
        //currentAbility = MathUtils.LoopIndex(currentAbility + 1, abilities.Length, true);
        
        yield return new WaitForSeconds(cooldownTime);
        ability.flags.SetProperty(AbilityFlag.CanExecute, true);
    }
#endregion
    
    public enum TargetOffsetType
    {
        None,
        Mouse,
        Player,
    }
    
    public enum MoveRegionType
    {
        None,
        Ground,
        Vertical,
    }
    
    public enum AttackTrigger
    {
        None,
        MouseInput,
    }
    
    [Header("Stat")]
    public Property<EntityProperty> properties;
    public EntityType type;
    
    public EntityAbility[] abilities;
    private int currentAbility;
    
#if NEW_AI
    private float abilityTimer;
    private AbilityTime time;
#endif
    
    public int health;
    public int damage;
    public int money;
    public int ammo;
    
    public string[] collisionTags;
    [MinMax(0, 10)] public RangedInt valueRange;
    
    [Header("Attack")]
    public WeaponStat stat;
    public AttackTrigger attackTrigger;
    private RangedFloat attackDuration;
    
    [Header("Movement")]
    public MoveType moveType;
    public RotateType rotateType;
    public TargetType targetType;
    [Range(0f, 50f)] public float speed;
    public float dRotate;
    public float range;
    public float maxFallingSpeed;
    public TargetOffsetType offsetType;
    public Vector2 targetOffset;
    public SpringData spring;
    [MinMax(-1, 1)] public RangedFloat fallDir;
    
    public MoveRegionType regionType;
    [ShowWhen("regionType", MoveRegionType.Vertical)] public float verticalHeight;
    private Rect moveRegion;
    
    public float moveTime;
    private float aliveTime;
    private float showTime;
    
    public float groundRememberTime;
    private float groundRemember;
    
    public float fallRememberTime;
    private float fallRemember;
    
    public float jumpPressedRememberTime;
    private float jumpPressedRemember;
    
    private Vector2 velocity;
    private Rigidbody2D rb;
    private Collider2D cd;
    private float speedY;
    private Vector2 targetDir;
    private Vector2 targetPos;
    private Vector2 offsetDir;
    
    [Header("Effects")]
    public Material whiteMat;
    public ParticleSystem leftDust;
    public ParticleSystem rightDust;
    public EntityVFX spawnVFX, deathVFX, hurtVFX;
    
#if false
    private SpriteRenderer sr;
    private Animator anim;
    private TrailRenderer trail;
    private TMPro.TextMeshPro text;
#else
    private VFXData data;
#endif
    [HideInInspector]
    public Vector2 spriteExtents;
    
    public void OnObjectInit()
    {
        if (HasProperty(EntityProperty.UsePooling))
            Init();
    }
    
    public void CustomInit()
    {
        Init();
        OnObjectSpawn(null);
        // TODO: Figure out whether we need to disable or destroy the object
    }
    
    public void OnObjectSpawn(GameObject defaultObject)
    {
        Entity entity = defaultObject?.GetComponent<Entity>();
        if (entity)
        {
            properties = entity.properties;
            // TODO: testProperties
            Debug.Assert(entity.abilities?.Length == abilities?.Length);
            currentAbility = 0;
            for (int i = 0; i < entity.abilities.Length; ++i)
            {
                abilities[i].flags = entity.abilities[i].flags;
                abilities[i].vfx.properties = entity.abilities[i].vfx.properties;
            }
            
            health = entity.health;
            damage = entity.damage;
            money = entity.money;
            ammo = entity.ammo;
            valueRange = entity.valueRange;
            
            attackTrigger = entity.attackTrigger;
            attackDuration = entity.attackDuration;
            
            moveType = entity.moveType;
            rotateType = entity.rotateType;
            targetType = entity.targetType;
            speed = entity.speed;
            range = entity.range;
            offsetType = entity.offsetType;
            
            moveTime = entity.moveTime;
            aliveTime = 0;
            
            groundRememberTime = entity.groundRememberTime;
            groundRemember = 0;
            
            fallRememberTime = entity.fallRememberTime;
            fallRemember = 0;
            
            jumpPressedRememberTime = entity.jumpPressedRememberTime;
            jumpPressedRemember = 0;
            
            velocity = Vector2.zero;
            speedY = 0;
            
            // NOTE: weaponStat, collisionTags, dRotate, maxFallingSPeed, spring, fallDir?
        }
        
        CalculateMoveRegion();
        if (HasProperty(EntityProperty.FallingOnSpawn))
            StartFalling(true);
        
        if (HasProperty(EntityProperty.DieAfterEffect))
        {
            float animTime = 0;
            if (data.anim)
            {
                data.sr.enabled = true;
                AnimatorStateInfo state = data.anim.GetCurrentAnimatorStateInfo(0);
                data.anim.Play(state.shortNameHash);
                animTime = state.length;
            }
            
            float particleTime = 0;
            ParticleSystem particle = GetComponent<ParticleSystem>();
            if (particle)
            {
                particle.Play();
                particleTime = particle.main.duration; // TODO: Check if this is the correct duration
            }
            
            aliveTime = Mathf.Max(animTime, particleTime);
        }
        if (HasProperty(EntityProperty.DieAfterMoveTime))
            aliveTime = Time.time + moveTime;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if (!HasProperty(EntityProperty.UsePooling) && !HasProperty(EntityProperty.CustomInit))
        {
            Init();
            OnObjectSpawn(null);
        }
    }
    
#region Initialize
    void Init()
    {
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<Collider2D>();
        
        data = new VFXData
        {
            transform = transform,
            sr = GetComponent<SpriteRenderer>(),
            anim = GetComponent<Animator>(),
            text = GetComponent<TMPro.TextMeshPro>(),
            trail = GetComponent<TrailRenderer>(),
        };
        SpriteRenderer sr = data.sr;
        
        if (rb && maxFallingSpeed != 0)
        {
            float drag = MathUtils.GetDragFromAcceleration(Mathf.Abs(Physics2D.gravity.y * rb.gravityScale), maxFallingSpeed);
            Debug.Assert(drag > 0, drag);
            rb.drag = drag;
        }
        if (whiteMat)
            data.whiteMat = Instantiate(whiteMat);
        if (sr)
            spriteExtents = sr.bounds.extents;
        
        MoveType move = moveType;
        RotateType rotate = rotateType;
        hurtVFX.done += () =>
        {
            SetProperty(EntityProperty.CanBeHurt, true);
            moveType = move;
            rotateType = rotate;
        };
        
        ammo = stat?.ammo ?? 0;
        if (spring != null && spring.f != 0)
            spring.Init(GameManager.player.transform.position.y);
    }
    
    public void InitCamera(bool automatic, bool useSmoothDamp, Vector2 value, float waitTime)
    {
        //SetProperty(EntityProperty.HasTargetOffset, !automatic);
        SetProperty(EntityProperty.StartAtMinMoveRegion, automatic);
        offsetType = automatic ? TargetOffsetType.None : TargetOffsetType.Mouse;
        targetType = automatic ? TargetType.MoveRegion : TargetType.Player;
        
        speed = value.magnitude;
        moveType = useSmoothDamp ? MoveType.SmoothDamp : MoveType.Fly;
        speed = value.x;
        speedY = value.y;
        
        // NOTE: This ability only executes when the camera has TargetType.MoveRegion
        abilities[0].flags = new Property<AbilityFlag>(AbilityFlag.ExecuteWhenOutOfMoveRegion, AbilityFlag.CanExecute);
        abilities[0].chargeTime = waitTime;
        
        GameInput.BindEvent(GameEventType.NextRoom, room => ToNextRoom(GameManager.GetBoundsFromRoom(room).ToRect()));
        void ToNextRoom(Rect roomRect)
        {
            moveRegion = roomRect;
            moveRegion.min += GameManager.mainCam.HalfSize();
            moveRegion.max -= GameManager.mainCam.HalfSize();
            Debug.Assert((moveRegion.xMin <= moveRegion.xMax) && (moveRegion.yMin <= moveRegion.yMax),
                         $"Camera's limit is wrong: (Move region: {moveRegion}, Rect: {roomRect})");
        }
    }
    
    public void InitBullet(int damage, bool isCritical, bool hitPlayer)
    {
        this.damage = damage;
        SetProperty(EntityProperty.IsCritical, isCritical);
        collisionTags[1] = hitPlayer ? "Player" : "Enemy";
    }
    
    public void InitDamagePopup(int damage, bool isCritical)
    {
        targetDir = Vector2.one;
#if NEW_VFX
        VFXManager.PlayVFX(EntityType.DamagePopup, data, VFXKind.Life, damage * (isCritical ? -1 : 1), true);
#else
        data.text.text = damage.ToString();
        spawnVFX = new EntityVFX
        {
            properties = new Property<VFXProperty>(VFXProperty.ScaleOverTime, VFXProperty.FadeTextWhenDone),
            scaleTime = moveTime / 2, scaleOffset = Vector2.one / 2, fadeTime = 1f / 3f,
            fontSize = isCritical ? 3f : 2.5f, textColor = isCritical ? Color.red : Color.white,
        };
        PlayVFX(spawnVFX);
#endif
    }
    
    [EasyButtons.Button]
    public void Pickup()
    {
        if (GameManager.player)
            spring.Init(GameManager.player.transform.position.y);
        offsetType = TargetOffsetType.Player;
        moveType = MoveType.Spring;
        targetType = TargetType.Player;
        rotateType = RotateType.Weapon;
        attackTrigger = AttackTrigger.MouseInput;
    }
    
    public void Shoot(bool isCritical, float rotZ)
    {
        Vector3 rot = transform.eulerAngles + Vector3.forward * rotZ;
        Entity bullet = ObjectPooler.Spawn<Entity>(PoolType.Bullet_Normal, transform.GetChild(0).position, rot);
        bullet.InitBullet(isCritical ? stat.critDamage : stat.damage, isCritical, false);
    }
#endregion
    
    // Update is called once per frame
    void Update()
    {
        // TODO: Test if this is actually working
        if (Time.deltaTime == 0)
            return;
        
        bool wasGrounded = HasProperty(EntityProperty.IsGrounded);
        // NOTE: We're passing -transform.up.y rather than -rb.gravityScale because:
        // 1. Some objects don't have a rigidbody. It's also in my roadmap to replace the rigidbody system entirely.
        // 2. The isGrounded only equals false if and only if the down velocity isn't zero.
        bool isGrounded = SetProperty(EntityProperty.IsGrounded,
                                      GameUtils.GroundCheck(transform.position, spriteExtents, -transform.up.y, Color.clear));
        
#if !NEW_AI
        if (abilities.Length > 0 &&
            abilities[currentAbility].flags.HasProperty(AbilityFlag.CanExecute) &&
            CanUseAbility(abilities[currentAbility]))
            StartCoroutine(UseAbility(abilities[currentAbility], moveType, targetType, speed));
#endif
        
        groundRemember -= Time.deltaTime;
        if (isGrounded)
            groundRemember = groundRememberTime;
        
        fallRemember -= Time.deltaTime;
        if (!isGrounded)
            fallRemember = fallRememberTime;
        
        jumpPressedRemember -= Time.deltaTime;
        if (HasProperty(EntityProperty.CanJump) && GameInput.GetInput(InputType.Jump))
            jumpPressedRemember = jumpPressedRememberTime;
        
        if (HasProperty(EntityProperty.FallingOnSpawn) && !isGrounded)
            return;
        
        //~ NOTE(long): Shooting
        {
            bool canShoot = false;
            bool canReload = false;
            switch (attackTrigger)
            {
                case AttackTrigger.MouseInput:
                {
                    if (!HasProperty(EntityProperty.IsReloading))
                    {
                        canShoot   = ammo >  0 && GameInput.GetInput(InputType.Shoot) && Time.time > attackDuration.max;
                        canReload  = ammo == 0 && GameInput.GetInput(InputType.Shoot);
                        canReload |= ammo <  stat.ammo && GameInput.GetInput(InputType.Reload);
                    }
                } break;
            }
            
            if (canShoot)
            {
                ammo--;
                attackDuration.max = Time.time + stat.timeBtwShots;
                
                bool isCritical = Random.value < stat.critChance;
                float rot = attackDuration.range > 0 ? (Mathf.PerlinNoise(0, attackDuration.range) * 2 - 1) * 15 : 0;
                Shoot(isCritical, rot);
#if NEW_VFX
                Vector2 knockback = -transform.right * .4f;
                VFXManager.PlayVFX(EntityType.Weapon, data, VFXKind.Attack, 0, true,
                                   start =>
                                   {
                                       Vector2 playerDir = GameManager.player.transform.Direction();
                                       knockback *= start ? playerDir : -Vector2.one;
                                       
                                       transform.position += (Vector3)(knockback * playerDir);
                                       spring.prevX += knockback.y * playerDir.y;
                                       // TODO(long): targetOffset is directional (MoveEntity: targetPos += offsetDir * targetOffset)
                                       // Maybe add another var for global offset?
                                       targetOffset += knockback;
                                   }, () => !gameObject.activeSelf || HasProperty(EntityProperty.IsReloading));
#endif
            }
            else if (canReload)
            {
                StartCoroutine(Reloading(stat, GameManager.gameUI.UpdateReload,
                                         enable =>
                                         {
                                             GameManager.gameUI.EnableReload(enable, stat.standardReload);
                                             GameInput.EnableInput(InputType.Interact, !enable);
                                             SetProperty(EntityProperty.IsReloading, enable);
                                             ammo = enable ? 0 : stat.ammo;
                                         }));
                
                IEnumerator Reloading(WeaponStat stat, System.Func<float, bool, bool> updateUI, System.Action<bool> enable)
                {
                    enable(true);
                    yield return null;
                    
                    float maxTime = stat.standardReload;
                    float t = 0;
                    bool hasReloaded = false;
                    while (t <= maxTime)
                    {
                        yield return null;
                        t += Time.deltaTime;
                        if (!hasReloaded)
                        {
                            bool isPerfect = updateUI(t, hasReloaded = GameInput.GetInput(InputType.Reload));
                            if (hasReloaded)
                                maxTime = isPerfect ? stat.perfectReload : stat.failedReload;
                        }
                    }
                    
                    enable(false);
                }
            }
            else if (!canShoot)
                attackDuration.min = attackDuration.max;
        }
        
        if (aliveTime != 0 && Time.time > aliveTime)
            Die();
        
#if !NEW_AI
        RotateEntity(transform, rotateType, dRotate, velocity.x);
        Vector2 prevVelocity = velocity;
        MoveEntity();
#endif
        
        if (HasProperty(EntityProperty.ClampToMoveRegion))
            transform.position = MathUtils.Clamp(transform.position, moveRegion.min, moveRegion.max, transform.position.z);
        //GameDebug.DrawBox(moveRegion, Color.green);
        
        //~ NOTE(long): Jumping
        bool startJumping = jumpPressedRemember >= 0 && groundRemember >= 0;
        {
#if NEW_AI
#elif NEW_VFX
            if (GameManager.player == this)
            {
                if (startJumping)
                {
                    jumpPressedRemember = 0;
                    groundRemember = 0;
                    // NOTE(long): Increase the y position a small amount so the ground checking will make isGrounded false next frame
                    transform.position += new Vector3(0, 0.05f, 0);
                    rb.gravityScale *= -1;
                    VFXManager.PlayVFX(EntityType.Player, data, VFXKind.Jump, velocity.x, true);
                }
                
                else if (!wasGrounded && isGrounded)
                {
                    if (HasProperty(EntityProperty.FallingOnSpawn))
                        StartFalling(false);
                    CapsuleCollider2D cs = cd as CapsuleCollider2D;
                    System.Action<bool> func = start => cs.direction = start ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
                    VFXManager.PlayVFX(EntityType.Player, data, VFXKind.Fall, velocity.x, false, cs != null ? func : null,
                                       () => jumpPressedRemember >= 0 && groundRemember >= 0);
                }
                
                else if (wasGrounded && !isGrounded)
                {
                    VFXManager.PlayVFX(EntityType.Player, data, VFXKind.Fall, 0, true);
                }
                
                else if (isGrounded)
                {
                    float deltaVelocity = MathUtils.Sign(velocity.x - prevVelocity.x);
                    if (velocity.x == -prevVelocity.x)
                        deltaVelocity *= 2;
                    VFXManager.PlayVFX(EntityType.Player, data, VFXKind.Move, deltaVelocity, velocity.x != 0);
                }
            }
#else
            EntityVFX playerVFX = new EntityVFX
            {
                shakeMode = ShakeMode.PlayerJump,
                waitTime = .25f,
                particles = new ParticleSystem[] { velocity.x >= 0 ? leftDust : null, velocity.x <= 0 ? rightDust : null },
            };
            
            // Start jumping
            if (startJumping)
            {
                jumpPressedRemember = 0;
                groundRemember = 0;
                SetProperty(EntityProperty.IsGrounded, false);
                rb.gravityScale *= -1;
                
                {
                    playerVFX.audio = AudioType.Player_Jump;
                    playerVFX.scaleOffset = new Vector2(-.25f, .25f);
                    playerVFX.rotateTime = .2f;
                    playerVFX.properties.SetProperty(VFXProperty.FlipX, true);
                }
            }
            
            // Landing
            else if (!wasGrounded && isGrounded)
            {
                if (HasProperty(EntityProperty.FallingOnSpawn))
                    StartFalling(false);
                
                {
                    playerVFX.audio = AudioType.Player_Land;
                    playerVFX.scaleOffset = new Vector2(.25f, -.25f);
                    velocity.x = 0; // NOTE: This's for resetting the delta velocity for start/stop moving
                    
                    CapsuleCollider2D capsule = cd as CapsuleCollider2D;
                    if (capsule)
                    {
                        capsule.direction = CapsuleDirection2D.Horizontal;
                        playerVFX.done = () => capsule.direction = CapsuleDirection2D.Vertical;
                    }
                }
            }
            
            // Start falling
            else if (wasGrounded && !isGrounded)
            {
                // TODO: Has a falling animation rather than the first frame of the idle one
                playerVFX = new EntityVFX { properties = new Property<VFXProperty>(VFXProperty.StopAnimation), nextAnimation = "Idle" };
            }
            
            else
            {
                playerVFX = null;
                if (isGrounded)
                {
                    Vector2 deltaVelocity = velocity - prevVelocity;
                    if (velocity.x != 0 && Time.time > timeBtwFootstepsValue)
                    {
                        timeBtwFootstepsValue = Time.time + timeBtwFootsteps.randomValue;
                        playerVFX = new EntityVFX() { audio = footstepAudio };
                    }
                    
                    if (deltaVelocity.x != 0)
                    {
                        if (playerVFX == null)
                            playerVFX = new EntityVFX();
                        playerVFX.particles = new ParticleSystem[] { deltaVelocity.x > 0 ? leftDust : rightDust };
                        if (prevVelocity.x == 0)
                            playerVFX.nextAnimation = "Move";
                        else if (velocity.x == 0)
                            playerVFX.nextAnimation = "Idle";
                    }
                }
            }
            
            // NOTE: Currently, only the player has a jumping/landing/falling VFX, but that will probably change soon.
            // When that happens, remember to abstract this code out. Currently, we have a check that only the player can call PlayVFX here.
            if (GameManager.player == this)
                PlayVFX(playerVFX);
#endif
        }
    }
    
    public enum TargetType
    {
        None,
        Input,
        Player,
        Random,
        MoveDir,
        MoveRegion,
        Target,
        
        Count
    }
    
    public enum MoveType
    {
        None,
        Run,
        Fly,
        SmoothDamp,
        Custom,
        Spring,
        
        Count
    }
    
    void MoveEntity()
    {
        SetProperty(EntityProperty.AtEndOfMoveRegion, false);
        switch (targetType)
        {
            case TargetType.Input:   targetDir = GameInput.GetAxis();       break;
            case TargetType.Random:  targetDir = MathUtils.RandomVector2(); break;
            case TargetType.MoveDir: targetDir = transform.right;           break;
            
            case TargetType.Player:  targetPos = GameManager.player.transform.position; goto case TargetType.Target;
            case TargetType.MoveRegion:
            {
                if (targetPos != moveRegion.max && targetPos != moveRegion.min)
                    SwitchTargetToMax(true, false);
                else if (targetPos == moveRegion.max && MathUtils.IsApproximate(transform.position, moveRegion.max, .001f, +1))
                    SwitchTargetToMax(false, true);
                else if (targetPos == moveRegion.min && MathUtils.IsApproximate(transform.position, moveRegion.min, .001f, -1))
                    SwitchTargetToMax(true, true);
                
                void SwitchTargetToMax(bool toMax, bool atEndOfMoveRegion)
                {
                    targetPos = toMax ? moveRegion.max : moveRegion.min;
                    velocity = Vector2.zero;
                    SetProperty(EntityProperty.AtEndOfMoveRegion, atEndOfMoveRegion);
                    if (!atEndOfMoveRegion && HasProperty(EntityProperty.StartAtMinMoveRegion))
                        transform.position = moveRegion.min.Z(transform.position.z);
                }
            } goto case TargetType.Target;
            
            case TargetType.Target:
            {
                switch (offsetType)
                {
                    case TargetOffsetType.Mouse:  { offsetDir = GameInput.GetMouseDir();                  } break;
                    case TargetOffsetType.Player: { offsetDir = GameManager.player.transform.Direction(); } break;
                }
                targetPos += offsetDir * targetOffset;
                targetDir = targetPos - (Vector2)transform.position;
            } break;
        }
        
        switch (moveType)
        {
            case MoveType.None: velocity = Vector2.zero; break;
            case MoveType.Fly: velocity = targetDir.normalized * speed; break;
            
            case MoveType.Run:
            {
                targetDir.x = MathUtils.Sign(targetDir.x);
                targetDir.y = 0;
                velocity = new Vector2(targetDir.x * speed, rb.velocity.y);
            } break;
            
            case MoveType.SmoothDamp: // TODO: Do we still need SmoothDamp? MoveType.Spring seems to also include this already.
            {
                transform.position = MathUtils.SmoothDamp(transform.position, targetPos, ref velocity, new Vector2(speed, speedY),
                                                          Time.deltaTime, transform.position.z);
            } return;
            case MoveType.Spring:
            {
                Vector2 newPos = targetPos;
                newPos.y = MathUtils.SecondOrder(Time.deltaTime, targetPos.y, transform.position.y, spring);
                //transform.SetPositionAndRotation(newPos, GameManager.player.transform.rotation);
                transform.position = newPos;
            } return;
            
            case MoveType.Custom: return;
        }
        
        if (rb)
            rb.velocity = velocity;
        else
            transform.position += (Vector3)velocity * Time.deltaTime;
    }
    
    public enum RotateType
    {
        None,
        PlayerX,
        MoveDirX,
        Weapon,
        MouseX,
        Linear,
        
        Count
    }
    
    void RotateEntity(Transform transform, RotateType rotateType, float dRotate, float velocityX)
    {
        float dirX = 0;
        Transform player = GameManager.player.transform;
        Vector2 mouseDir = GameInput.GetDirToMouse(transform.position);
        switch (rotateType)
        {
            case RotateType.PlayerX:
            {
                dirX = player.position.x - transform.position.x;
            } break;
            case RotateType.MoveDirX:
            {
                dirX = velocityX;
            } break;
            case RotateType.MouseX:
            {
                dirX = mouseDir.x;
            } break;
            case RotateType.Weapon:
            {
                if (GameManager.player.fallRemember > 0 || HasProperty(EntityProperty.IsReloading))
                    transform.rotation = player.rotation;
                else
                    transform.rotation = MathUtils.LookRotation(mouseDir, player.forward);
            } break;
            case RotateType.Linear:
            {
                transform.Rotate(0, 0, dRotate * Time.deltaTime);
            } break;
        }
        
        if (dirX != 0 && Mathf.Sign(dirX) != Mathf.Sign(transform.right.x))
            transform.Rotate(0, 180, 0);
    }
    
    void StartFalling(bool startFalling)
    {
        rb.velocity = startFalling ? new Vector2(fallDir.randomValue, Random.value).normalized * speed : Vector2.zero;
        moveType = startFalling ? MoveType.Custom : MoveType.Fly;
        cd.isTrigger = !startFalling;
        rb.bodyType = startFalling ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
        SetProperty(EntityProperty.FallingOnSpawn, startFalling);
        if (!startFalling)
            CalculateMoveRegion();
    }
    
    public bool CompleteCycle()
    {
        return HasProperty(EntityProperty.AtEndOfMoveRegion) && targetPos == moveRegion.max;
    }
    
    public void TestPlayerVFX()
    {
        Hurt(0);
    }
    
    public void Hurt(int damage)
    {
        if (HasProperty(EntityProperty.CanBeHurt))
        {
            health -= damage;
            
            // TODO: Replace this with a stat
            MoveType move = moveType;
            RotateType rotate = rotateType;
            
            moveType = MoveType.None;
            rotateType = RotateType.None;
            SetProperty(EntityProperty.CanBeHurt, false);
            
            if (health > 0)
            {
#if NEW_VFX
                VFXManager.PlayVFX(type, data, VFXKind.Hurt, 0, false, start =>
                                   {
                                       if (!start)
                                       {
                                           moveType = move;
                                           rotateType = rotate;
                                           SetProperty(EntityProperty.CanBeHurt, true);
                                       }
                                   });
#else
                PlayVFX(hurtVFX);
#endif
            }
            else
                Die();
        }
    }
    
    void Die()
    {
        System.Action done = () =>
        {
            if (HasProperty(EntityProperty.SpawnCellWhenDie))
                for (int i = valueRange.randomValue; i > 0; i--)
                    ObjectPooler.Spawn(PoolType.Cell, transform.position);
            
            if (HasProperty(EntityProperty.UsePooling))
                gameObject.SetActive(false);
            else
                Destroy(this);
        };
        
#if NEW_VFX
        VFXManager.PlayVFX(type, data, VFXKind.Life, 0, false, start => { if (!start) done(); });
#else
        deathVFX.done = done;
        PlayVFX(deathVFX);
#endif
    }
    
    public bool HasProperty(EntityProperty property)
    {
        return properties.HasProperty(property);
    }
    
    bool SetProperty(EntityProperty property, bool set)
    {
        properties.SetProperty(property, set);
        return set;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnHitEnter(collision);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnHitEnter(collision.collider);
    }
    
    void OnHitEnter(Collider2D collision)
    {
        foreach (string tag in collisionTags)
        {
            if (collision.CompareTag(tag))
            {
                Entity entity;
                if (entity = collision.GetComponent<Entity>())
                {
                    if (HasProperty(EntityProperty.DamageWhenCollide))
                        entity.Hurt(damage);
                    if (HasProperty(EntityProperty.AddMoneyWhenCollide))
                        entity.money += money;
                }
                if (HasProperty(EntityProperty.DieWhenCollide))
                    Die();
                if (HasProperty(EntityProperty.SpawnDamagePopup))
                    ObjectPooler.Spawn<Entity>(PoolType.DamagePopup, transform.position).InitDamagePopup(damage, HasProperty(EntityProperty.IsCritical));
            }
        }
    }
    
    public bool IsInRange(float range, Vector2 targetPos)
    {
        return (targetPos - (Vector2)transform.position).sqrMagnitude < range * range;
    }
    
    public bool IsInRange(Vector2 range, Vector2 targetPos)
    {
        Vector2 targetDir = MathUtils.Abs(targetPos - (Vector2)transform.position);
        return targetDir.x < range.x && targetDir.y < range.y;
    }
    
    public bool IsInRangeY(float range, Vector2 targetPos)
    {
        return Mathf.Abs(targetPos.y - transform.position.y) < range;
    }
    
    void CalculateMoveRegion()
    {
        switch (regionType)
        {
            case MoveRegionType.Ground:
            {
                moveRegion = GameManager.CalculateMoveRegion(transform.position, spriteExtents, -transform.up.y);
            } break;
            case MoveRegionType.Vertical:
            {
                Debug.Assert(transform.up.y == 1);
                if (GameManager.GetGroundPos(transform.position, spriteExtents, -1f, out _, out Vector3Int groundPos))
                {
                    moveRegion = new Rect(new Vector2(transform.position.x, groundPos.y + spriteExtents.y), Vector2.up * verticalHeight);
                    GameDebug.DrawBox(new Rect((Vector3)groundPos, spriteExtents), Color.red);
                }
            } break;
        }
    }
    
#if !NEW_VFX
    void PlayVFX(EntityVFX vfx) => VFXManager.PlayVFX(vfx, data, this);
#endif
}