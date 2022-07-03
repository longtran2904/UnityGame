using System.Collections;
using UnityEngine;

[System.Serializable]
public struct Property<T>
{
#if UNITY_EDITOR
    public string[] serializedEnumNames;
#endif
    public ulong[] properties;

    public Property(params T[] properties) : this()
    {
        this.properties = new ulong[(System.Enum.GetNames(typeof(T)).Length + 63) / 64];
        SetProperties(properties);
    }

    public void Init()
    {
        properties = new ulong[(System.Enum.GetNames(typeof(T)).Length + 63) / 64];
    }

    public bool HasProperty(T property)
    {
        int p = System.Convert.ToInt32(property);
        return (properties[p / 64] & (1ul << (p % 64))) != 0;
    }

    public void SetProperty(T property, bool set)
    {
        int p = System.Convert.ToInt32(property);
        if (set)
            properties[p / 64] |= 1ul << (p % 64);
        else
            properties[p / 64] &= ~(1ul << (p % 64));
    }

    public void SetProperties(params T[] properties)
    {
        foreach (T property in properties)
            SetProperty(property, true);
    }
}

public enum EntityProperty
{
    CanFlipGravity,
    CanBeHurt,
    OpenGameOverMenu,
    DamageWhenCollide,
    DieWhenCollide,
    DieAfterMoveTime,
    SpawnCellWhenDie,
    SpawnDamagePopup,
    ClampToMoveRegion,
    StartAtMinMoveRegion,
    IsCritical,
    UsePooling,
    AddMoneyWhenCollide,

    // Don't show in the inspector
    IsGrounded,
    AtEndOfMoveRegion,
    HasTargetOffset,

    Count
}

public class Entity : MonoBehaviour, IPooledObject
{
    // Move horizontally based on input
    // Jump
    // Flip gravity based on input
    // dash when near
    // explode when near/died
    // Teleport when player is far away
    // Move towards the player, teleport when hit a cliff

    public enum EntityStatProperty
    {
        AddSpeed,
        MulSpeed,
        AddDamage,
        MulDamage,

        //AimAtNextPos,
        FindNewStat,
        Interruptible,
        TillDeathDoUsPart,
        DamageNearbyEntities,
        DieAfterExecution,
        TeleportToTarget,
        StartFalling,
        StartMoving,
        FlipGraviy,

        PlayerInRange,
        EntityOutOfMoveRegion,
        LowHealth,
        EndRoom,
    }

    [System.Serializable]
    public class EntityStat
    {
        public Property<EntityStatProperty> properties;
        public int nextStat;

        [Header("Condition")]
        public int healthToExecute;
        public Vector2 distanceToExecute;

        [Header("Execution")]
        public float duration;
        public float chargeTime;
        public float groundRememberTime;
        public float jumpRememberTime;
        public float playerRememberTime;
        public RangedFloat affectedRange;

        public MoveType moveType;
        public TargetType targetType;
        public RotateType rotateType;
        public float speed;
        public float damage;
    }

    bool CanExecute(EntityStat stat)
    {
        bool canExecute = true;
        if (GameManager.player)
            canExecute &= stat.properties.HasProperty(EntityStatProperty.PlayerInRange) == IsInRange(stat.distanceToExecute, GameManager.player.transform.position);
        if (stat.properties.HasProperty(EntityStatProperty.EntityOutOfMoveRegion))
            canExecute &= HasProperty(EntityProperty.AtEndOfMoveRegion);
        if (stat.properties.HasProperty(EntityStatProperty.LowHealth))
            canExecute &= health <= stat.healthToExecute;

        return canExecute;
    }

    float MulAdd(float a, float b, bool mul, bool add)
    {
        if (mul && add)
            return a + a * b;
        else if (mul)
            return a * b;
        else if (add)
            return a + b;
        else
            return b;
    }

    IEnumerator ExecuteStat(EntityStat stat, float defaultSpeed, float defaultDamage)
    {
        if (stat == null)
            yield break;
        if (stat.properties.properties == null)
            stat.properties.Init();

        rotateType = stat.rotateType;
        targetType = stat.targetType;

        // vfx.Play();
        float timer = 0;
        moveType = MoveType.None;
        while (timer < stat.playerRememberTime)
        {
            if (stat.properties.HasProperty(EntityStatProperty.Interruptible))
                if (!CanExecute(stat))
                    goto END;
            yield return null;
            timer += Time.deltaTime;
        }
        yield return new WaitForSeconds(stat.chargeTime - stat.playerRememberTime);

        moveType = stat.moveType;
        speed = MulAdd(defaultSpeed, stat.speed, stat.properties.HasProperty(EntityStatProperty.MulSpeed), stat.properties.HasProperty(EntityStatProperty.AddSpeed));
        damage = (int)MulAdd(defaultDamage, stat.damage, stat.properties.HasProperty(EntityStatProperty.MulDamage), stat.properties.HasProperty(EntityStatProperty.AddDamage));


        if (stat.properties.HasProperty(EntityStatProperty.StartFalling))
        {
            rb.velocity = new Vector2(Random.Range(0f, 1f) * (MathUtils.RandomBool() ? 1f : -1f), Random.Range(0.5f, 1f)).normalized * speed;
            cd.isTrigger = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        else if (stat.properties.HasProperty(EntityStatProperty.StartMoving))
        {
            cd.isTrigger = true;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        /*else if (stat.properties.HasProperty(EntityStatProperty.FlipGraviy))
        {
            while (true)
            {
                float groundTime = 0;
                float jumpTime = 0;
                do
                {
                    yield return null;
                    groundTime -= Time.deltaTime;
                    jumpTime -= Time.deltaTime;
                    if (GroundCheck())
                        groundTime = stat.groundRememberTime;
                    if (GameInput.GetInput(InputType.Jump))
                        jumpTime = stat.jumpRememberTime;
                } while (groundTime < 0 || jumpTime < 0);
                rb.gravityScale *= -1;
            }
        }*/

        if (stat.properties.HasProperty(EntityStatProperty.TeleportToTarget))
        {
            float playerUp = GameManager.player.transform.up.y;
            Vector3 destination;
            {
                destination = targetPos;
                destination.y += (spriteExtents.y - GameManager.player.spriteExtents.y) * playerUp;

                float playerToEntityX = Mathf.Sign(targetPos.x - transform.position.x);
                float randomDistance = stat.affectedRange.randomValue * playerToEntityX;
                float minX = stat.affectedRange.min * playerToEntityX;

                // TODO: Maybe teleport opposite to where the player is heading or teleport to nearby platform
                if (!IsPosValid(randomDistance))
                    if (!IsPosValid(minX))
                        if (!IsPosValid(-randomDistance))
                            if (!IsPosValid(-minX))
                                yield break;

                bool IsPosValid(float offsetX)
                {
                    Vector3 offset = new Vector3(offsetX, 0);
                    bool onGround = BoxCast(destination + offset - new Vector3(0, spriteExtents.y * playerUp), new Vector2(spriteExtents.x, .1f), Color.yellow);
                    bool insideWall = BoxCast(destination + offset + new Vector3(0, .1f * playerUp), sr.bounds.size, Color.green);
                    if (onGround && !insideWall)
                    {
                        destination.x += offsetX;
                        return true;
                    }
                    return false;
                }
            }

            if (playerUp != transform.up.y)
                transform.Rotate(new Vector3(180, 0, 0), Space.World);

            yield return null;
            transform.position = destination;
            // TODO: Recalculate the moveRegion here or at the default stat
        }

        yield return new WaitForSeconds(stat.duration);

        if (stat.properties.HasProperty(EntityStatProperty.DamageNearbyEntities))
        {
            if (IsInRange(stat.affectedRange.randomValue, GameManager.player.transform.position))
                GameManager.player.Hurt(damage);
            // TODO: Setup deathVFX for explosion
        }

        if (stat.properties.HasProperty(EntityStatProperty.DieAfterExecution))
            Hurt(health);
        if (stat.properties.HasProperty(EntityStatProperty.TillDeathDoUsPart))
            yield break;

        END:
        currentStat = stat.nextStat;
        if (stat.properties.HasProperty(EntityStatProperty.FindNewStat))
        {
            int nextStat = currentStat;
            while (currentStat == nextStat)
            {
                yield return null;
                for (int i = 0; i < stats.Length; i++)
                    if (i != currentStat)
                        if (CanExecute(stats[i]))
                            currentStat = i;
            }
        }
    }

    [Header("Stat")]
    public Property<EntityProperty> properties;
    public EntityStat[] stats;
    public int health;
    public int damage;
    public int money;
    public string[] collisionTags;
    [MinMax(0, 10)] public RangedInt valueRange;
    private int currentStat;
    private int prevStat;

    [Header("Movement")]
    public MoveType moveType;
    public RotateType rotateType;
    public TargetType targetType;
    public float speed;
    public float dRotate;
    public float range;
    public Vector2 offsetTarget;

    public float moveTime;
    private float moveTimeValue;

    public float groundRememberTime;
    private float groundRemember;

    public float jumpPressedRememberTime;
    private float jumpPressedRemember;

    public AudioType footstepAudio;
    public RangedFloat timeBtwFootsteps;
    private float timeBtwFootstepsValue;

    private Rigidbody2D rb;
    private Collider2D cd;
    private Rect moveRegion;
    private Vector2 velocity;
    private float speedY;
    private Vector2 targetDir;
    private Vector2 targetPos;

    [Header("Effects")]
    public Material whiteMat;
    public ParticleSystem leftDust;
    public ParticleSystem rightDust;
    public EntityVFX deathVFX, hurtVFX;

    private ParticleEffect particle;
    private Camera cam;

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

    public void OnObjectSpawn()
    {
        InitOnSpawn();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!HasProperty(EntityProperty.UsePooling))
        {
            Init();
            InitOnSpawn();
            deathVFX.done += () => Destroy(this);
        }
    }

#region Initialize
    void InitOnSpawn()
    {
        prevStat = -1;
        currentStat = 0;
        moveTimeValue = moveTime + Time.time;
        /*if (HasProperty(EntityProperty.FallingOnSpawn))
        {
            velocity = new Vector2(Random.Range(-1, 1), Random.value).normalized * speed;
            StartMoving(false);
        }*/
    }

    void Init()
    {
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<Collider2D>();
        trail = GetComponent<TrailRenderer>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        if (whiteMat)
            whiteMat = Instantiate(whiteMat);
        if (sr)
            spriteExtents = sr.bounds.extents;

        particle = FindObjectOfType<ParticleEffect>();
        cam = Camera.main;

        if (HasProperty(EntityProperty.OpenGameOverMenu))
            deathVFX.done += () => { /* TODO: Open Game over menu after vfx */ };
        if (HasProperty(EntityProperty.SpawnCellWhenDie))
            deathVFX.done += () =>
            {
                int dropValue = valueRange.randomValue;
                for (int i = 0; i < dropValue; i++)
                    ObjectPooler.Spawn(PoolType.Cell, transform.position);
            };

        SetProperty(EntityProperty.CanBeHurt, true);
        hurtVFX.done += () =>
        {
            SetProperty(EntityProperty.CanBeHurt, true);
            prevStat = -1;
        };

        /*if (HasProperty(EntityProperty.MoveWhenClearRoom))
            GameInput.BindEvent(GameEventType.EndRoom, _ => StartMoving(true));*/

        for (int i = 0; i < stats.Length; ++i)
            if (stats[i].properties.HasProperty(EntityStatProperty.EndRoom))
                GameInput.BindEvent(GameEventType.EndRoom, _ => currentStat = i);
    }

    public void InitCamera(bool automatic, bool useSmoothDamp, Vector2 value, float waitTime)
    {
        if (stats == null || stats.Length < 2)
            stats = new EntityStat[2];

        SetProperty(EntityProperty.HasTargetOffset, !automatic);
        stats[0] = new EntityStat
        {
            properties = new Property<EntityStatProperty>(EntityStatProperty.FindNewStat),
            targetType = automatic ? TargetType.MoveRegion : TargetType.Player,
            speed = value.magnitude,
            moveType = useSmoothDamp ? MoveType.SmoothDamp : MoveType.Fly,
        };
        prevStat = -1;
        //targetType = automatic ? TargetType.MoveRegion : TargetType.Player;
        //speed = value.magnitude;
        //moveType = useSmoothDamp ? MoveType.SmoothDamp : MoveType.Fly;

        speed = value.x;
        speedY = value.y;

        stats[1] = new EntityStat { properties = new Property<EntityStatProperty>(EntityStatProperty.EntityOutOfMoveRegion), chargeTime = waitTime };
        GameInput.BindEvent(GameEventType.NextRoom, room => ToNextRoom(GameManager.GetBoundsFromRoom(room).ToRect()));

        void ToNextRoom(Rect roomRect)
        {
            moveRegion = roomRect;
            moveRegion.min += cam.HalfSize();
            moveRegion.max -= cam.HalfSize();
            if (automatic)
            {
                transform.position = moveRegion.min.Z(transform.position.z);
                SwitchTargetToMax(true, false);
            }
            Debug.Assert((moveRegion.xMin <= moveRegion.xMax) && (moveRegion.yMin <= moveRegion.yMax),
                $"Camera's limit is wrong: (Move region: {moveRegion}, Rect: {roomRect})");
        }
    }

    public void InitBullet(WeaponStat stat, bool isCritical, bool hitPlayer)
    {
        stats[0].damage = isCritical ? stat.critDamage : stat.damage;
        //damage = isCritical ? stat.critDamage : stat.damage;
        SetProperty(EntityProperty.IsCritical, isCritical);
        collisionTags[1] = hitPlayer ? "Player" : "Enemy";
    }

    public void InitDamagePopup(int damage, bool isCritical)
    {
        targetDir = Vector2.one;
        EntityVFX vfx = new EntityVFX
        {
            properties = new Property<VFXProperty>(VFXProperty.ScaleOverTime, VFXProperty.FadeTextWhenDone, VFXProperty.ChangeFontSize, VFXProperty.ChangeTextColor),
            scaleTime = moveTime / 2,
            scaleOffset = Vector2.one / 2,
            fadeTime = 1f / 3f,
            newText = damage.ToString(),
            fontSize = isCritical ? 3f : 2.5f,
            textColor = isCritical ? Color.red : Color.white,
        };
        PlayVFX(vfx);
    }

    /*void StartMoving(bool startMoving)
    {
        moveType = startMoving ? MoveType.Fly : MoveType.Custom;
        cd.isTrigger = startMoving;
        rb.bodyType = startMoving ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
    }*/
#endregion

    // Update is called once per frame
    void Update()
    {
        RotateEnemy(rotateType, dRotate, velocity.x);
        Vector2 prevVelocity = velocity;
        /*if (HasProperty(EntityProperty.MoveToNearPlayer))
            if (IsInRange(range, GameManager.player.transform.position))
                StartMoving(true);*/
        if (Time.time > moveTimeValue)
            if (HasProperty(EntityProperty.DieAfterMoveTime))
                Hurt(health);
        MoveEntity();

        if (prevStat != currentStat && currentStat < stats.Length)
        {
            StartCoroutine(ExecuteStat(stats[currentStat], speed, damage));
            prevStat = currentStat;
        }

        bool isGrounded = HasProperty(EntityProperty.IsGrounded);

        if (HasProperty(EntityProperty.CanFlipGravity))
        {
            bool wasGrounded = isGrounded;

            groundRemember -= Time.deltaTime;
            if (SetProperty(EntityProperty.IsGrounded, isGrounded = GroundCheck()))
                groundRemember = groundRememberTime;

            jumpPressedRemember -= Time.deltaTime;
            if (GameInput.GetInput(InputType.Jump))
                jumpPressedRemember = jumpPressedRememberTime;

            EntityVFX verticalVFX = new EntityVFX
            {
                shakeMode = ShakeMode.Medium,
                waitTime = .25f,
                trauma = .5f,
                particles = new ParticleSystem[] { velocity.x >= 0 ? leftDust : null, velocity.x <= 0 ? rightDust : null },
            };

            // Jumping
            if (jumpPressedRemember >= 0 && groundRemember >= 0)
            {
                jumpPressedRemember = 0;
                groundRemember = 0;
                rb.gravityScale *= -1;

                verticalVFX.audio = AudioType.Player_Jump;
                verticalVFX.scaleOffset = new Vector2(-.25f, .25f);
                this.InvokeAfter(.2f, () => transform.Rotate(180, 0, 0));
                PlayVFX(verticalVFX);
            }
            // Landing
            else if (!wasGrounded && isGrounded)
            {
                verticalVFX.audio = AudioType.Player_Land;
                verticalVFX.scaleOffset = new Vector2(.25f, -.25f);
                if (velocity.x != 0)
                    verticalVFX.nextAnimation = "Move";
                PlayVFX(verticalVFX);
            }
            // Start falling
            else if (wasGrounded && !isGrounded)
            {
                PlayVFX(new EntityVFX
                {
                    // TODO: Has a falling animation rather than the first frame of the idle one.
                    properties = new Property<VFXProperty>(VFXProperty.StopAnimation),
                    nextAnimation = "Idle"
                });
            }
        }

        if (isGrounded)
        {
            Vector2 deltaVelocity = velocity - prevVelocity;
            if (velocity.x != 0 && Time.time > timeBtwFootstepsValue)
            {
                timeBtwFootstepsValue = Time.time + timeBtwFootsteps.randomValue;
                AudioManager.PlayAudio(footstepAudio);
            }

            if (deltaVelocity.x != 0)
            {
                EntityVFX moveVFX = new EntityVFX
                {
                    particles = new ParticleSystem[] { deltaVelocity.x > 0 ? leftDust : rightDust },
                };
                if (prevVelocity.x == 0)
                    moveVFX.nextAnimation = "Move";
                else if (velocity.x == 0)
                    moveVFX.nextAnimation = "Idle";
                PlayVFX(moveVFX);
            }
        }

        if (HasProperty(EntityProperty.ClampToMoveRegion))
            transform.position = MathUtils.Clamp(transform.position, moveRegion.min, moveRegion.max, transform.position.z);
    }

    bool GroundCheck()
    {
        Vector2 boxSize = new Vector2(spriteExtents.x / 1.5f, 0.02f);
        Vector2 boxPos = transform.position - new Vector3(0, spriteExtents.y + boxSize.y + .075f) * Mathf.Sign(rb.gravityScale);
        return BoxCast(boxPos, boxSize, Color.red);
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
                    if (targetPos == moveRegion.max && IsInRange(.1f, moveRegion.max))
                        SwitchTargetToMax(false, true);
                    else if (targetPos == moveRegion.min && IsInRange(.1f, moveRegion.min))
                        SwitchTargetToMax(true, true);
                } goto case TargetType.Target;
            case TargetType.Target:
                {
                    if (HasProperty(EntityProperty.HasTargetOffset))
                        targetPos += GameInput.GetMouseDir() * offsetTarget;
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
            case MoveType.SmoothDamp:
                {
                    transform.position = MathUtils.SmoothDamp(transform.position, targetPos, ref velocity, new Vector2(speed, speedY), Time.deltaTime, transform.position.z);
                } return;
            case MoveType.Custom:
                return;
        }

        if (rb)
            rb.velocity = velocity;
        else
            transform.position += (Vector3)velocity * Time.deltaTime;
    }

    void SwitchTargetToMax(bool toMax, bool atEndOfMoveRegion)
    {
        targetPos = toMax ? moveRegion.max : moveRegion.min;
        velocity = Vector2.zero;
        SetProperty(EntityProperty.AtEndOfMoveRegion, atEndOfMoveRegion);
    }

    public bool CompleteCycle()
    {
        return HasProperty(EntityProperty.AtEndOfMoveRegion) && targetPos == moveRegion.max;
    }

    public enum RotateType
    {
        None,
        Player,
        MoveDir,
        Mouse,
        MouseX,
        Linear,
        EndRotation,

        Count
    }

    void RotateEnemy(RotateType rotateType, float dRotate, float velocityX)
    {
        float dirX = 0;
        switch (rotateType)
        {
            case RotateType.Player:
                {
                    dirX = GameManager.player.transform.position.x - transform.position.x;
                } break;
            case RotateType.MoveDir:
                {
                    dirX = velocityX;
                } break;
            case RotateType.Mouse:
                {
                    Vector2 difference = GameInput.GetDirToMouse(transform.position);
                    float rotZ = difference == Vector2.zero ? 0 : Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
                    transform.localRotation = Quaternion.Euler(0f, 0f, (difference.x >= 0 ? rotZ : 180f - rotZ) * transform.up.y);
                } break;
            case RotateType.MouseX:
                {
                    dirX = GameInput.GetDirToMouse(transform.position).x;
                } break;
            case RotateType.Linear:
                {
                    transform.Rotate(0, 0, dRotate * Time.deltaTime);
                } break;
        }
        
        if (dirX != 0 && Mathf.Sign(dirX) != Mathf.Sign(transform.right.x))
            transform.Rotate(0, 180, 0);
    }

    public void Hurt(int damage)
    {
        if (HasProperty(EntityProperty.CanBeHurt))
        {
            health -= damage;
            SetProperty(EntityProperty.CanBeHurt, false);

            // TODO: Replace this with a stat
            moveType = MoveType.None;
            rotateType = RotateType.None;
            PlayVFX(health <= 0 ? deathVFX : hurtVFX);
        }
    }

    public bool HasProperty(EntityProperty property)
    {
        //return (properties[(int)property / 64] & (1ul << ((int)property % 64))) != 0;
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
                    PlayVFX(deathVFX);
                if (HasProperty(EntityProperty.SpawnDamagePopup))
                    ObjectPooler.Spawn(PoolType.DamagePopup, transform.position).GetComponent<Entity>().InitDamagePopup(damage, HasProperty(EntityProperty.IsCritical));
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

    bool BoxCast(Vector2 pos, Vector2 size, Color color)
    {
        GameDebug.DrawBox(pos, size, color);
        return Physics2D.BoxCast(pos, size, 0, Vector2.zero, 0, LayerMask.GetMask("Ground"));
    }

#region VFX
    public enum VFXProperty
    {
        StopAnimation,
        ChangeEffectObjBack,
        ScaleOverTime,
        FadeTextWhenDone,
        ShockCamera,
        FlashCamera,
        StartTrailing,
        DecreaseTrailWidth,
        PlayParticleInOrder,
        ChangeTextColor,
        ChangeFontSize,

        Count
    }

    [System.Serializable]
    public class EntityVFX
    {
        public Property<VFXProperty> properties;

        public System.Action done;
        public string nextAnimation;
        public GameObject effectObj;
        public ParticleSystem[] particles;

        [Header("Time")]
        public float waitTime;
        public float scaleTime;

        [Header("Trail Effect")]
        public float trailEmitTime;
        public float trailStayTime;

        [Header("Flashing")]
        public float flashTime;
        public float flashDuration;
        public Color triggerColor;

        [Header("Text Effect")]
        public Color textColor;
        public string newText;
        public float fontSize;

        [Header("Camera Effect")]
        public float stopTime;
        public float trauma;
        public ShakeMode shakeMode;
        public SmoothFunc smoothFunc;
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
        if (vfx.properties.properties == null || vfx.properties.properties.Length < 1)
            vfx.properties.Init();

        if (vfx.properties.HasProperty(VFXProperty.ScaleOverTime))
            StartCoroutine(ScaleOverTime());
        else
            transform.localScale += (Vector3)vfx.scaleOffset;

        if (!string.IsNullOrEmpty(vfx.nextAnimation))
            anim.Play(vfx.nextAnimation);
        if (vfx.properties.HasProperty(VFXProperty.StopAnimation))
            anim.speed = 0;

        TMPro.TextMeshPro text = GetComponent<TMPro.TextMeshPro>();
        if (!string.IsNullOrEmpty(vfx.newText))
            text.text = vfx.newText;
        if (vfx.properties.HasProperty(VFXProperty.ChangeTextColor))
            text.color = vfx.textColor;
        if (vfx.properties.HasProperty(VFXProperty.ChangeFontSize))
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
            vfx.effectObj.SetActive(vfx.effectObj.activeSelf);

        StartCoroutine(CameraSystem.instance.Flash(vfx.camFlashTime, vfx.camFlashAlpha));
        CameraSystem.instance.Shake(vfx.shakeMode, vfx.smoothFunc, vfx.trauma == 0 ? 1 : vfx.trauma);
        if (vfx.properties.HasProperty(VFXProperty.ShockCamera))
            CameraSystem.instance.Shock(vfx.shockSpeed, vfx.shockSize);

        AudioManager.PlayAudio(vfx.audio);
        ObjectPooler.Spawn(vfx.poolType, transform.position);
        particle.SpawnParticle(vfx.particleType, transform.position, vfx.range);

        StartCoroutine(GameUtils.StopTime(vfx.stopTime));
        StartCoroutine(Flashing(whiteMat, vfx.triggerColor, vfx.flashDuration, vfx.flashTime));

        this.InvokeAfter(Mathf.Max(vfx.flashDuration, totalParticleTime, vfx.scaleTime) - vfx.flashDuration, () =>
        {
            if (vfx.properties.HasProperty(VFXProperty.FadeTextWhenDone))
                StartCoroutine(FadeText());
            else
                StartCoroutine(Flashing(whiteMat, new Color(1, 1, 1, vfx.alpha), vfx.fadeTime, vfx.fadeTime));

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

        IEnumerator Flashing(Material whiteMat, Color color, float duration, float flashTime)
        {
            if (whiteMat == null)
                yield break;
            whiteMat.color = color;
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
        }

        IEnumerator ScaleOverTime()
        {
            float duration = vfx.scaleTime;
            while (duration > 0)
            {
                duration -= Time.deltaTime;
                transform.localScale += (Vector3)vfx.scaleOffset * Time.deltaTime;
                yield return null;
            }

            while (gameObject.activeSelf)
            {
                transform.localScale -= (Vector3)vfx.scaleOffset * Time.deltaTime;
                yield return null;
            }
        }

        IEnumerator FadeText()
        {
            float dAlpha = (text.alpha - vfx.alpha) / vfx.fadeTime;
            while (text.alpha > vfx.alpha)
            {
                text.alpha -= dAlpha * Time.deltaTime;
                yield return null;
            }
        }

        IEnumerator EnableTrail(TrailRenderer trail, float emitTime, float stayTime)
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

        IEnumerator DecreaseTrailWidth(TrailRenderer trail, float decreaseTime)
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

    // Damage popu
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
        float groundHeight = Physics2D.BoxCast(transform.position, new Vector2(spriteExtents.x / 2, 0.01f), 0, -transform.up, spriteExtents.y * 2, LayerMask.GetMask("Ground")).distance;
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
