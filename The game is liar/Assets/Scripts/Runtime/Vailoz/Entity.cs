//#define SCRIPTABLE_VFX
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

public enum EntityState
{
    None,
    
    Jumping,
    Falling,
    Landing,
    
    StartMoving,
    StopMoving,
    
    StartAttack,
    StartCooldown,
    
    OnSpawn,
    OnHit,
    OnDeath,
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
        Debug.Log(ability.type);
        ability.flags.SetProperty(AbilityFlag.CanExecute, false);
        float cooldownTime = 0;
        this.moveType = MoveType.None;
        float timer = 0;
        ability.vfx.canStop = () => ability.flags.HasProperty(AbilityFlag.CanExecute);
        PlayVFX(ability.vfx);
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
                    testProperties[0].SetProperty(VFXProperty.StartTrailing, false);
                    StartFalling(false);
                    yield break;
                }
            } break;
            case AbilityType.Teleport:
            {
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
                        bool insideWall = GameUtils.BoxCast(destination + offset + new Vector3(0, .1f * playerUp), sr.bounds.size, Color.green);
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
            }
            break;
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
        currentAbility = MathUtils.LoopIndex(currentAbility + 1, abilities.Length, true);
        
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
    public Property<VFXProperty>[] testProperties;
    public EntityAbility[] abilities;
    private int currentAbility;
    
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
    
    public AudioType footstepAudio;
    public RangedFloat timeBtwFootsteps;
    private float timeBtwFootstepsValue;
    
    private Vector2 velocity;
    private Rigidbody2D rb;
    private Collider2D cd;
    private float speedY;
    private Vector2 targetDir;
    private Vector2 targetPos;
    private Vector2 offsetDir;
    private EntityState state;
    
    [Header("Effects")]
    public Material whiteMat;
    public ParticleSystem leftDust;
    public ParticleSystem rightDust;
    public VFXCollection vfx;
    public EntityVFX spawnVFX, deathVFX, hurtVFX;
    
    private TMPro.TextMeshPro text;
    private TrailRenderer trail;
    private Animator anim;
    private SpriteRenderer sr;
    private Vector2 spriteExtents;
    
    public void OnObjectInit()
    {
        if (HasProperty(EntityProperty.UsePooling))
        {
            Init();
            deathVFX.done += () => gameObject.SetActive(false);
        }
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
            
            timeBtwFootstepsValue = 0;
            
            velocity = Vector2.zero;
            speedY = 0;
            state = EntityState.OnSpawn;
            
            // NOTE: weaponStat, collisionTags, dRotate, maxFallingSPeed, spring, fallDir?
        }
        
        CalculateMoveRegion();
        if (HasProperty(EntityProperty.FallingOnSpawn))
            StartFalling(true);
        
        if (HasProperty(EntityProperty.DieAfterEffect))
        {
            float animTime = 0;
            if (anim)
            {
                sr.enabled = true;
                AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
                anim.Play(state.shortNameHash);
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
        aliveTime = Mathf.Max(aliveTime, HasProperty(EntityProperty.DieAfterMoveTime) ? moveTime : 0);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if (!HasProperty(EntityProperty.UsePooling) && !HasProperty(EntityProperty.CustomInit))
        {
            Init();
            OnObjectSpawn(null);
            deathVFX.done += () => Destroy(this);
        }
    }
    
#region Initialize
    void Init()
    {
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<Collider2D>();
        text = GetComponent<TMPro.TextMeshPro>();
        trail = GetComponent<TrailRenderer>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        
        if (rb && maxFallingSpeed != 0)
        {
            float drag = MathUtils.GetDragFromAcceleration(Mathf.Abs(Physics2D.gravity.y * rb.gravityScale), maxFallingSpeed);
            Debug.Assert(drag > 0, drag);
            rb.drag = drag;
        }
        if (whiteMat)
            whiteMat = Instantiate(whiteMat);
        if (sr)
            spriteExtents = sr.bounds.extents;
        
        if (HasProperty(EntityProperty.SpawnCellWhenDie))
            deathVFX.done += () =>
        {
            int dropValue = valueRange.randomValue;
            for (int i = 0; i < dropValue; i++)
                ObjectPooler.Spawn(PoolType.Cell, transform.position);
        };
        
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
        
        state = EntityState.OnSpawn;
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
        text.text = damage.ToString();
        spawnVFX = new EntityVFX
        {
            properties = new Property<VFXProperty>(VFXProperty.ScaleOverTime, VFXProperty.FadeTextWhenDone),
            scaleTime = moveTime / 2, scaleOffset = Vector2.one / 2, fadeTime = 1f / 3f,
            fontSize = isCritical ? 3f : 2.5f, textColor = isCritical ? Color.red : Color.white,
        };
        PlayVFX(spawnVFX);
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
    
    public void Shoot(bool isCritical)
    {
        Entity bullet = ObjectPooler.Spawn<Entity>(PoolType.Bullet_Normal, transform.position, transform.eulerAngles);
        bullet.InitBullet(isCritical ? stat.damage : stat.critDamage, isCritical, false);
    }
#endregion
    
    /*struct EntityTrigger
    {
        public Property<TriggerFlag> flags;
        public int healthToExecute;
        public Vector2 distanceToExecute;
        
        public float bufferTime;
    };
    
    enum ActionState
    {
        None,
        Charge,
        Execute,
        //Exit,
        Cooldown,
    }
    
    class EntityAction
    {
        public EntityTrigger trigger;
        public Property<ActionFlag> flags;
        
        public ActionState state;
        public ActionType type; // None, Move, Jump, Flip, Teleport, Explode
        
        public float chargeTime;
        public float duration;
        //public float exitTime;
        public float cooldownTime;
        
        public int damage;
        public float range;
        public float speed
        {
            get => duration / range;
            set => range = duration / value;
        }
    };
    
    EntityAction[] actions;
    private int currentAction;
    
    void UpdateAction()
    {
        foreach (EntityAction action in actions)
        {
            
        }
    }*/
    
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
        bool isGrounded = SetProperty(EntityProperty.IsGrounded, GameUtils.GroundCheck(transform.position, spriteExtents, -transform.up.y, Color.red));
        
        if (abilities.Length > 0 &&
            abilities[currentAbility].flags.HasProperty(AbilityFlag.CanExecute) &&
            CanUseAbility(abilities[currentAbility]))
            StartCoroutine(UseAbility(abilities[currentAbility], moveType, targetType, speed));
        
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
        
        {
            bool canShoot = false;
            bool canReload = false;
            switch (attackTrigger)
            {
                case AttackTrigger.MouseInput:
                {
                    if (!HasProperty(EntityProperty.IsReloading))
                    {
                        canShoot  = ammo > 0         && GameInput.GetInput(InputType.Shoot);
                        canReload = ammo < stat.ammo && GameInput.GetInput(InputType.Reload);
                    }
                } break;
            }
            
            if (canShoot && Time.time > attackDuration.max)
            {
                ammo--;
                attackDuration.max = Time.time + stat.timeBtwShots;
                state = EntityState.StartAttack;
                
                bool isCritical = Random.value < stat.critChance;
                float rot = attackDuration.range > 0 ? (Mathf.PerlinNoise(0, attackDuration.range) * 2f - 1f) * 15f : 0;
                Shoot(isCritical);
            }
            else if ((ammo == 0 && canShoot) || canReload)
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
        
        RotateEntity(transform, rotateType, dRotate, velocity.x);
        Vector2 prevVelocity = velocity;
        MoveEntity();
        
        if (HasProperty(EntityProperty.ClampToMoveRegion))
            transform.position = MathUtils.Clamp(transform.position, moveRegion.min, moveRegion.max, transform.position.z);
        //GameDebug.DrawBox(moveRegion, Color.green);
        
        
        bool startJumping = jumpPressedRemember >= 0 && groundRemember >= 0;
        {
            EntityVFX playerVFX = new EntityVFX
            {
                shakeMode = ShakeMode.PlayerJump,
                waitTime = .25f,
                particles = new ParticleSystem[] { velocity.x >= 0 ? leftDust : null, velocity.x <= 0 ? rightDust : null },
            };
            // Start jumping
            if (startJumping)
            {
                state = EntityState.Jumping;
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
                state = EntityState.Landing;
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
                state = EntityState.Falling;
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
                        {
                            state = EntityState.StartMoving;
                            playerVFX.nextAnimation = "Move";
                        }
                        else if (velocity.x == 0)
                        {
                            state = EntityState.StopMoving;
                            playerVFX.nextAnimation = "Idle";
                        }
                    }
                }
            }
            
#if !SCRIPTABLE_VFX
            // NOTE: Currently, only the player has a jumping/landing/falling VFX, but that will probably change soon.
            // When that happens, remember to abstract this code out. Currently, we have a check that only the player can call PlayVFX here.
            if (GameManager.player == this)
                PlayVFX(playerVFX);
#else
            if (vfx)
                foreach (var effect in vfx.items[state])
                StartCoroutine(PlayVFX(effect));
#endif
            state = EntityState.None;
        }
    }
    
    public IEnumerator PlayVFX(VFX vfx)
    {
        if (vfx == null)
            yield break;
        Debug.Log(vfx.name);
        
        if (vfx.timeline.min > 0)
            yield return new WaitForSeconds(vfx.timeline.min);
        System.Action after = () => { };
        IEnumerator[] enumerators = new IEnumerator[4];
        int enumeratorCount = 0;
        
        // Position/Scale
        {
            float duration = vfx.flags.HasProperty(VFXFlag.OverTime) ? vfx.timeline.range : 0;
            if (vfx.flags.HasProperty(VFXFlag.OffsetPosition))
                Offset(() => transform.position, p => transform.position = p);
            if (vfx.flags.HasProperty(VFXFlag.OffsetScale) && vfx.type != VFXType.Trail)
                Offset(() => transform.localScale, s => transform.localScale = s);
            
            void Offset(System.Func<Vector3> getter, System.Action<Vector3> setter)
            {
                StartCoroutine(ChangeOverTime(p => setter(p), getter(), getter() + (Vector3)vfx.offset, duration));
                after += () => StartCoroutine(ChangeOverTime(p => setter(p), getter(), getter() - (Vector3)vfx.offset, vfx.stayTime));
                //enumerators[enumeratorCount++] = ChangeOverTime(p => setter(p), getter(), getter() - (Vector3)vfx.offset, vfx.stayTime);
                
                static IEnumerator ChangeOverTime(System.Action<Vector3> setValue, Vector3 startValue, Vector3 endValue, float decreaseTime)
                {
                    if (decreaseTime > 0)
                    {
                        float t = 0;
                        while (t <= 1)
                        {
                            setValue(Vector3.Lerp(startValue, endValue, t));
                            t += Time.deltaTime / decreaseTime;
                            yield return null;
                        }
                    }
                    setValue(endValue);
                    //Debug.Log($"{vfx.name}: {Time.frameCount}, {startValue}, {endValue}");
                }
            }
        }
        
        // Rotation
        after += () =>
        {
            Vector3 rotation = Vector3.zero;
            if (vfx.flags.HasProperty(VFXFlag.FlipX))
                rotation.x = 180;
            if (vfx.flags.HasProperty(VFXFlag.FlipY))
                rotation.y = 180;
            if (vfx.flags.HasProperty(VFXFlag.FlipZ))
                rotation.z = 180;
            transform.Rotate(rotation);
        };
        
        // Animation
        if (anim)
        {
            if (!string.IsNullOrEmpty(vfx.animation))
                anim.Play(vfx.animation);
            if (vfx.flags.HasProperty(VFXFlag.StopAnimation))
                anim.speed = 0;
            if (vfx.flags.HasProperty(VFXFlag.ResumeAnimation))
                after += () => anim.speed = 1; // NOTE: Maybe resume animation instantly
        }
        
        // Other
        {
            if (vfx.flags.HasProperty(VFXFlag.StopTime))
                StartCoroutine(GameUtils.StopTime(vfx.timeline.range));
            if (vfx.flags.HasProperty(VFXFlag.ToggleCurrent))
                gameObject.SetActive(!gameObject.activeSelf);
            
            ParticleEffect.instance.SpawnParticle(vfx.particleType, transform.position, vfx.size);
            AudioManager.PlayAudio(vfx.audio);
            CameraSystem.instance.Shake(vfx.shakeMode);
            CameraSystem.instance.Shock(vfx.speed, vfx.size);
            
            if (vfx.pools != null)
                foreach (PoolType pool in vfx.pools)
                ObjectPooler.Spawn(pool, transform.position);
        }
        
        switch (vfx.type)
        {
            case VFXType.Camera:
            {
                StartCoroutine(CameraSystem.instance.Flash(vfx.timeline.range, vfx.color.a));
            }
            break;
            case VFXType.Flash:
            {
                StartCoroutine(Flashing(sr, whiteMat, vfx.color, vfx.timeline.range, vfx.stayTime));
                
                static IEnumerator Flashing(SpriteRenderer sr, Material whiteMat, Color color, float duration, float flashTime)
                {
                    if (whiteMat == null)
                        yield break;
                    
                    whiteMat.color = color;
                    Debug.Log("Start flashing");
                    
                    while (duration > 0)
                    {
                        float currentTime = Time.time;
                        Material defMat = sr.material;
                        sr.material = whiteMat;
                        yield return new WaitForSeconds(flashTime);
                        
                        sr.material = defMat;
                        yield return new WaitForSeconds(flashTime);
                        duration -= Time.time - currentTime;
                    }
                    
                    whiteMat.color = Color.white;
                    Debug.Log("End flashing");
                }
            }
            break;
            case VFXType.Fade:
            {
                StartCoroutine(DecreaseOverTime(alpha => sr.color += new Color(0, 0, 0, alpha - sr.color.a), vfx.color.a, vfx.timeline.range));
            }
            break;
            case VFXType.Trail:
            {
                StartCoroutine(EnableTrail(trail, vfx.timeline.range, vfx.stayTime));
                if (vfx.flags.HasProperty(VFXFlag.OffsetScale))
                    after += () => StartCoroutine(DecreaseOverTime(width => trail.widthMultiplier = width, trail.widthMultiplier, vfx.stayTime));
                //enumerators[enumeratorCount++] = DecreaseOverTime(width => trail.widthMultiplier = width, trail.widthMultiplier, vfx.stayTime);
                if (vfx.flags.HasProperty(VFXFlag.FadeOut))
                {
                    // TODO: Fade trail
                }
                
                static IEnumerator EnableTrail(TrailRenderer trail, float emitTime, float stayTime)
                {
                    trail.enabled = true;
                    trail.emitting = true;
                    yield return new WaitForSeconds(emitTime);
                    
                    trail.emitting = false;
                    yield return new WaitForSeconds(stayTime);
                    
                    trail.Clear();
                    trail.emitting = true;
                    trail.enabled = false;
                }
            }
            break;
            case VFXType.Text:
            {
                TMPro.TextMeshPro text = GetComponent<TMPro.TextMeshPro>();
                if (vfx.color != Color.clear)
                    text.color = vfx.color;
                if (vfx.size != 0)
                    text.fontSize = vfx.size;
                if (vfx.flags.HasProperty(VFXFlag.FadeOut))
                    after += () => StartCoroutine(DecreaseOverTime(alpha => text.alpha = alpha, text.alpha, vfx.stayTime));
                //enumerators[enumeratorCount++] = DecreaseOverTime(alpha => text.alpha = alpha, text.alpha, vfx.stayTime);
            }
            break;
        }
        
        yield return new WaitForSeconds(vfx.timeline.range);
        after();
        /*Coroutine[] coroutines = new Coroutine[enumeratorCount];
        for (int i = 0; i < enumeratorCount; i++)
            coroutines[i] = StartCoroutine(enumerators[i]);
        foreach (Coroutine coroutine in coroutines)
        {
            yield return coroutine;
        }*/
        
        // TODO: Maybe change this to an offset-based
        static IEnumerator DecreaseOverTime(System.Action<float> setValue, float startValue, float decreaseTime)
        {
            if (decreaseTime > 0)
            {
                float t = 0;
                while (t <= 1)
                {
                    setValue(Mathf.Lerp(startValue, 0, t));
                    t += Time.deltaTime / decreaseTime;
                    yield return null;
                }
                yield return new WaitForEndOfFrame();
                setValue(startValue);
            }
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
            case TargetType.Input:
            {
                targetDir = new Vector2(GameInput.GetAxis(AxisType.Horizontal), GameInput.GetAxis(AxisType.Vertical));
            } break;
            case TargetType.Player:
            {
                targetPos = GameManager.player.transform.position;
            } goto case TargetType.Target;
            case TargetType.Random:
            {
                targetDir = MathUtils.RandomVector2();
            } break;
            case TargetType.MoveDir:
            {
                targetDir = transform.right;
            } break;
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
            case MoveType.None:
            {
                velocity = Vector2.zero;
            } break;
            case MoveType.Run:
            {
                targetDir.x = MathUtils.Sign(targetDir.x);
                targetDir.y = 0;
                velocity = new Vector2(targetDir.x * speed, rb.velocity.y);
            } break;
            case MoveType.Fly:
            {
                velocity = targetDir.normalized * speed;
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
    
    static void RotateEntity(Transform transform, RotateType rotateType, float dRotate, float velocityX)
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
                transform.rotation = GameManager.player.fallRemember > 0 ? player.rotation : MathUtils.GetQuaternionFlipY(mouseDir, player.up.y);
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
#if !SCRIPTABLE_VFX
        Hurt(0);
#else
        VFX vfx1 = CreateTestVFX("vfx1", new RangedFloat(.0f, .5f), new Vector2(-.25f, .25f));
        VFX vfx2 = CreateTestVFX("vfx2", new RangedFloat(.3f, .8f), new Vector2(.25f, -.25f));
        
        StartCoroutine(PlayVFX(vfx1));
        StartCoroutine(PlayVFX(vfx2));
        
        static VFX CreateTestVFX(string name, RangedFloat timeline, Vector2 offset)
        {
            VFX vfx = ScriptableObject.CreateInstance<VFX>();
            vfx.name = name;
            vfx.offset = offset;
            vfx.flags.SetProperty(VFXFlag.OffsetScale, true);
            vfx.timeline = timeline;
            return vfx;
        }
#endif
    }
    
    public void Hurt(int damage)
    {
        if (HasProperty(EntityProperty.CanBeHurt))
        {
            health -= damage;
            
            // TODO: Replace this with a stat
            moveType = MoveType.None;
            rotateType = RotateType.None;
            SetProperty(EntityProperty.CanBeHurt, false);
            if (health > 0)
            {
                state = EntityState.OnHit;
                //PlayVFX(hurtVFX);
            }
            else
                Die();
        }
    }
    
    void Die()
    {
        state = EntityState.OnDeath;
        PlayVFX(deathVFX);
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
    
    bool IsInRange(float range, Vector2 targetPos)
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
    
#region VFX
    public enum VFXProperty
    {
        StopAnimation,
        ChangeEffectObjBack,
        ScaleOverTime,
        FadeTextWhenDone,
        StartTrailing,
        DecreaseTrailWidth,
        PlayParticleInOrder,
        FlipX,
        FlipY,
        FlipZ,
    }
    
    [System.Serializable]
    public class EntityVFX
    {
        public Property<VFXProperty> properties;
        
        public System.Action done;
        public System.Func<bool> canStop;
        public string nextAnimation;
        public GameObject effectObj;
        public ParticleSystem[] particles;
        
        [Header("Time")]
        public float waitTime;
        public float scaleTime;
        public float rotateTime;
        
        [Header("Trail Effect")]
        public float trailEmitTime;
        public float trailStayTime;
        
        [Header("Flashing")]
        public float flashTime;
        public float flashDuration;
        public Color triggerColor;
        
        [Header("Text Effect")]
        public Color textColor;
        public float fontSize;
        
        [Header("Camera Effect")]
        public float stopTime;
        public float trauma;
        public ShakeMode shakeMode;
        public float shockSpeed;
        public float shockSize;
        public float camFlashTime;
        public float camFlashAlpha;
        
        [Header("After Fade")]
        public float alpha;
        public float fadeTime;
        
        [Header("Explode Particle")]
        public float range;
        public ParticleType particleType;
        
        [Header("Other")]
        public AudioType audio;
        public PoolType poolType;
        public Vector2 scaleOffset;
    }
    
    void PlayVFX(EntityVFX vfx)
    {
        if (vfx == null)
            return;
        Debug.Log(name + ": " + state, this);
        
        if (vfx.properties.HasProperty(VFXProperty.ScaleOverTime))
            StartCoroutine(ScaleOverTime(transform, vfx.scaleTime, vfx.scaleOffset));
        else
            transform.localScale += (Vector3)vfx.scaleOffset;
        
        if (!string.IsNullOrEmpty(vfx.nextAnimation))
            anim.Play(vfx.nextAnimation);
        if (vfx.properties.HasProperty(VFXProperty.StopAnimation))
            anim.speed = 0;
        
        if (vfx.textColor != Color.clear)
            text.color = vfx.textColor;
        if (vfx.fontSize != 0)
            text.fontSize = vfx.fontSize;
        
        float totalParticleTime = 0;
        float particleCount = vfx.particles?.Length ?? 0;
        for (int i = 0; i < particleCount; ++i)
        {
            if (vfx.particles[i])
            {
                this.InvokeAfter(totalParticleTime, () => vfx.particles[i].Play(), true);
                if (vfx.properties.HasProperty(VFXProperty.PlayParticleInOrder))
                    totalParticleTime += vfx.particles[i].main.duration;
            }
        }
        
        if (vfx.effectObj)
            vfx.effectObj.SetActive(!vfx.effectObj.activeSelf);
        
        StartCoroutine(CameraSystem.instance.Flash(vfx.camFlashTime, vfx.camFlashAlpha));
        CameraSystem.instance.Shake(vfx.shakeMode, null);//, vfx.trauma == 0 ? 1 : vfx.trauma);
        CameraSystem.instance.Shock(vfx.shockSpeed, vfx.shockSize);
        
        AudioManager.PlayAudio(vfx.audio);
        ObjectPooler.Spawn(vfx.poolType, transform.position);
        ParticleEffect.instance.SpawnParticle(vfx.particleType, transform.position, vfx.range);
        
        StartCoroutine(GameUtils.StopTime(vfx.stopTime));
        StartCoroutine(Flashing(sr, whiteMat, vfx.triggerColor, vfx.flashDuration, vfx.flashTime, vfx.canStop));
        float x = vfx.properties.HasProperty(VFXProperty.FlipX) ? 180 : 0;
        float y = vfx.properties.HasProperty(VFXProperty.FlipY) ? 180 : 0;
        float z = vfx.properties.HasProperty(VFXProperty.FlipZ) ? 180 : 0;
        if (x != 0 || y != 0 || z != 0)
            this.InvokeAfter(vfx.rotateTime, () => transform.Rotate(new Vector3(x, y, z)));
        
        this.InvokeAfter(Mathf.Max(vfx.flashDuration, totalParticleTime, vfx.scaleTime), () =>
                         {
                             if (vfx.properties.HasProperty(VFXProperty.FadeTextWhenDone))
                                 StartCoroutine(FadeText(text, vfx.alpha, vfx.fadeTime));
                             else
                                 StartCoroutine(Flashing(sr, whiteMat, new Color(1, 1, 1, vfx.alpha), vfx.fadeTime, vfx.fadeTime, vfx.canStop));
                             
                             if (vfx.properties.HasProperty(VFXProperty.StartTrailing))
                                 StartCoroutine(EnableTrail(trail, vfx.trailEmitTime, vfx.trailStayTime));
                             if (vfx.properties.HasProperty(VFXProperty.DecreaseTrailWidth))
                                 this.InvokeAfter(vfx.trailEmitTime, () => StartCoroutine(DecreaseTrailWidth(trail, vfx.trailStayTime)));
                             
                             this.InvokeAfter(Mathf.Max(vfx.fadeTime, vfx.trailEmitTime + vfx.trailStayTime) + vfx.waitTime, () =>
                                              {
                                                  if (!vfx.properties.HasProperty(VFXProperty.ScaleOverTime))
                                                      transform.localScale -= (Vector3)vfx.scaleOffset;
                                                  if (vfx.properties.HasProperty(VFXProperty.ChangeEffectObjBack))
                                                      vfx.effectObj.SetActive(!vfx.effectObj.activeSelf);
                                                  if (anim)
                                                      anim.speed = 1;
                                                  vfx.done?.Invoke();
                                              });
                         });
        
        static IEnumerator Flashing(SpriteRenderer sr, Material whiteMat, Color color, float duration, float flashTime, System.Func<bool> canStop)
        {
            if (whiteMat == null)
                yield break;
            
            whiteMat.color = color;
            
            while (duration > 0)
            {
                if (canStop?.Invoke() ?? false)
                    break;
                
                float currentTime = Time.time;
                Material defMat = sr.material;
                sr.material = whiteMat;
                yield return new WaitForSeconds(flashTime);
                
                sr.material = defMat;
                yield return new WaitForSeconds(flashTime);
                duration -= Time.time - currentTime;
            }
            
            whiteMat.color = Color.white;
        }
        
        static IEnumerator ScaleOverTime(Transform transform, float duration, Vector3 scaleOffset)
        {
            while (duration > 0)
            {
                duration -= Time.deltaTime;
                transform.localScale += scaleOffset * Time.deltaTime;
                yield return null;
            }
            
            while (transform.gameObject.activeSelf)
            {
                transform.localScale -= scaleOffset * Time.deltaTime;
                yield return null;
            }
        }
        
        static IEnumerator FadeText(TMPro.TextMeshPro text, float alpha, float fadeTime)
        {
            float dAlpha = (text.alpha - alpha) / fadeTime;
            while (text.alpha > alpha)
            {
                text.alpha -= dAlpha * Time.deltaTime;
                yield return null;
            }
        }
        
        static IEnumerator EnableTrail(TrailRenderer trail, float emitTime, float stayTime)
        {
            trail.enabled = true;
            trail.emitting = true;
            yield return new WaitForSeconds(emitTime);
            
            trail.emitting = false;
            yield return new WaitForSeconds(stayTime);
            
            trail.Clear();
            trail.emitting = true;
            trail.enabled = false;
        }
        
        static IEnumerator DecreaseTrailWidth(TrailRenderer trail, float decreaseTime)
        {
            float startWidth = trail.widthMultiplier;
            float startTime = decreaseTime;
            while (decreaseTime > 0)
            {
                trail.widthMultiplier = decreaseTime / startTime * startWidth;
                decreaseTime -= Time.deltaTime;
                yield return null;
            }
            trail.widthMultiplier = startWidth;
        }
    }
#endregion
}

/*void PlayVFX(EntityVFX vfx)
{
    // Sample Code

    // Player hurt
    EntityVFX vfx = new EntityVFX
    {
        stopTime = .15f,
        flashDuration = .1f,
        flashTime = .1f,
        triggerColor = Color.white,
        camFlashTime = .15f,
        camFlashAlpha = .8f,
        stopAnimation = true,
        //effectObj = hitEffect, revertObj = true
    };

    // Player death
    vfx = new EntityVFX
    {
        audio = AudioType.Player_Death,
        nextAnimation = "Death",
        effectObj = transform.GetChild(0).gameObject,
        stopTime = 2,
        particles = new ParticleSystem[] { deathBurstParticle, deathFlowParticle },
    };

    // Player flip
    transform.position -= GetPosOnGround();
    vfx = new EntityVFX
    {
        shakeMode = ShakeMode.Medium,
        audio = AudioType.Player_Jump,
        scaleOffset = new Vector2(-.25f, .25f),
        duration = .2f,
        particles = checking() ? ...
    };

    // Bullet hit effect
    ObjectPooler.Spawn(damagePopup, collision.transform.position, Quaternion.identity).GetComponent<MovingEntity>().InitDamagePopup(damage);
    vfx = new EntityVFX
    {
        poolType = PoolType.VFX_Destroyed_Bullet,
        audio = AudioType.Weapon_Hit_Wall,
        effectObj = gameObject,
    };

    // Explosion
    vfx = new EntityVFX
    {
        audio = AudioType.Enemy_Explosion,
        shakeMode = ShakeMode.Strong,
        smoothFunc = MathUtils.SmoothStart3,
        shockSpeed = 2,
        shockSize = .1f,
        particleType = ParticleType.Explosion, range = explodeRange,
        stopTime = .05f,
    };
    if (IsInRange(explodeRange))
        player.Hurt(abilityDamage);
    int dropValue = moneyDrop.randomValue;
    for (int i = 0; i < dropValue; i++)
        ObjectPooler.Spawn(PoolType.Cell, transform.position, Quaternion.identity);
    numberOfEnemiesAlive--;
    Destroy(gameObject);

    // Enemy Death
    vfx = new EntityVFX
    {
        shakeMode = ShakeMode.Medium,
        poolType = PoolType.VFX_Destroyed_Enemy,
        stopTime = .05f,
        audio = AudioType.Enemy_Death,
        // TODO: Spawn splash of blood and small pieces
    };
    int dropValue = moneyDrop.randomValue;
    for (int i = 0; i < dropValue; i++)
        ObjectPooler.Spawn(PoolType.Cell, transform.position, Quaternion.identity);
    numberOfEnemiesAlive--;
    Destroy(gameObject);

    // Enemy Hurt
    vfx = new EntityVFX
    {
        audio = AudioType.Player_Hurt,
        stopTime = .02f,
        flashDuration = .1f,
        flashTime = .1f,
        triggerColor = hurtColor,
    };

    // Damage popup
    vfx = new EntityVFX
    {
        duration = 1,
        scaleOffset = Vector2.one * .5f,
        alpha = 3,
        done = ,
    };

    // ----------------------------------------

    GameInput.EnableAllInputs(false);
    controller.audioManager.PlayAudio(AudioType.Player_Death);
    anim.Play("Death");
    transform.GetChild(0).gameObject.SetActive(false);
    yield return new WaitForSeconds(.5f);

    Time.timeScale = 0;
    deathBurstParticle.Play();
    yield return new WaitForSecondsRealtime(2);

    Time.timeScale = 1;
    deathFlowParticle.Play();
    yield return new WaitForSeconds(deathFlowParticle.main.duration);
    // TODO: Replay and enable all inputs

    // ----------------------------------------

    anim.speed = 0;
    sr.material = hurtMat;
    hitEffect.SetActive(true);
    transform.localScale = new Vector2(.75f, 1f);

    Time.timeScale = 0f;
    StartCoroutine(cam.Flash(.15f, .8f));
    yield return new WaitForSecondsRealtime(.15f);
    Time.timeScale = 1f;

    yield return new WaitForSeconds(.1f);
    sr.material = defMat;
    hitEffect.SetActive(false);
    transform.localScale = new Vector2(1f, 1f);

    Color temp = sr.color;
    temp.a = invincibleOpacity;
    sr.color = temp;

    yield return new WaitForSeconds(invincibleTime);

    temp.a = 1;
    sr.color = temp;
    anim.speed = 1;

    // ----------------------------------------

    CameraShake.instance?.Shake(ShakeMode.Medium, trauma: .4f);
    PlayDust(-moveInput);
    audioManager.PlayAudio(isJumping ? AudioType.Player_Jump : AudioType.Player_Land);

    // Change Size
    transform.localScale = isJumping ? new Vector3(.75f, 1.25f) : new Vector3(1.25f, .75f);
    transform.position -= GetPosOnGround();

    StopCoroutine(resetSize);
    resetSize = this.InvokeAfter(.2f, () => {
        transform.localScale = new Vector3(1f, 1f);
        if (!isJumping)
            transform.position -= GetPosOnGround();
    });

    Vector3 GetPosOnGround()
    {
        float groundHeight = Physics2D.BoxCast(transform.position, new Vector2(spriteExtents.x / 2, 0.01f), 0, -transform.up, spriteExtents.y * 2,
                LayerMask.GetMask("Ground")).distance;
        Vector3 offset = new Vector3(0, groundHeight - spriteExtents.y * transform.localScale.y) * transform.up.y;
        Debug.DrawRay(transform.position, -transform.up * groundHeight, Color.blue);
        return offset;
    }

    // ----------------------------------------

    if (spawnDamagePopup)
        ObjectPooler.Spawn(damagePopup, collision.transform.position, Quaternion.identity).GetComponent<MovingEntity>().InitDamagePopup(damage);
    ObjectPooler.Spawn(PoolType.VFX_Destroyed_Bullet, transform.position, Quaternion.identity);
    audioManager.PlayAudio(AudioType.Weapon_Hit_Wall);
    gameObject.SetActive(false);

    // ----------------------------------------

    Charge();
    yield return new WaitForSeconds(abilityChargeTime);

    audioManager.PlaySfx(abilitySound);
    CameraShake.instance.Shake(cameraShakeMode, MathUtils.SmoothStart3);
    CameraShake.instance.Shock(2);
    // TODO: Change ParticleEffect to a singleton
    FindObjectOfType<ParticleEffect>().SpawnParticle(ParticleType.Explosion, transform.position, explodeRange);
    if (IsInRange(explodeRange))
        player.Hurt(abilityDamage);
    Die(true);

    // ----------------------------------------

    whiteMat.color = color;
    while (duration > 0)
    {
        float currentTime = Time.time;

        sr.material = whiteMat;
        yield return new WaitForSeconds(flashTime);

        sr.material = defMat;
        yield return new WaitForSeconds(flashTime);

        duration -= Time.time - currentTime;
    }

    // ----------------------------------------

    if (!explode)
    {
        CameraShake.instance.Shake(ShakeMode.Medium);
        ObjectPooler.Spawn(PoolType.VFX_Destroyed_Enemy, transform.position, Quaternion.identity);
    }

    // TODO: Spawn splash of blood and small pieces

    player.InvokeAfter(.3f, () => player.StartCoroutine(GameUtils.StopTime(.05f)));
    audioManager.PlayAudio(AudioType.Enemy_Death);
    int dropValue = moneyDrop.randomValue;
    for (int i = 0; i < dropValue; i++)
        ObjectPooler.Spawn(PoolType.Cell, transform.position, Quaternion.identity);
    numberOfEnemiesAlive--;
    Destroy(gameObject);

    // ----------------------------------------

    if (state == EnemyState.Invincible)
        return;
    // TODO: Have different hurt sound for enemies.
    audioManager.PlayAudio(AudioType.Player_Hurt);
    health.value -= damage;
    if (health.value > 0f)
        StartCoroutine(GameUtils.StopTime(.02f));
    StartCoroutine(Flashing(.1f, .1f, hurtColor));
}*/