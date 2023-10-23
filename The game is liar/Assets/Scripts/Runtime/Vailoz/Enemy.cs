using UnityEngine;
using System.Collections;

public partial class Enemy : MonoBehaviour
{
    public enum BrainType
    {
        Maggot,
        NoEyes,
        FlyDrone,
        Bat,
        Alien,
        GiantEye,
        LittleEye,
        
        BrainCount
    }
    
    enum RotationType
    {
        MoveDir,
        Player,
    }
    
    enum EnemyState
    {
        Normal,
        Charge,
        Cooldown,
        Invincible,
    }
    
    public static int numberOfEnemiesAlive = 0;
    
    [Header("Enemy Data")]
    public IntReference health;
    [MinMax(0, 5)] public RangedInt moneyDrop;
    public Material whiteMat;
    public int collideDamage;
    public Color hurtColor;
    public GameObject deathParticle;
    public GameObject deathEffect;
    
    public BrainType brain;
    
    public float speed;
    private Vector2 targetDir;
    
    [Header("Ability Data")]
    public float abilityCooldownTime;
    public float waitTimeAfterExecute;
    public float distanceToExecute;
    public int abilityDamage;
    public float abilityChargeTime;
    public float timeBtwFlashes;
    public Color flashColor;
    public AudioType abilitySound;
    
    [Header("Drone Data")]
    public float rotZOffset;
    public float timeToReachRot;
    
    public Transform eye;
    public Vector2 lookUpPos;
    public Vector2 lookDownPos;
    
    [Header("Trail Data")]
    public float trailTime;
    public float emitTrailTime;
    private TrailRenderer trail;
    
    [Header("Teleport Data")]
    public float distanceToTeleportX;
    public float distanceToTeleportY;
    [MinMax(0, 10)] public RangedFloat teleportPosOffset;
    public float delayTeleportTime;
    private bool isDelaying;
    private bool canTeleport;
    
    [Header("Dash Data")]
    [Range(0f, 1f)] public float updatePlayerPosTime;
    [Range(0f, 1f)] public float speedAfterDashScale;
    public float dashSpeed;
    public float dashTime;
    public AudioType hitSFX;
    
    [Header("Explosion Data")]
    public ShakeMode cameraShakeMode;
    public float explodeRange;
    public GameObject explodeParticle;
    
    [Header("Bullet Data")]
    public int bulletCount;
    public string bullet;
    public int bulletDamage;
    public float radius;
    private Vector3[] bulletsPos;
    
    private int playerDirX;
    private Rigidbody2D rb;
    private Player player;
    private SpriteRenderer playerSr;
    private SpriteRenderer sr;
    private Material defMat;
    private ParticleSystem teleportEffect;
    private Animator anim;
    private bool groundCheck;
    private bool cliffCheck;
    private bool wallCheck;
    private EnemyState state;
    private RotationType rotationType;
    private Vector2 spriteOffset;
    private Vector2 spriteSize;
    
    void Start()
    {
        Init();
        InitBrain();
    }
    
    void Init()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        defMat = sr.material;
        whiteMat = new Material(whiteMat);
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        playerSr = player.GetComponent<SpriteRenderer>();
        trail = GetComponent<TrailRenderer>();
    }
    
    void InitBrain()
    {
        switch (brain)
        {
            case BrainType.Maggot:
            {
                teleportEffect = GetComponentInChildren<ParticleSystem>();
                targetDir = new Vector2(Mathf.Sign(player.transform.position.x - transform.position.x), 0);
            } break;
            case BrainType.FlyDrone:
            {
                StartCoroutine(RotateEnemy(rotZOffset, timeToReachRot));
            } break;
            case BrainType.GiantEye:
            {
                bulletsPos = new Vector3[bulletCount];
                BoxCollider2D box = GetComponent<BoxCollider2D>();
                spriteOffset = box.offset * transform.lossyScale;
                spriteSize = box.size * transform.lossyScale;
            } break;
        }
    }
    
    void Update()
    {
        if (state == EnemyState.Invincible) return; 
        
        if (health <= 0)
            Die();
        
        playerDirX = (int)Mathf.Sign(player.transform.position.x - transform.position.x);
        switch (rotationType)
        {
            case RotationType.MoveDir:
            {
                float target = Mathf.Sign(targetDir.x);
                Debug.Assert(target != 0, "No Dir!"); // NOTE: All enemies stop by changing the velocity directly to zero, nobody changes targetDir to zero.
                if ((target != 0) && (Mathf.Sign(transform.right.x) != target))
                    transform.Rotate(new Vector3(0, 180, 0), Space.World);
            } break;
            case RotationType.Player:
            {
                if (Mathf.Sign(transform.right.x) != playerDirX)
                    transform.Rotate(new Vector3(0, 180, 0), Space.World);
            } break;
        }
        
        switch (brain)
        {
            case BrainType.GiantEye:
            {
                Vector2 dir = MathUtils.Sign(targetDir);
                Vector2 pos = spriteOffset * dir.x;
                groundCheck = RayCast(pos + new Vector2(0, spriteSize.y / 2) * dir.y, Vector2.up * dir.y, Color.green);
                wallCheck = RayCast(pos + new Vector2(spriteSize.x / 2, 0) * dir.x, transform.right, Color.yellow);
            } break;
            default:
            {
                groundCheck = BoxCast((Vector2)transform.position - new Vector2(0, sr.bounds.extents.y * transform.up.y), new Vector2(sr.bounds.size.x, 0.1f), Color.green);
                wallCheck = RayCast(new Vector2(sr.bounds.extents.x * transform.right.x, 0), new Vector2(Mathf.Sign(targetDir.x), 0), Color.yellow);
                cliffCheck = !RayCast(new Vector2((sr.bounds.extents.x + .2f) * Mathf.Sign(targetDir.x), -sr.bounds.extents.y * transform.up.y), -transform.up, Color.cyan);
            } break;
        }
        
        if (state == EnemyState.Charge) return;
        
        ExecuteBrain(brain);
        if (state == EnemyState.Normal)
            rb.velocity = targetDir * speed;
    }
    
    void ExecuteBrain(BrainType type)
    {
        switch (type)
        {
            case BrainType.Maggot:
            {
                if (cliffCheck || wallCheck)
                    targetDir *= -1;
                switch (state)
                {
                    case EnemyState.Normal:
                    {
                        if (!isDelaying)
                        {
                            bool inRange = IsInRangeX(distanceToTeleportX) && IsInRangeY(distanceToTeleportY);
                            if (player.controller.groundCheck && (!inRange || cliffCheck))
                            {
                                if (canTeleport || cliffCheck)
                                {
                                    canTeleport = false;
                                    StartCoroutine(Teleport(waitTimeAfterExecute));
                                }
                                else
                                {
                                    StartCoroutine(DelayTeleport(delayTeleportTime));
                                    
                                    IEnumerator DelayTeleport(float delayTime)
                                    {
                                        isDelaying = true;
                                        yield return new WaitForSeconds(delayTime);
                                        canTeleport = true;
                                        isDelaying = false;
                                    }
                                }
                            }
                            else if (canTeleport)
                                canTeleport = false;
                        }
                        
                        if (IsInRange(distanceToExecute))
                            StartCoroutine(Explode());
                    } break;
                }
            } break;
            case BrainType.NoEyes:
            {
                // TODO: Handle cases when the player is still in range after cooldown.
                switch (state)
                {
                    case EnemyState.Normal:
                    {
                        if (IsInRange(distanceToExecute))
                            StartCoroutine(Dash(true));
                        else
                            targetDir = (player.transform.position - transform.position).normalized;
                    } break;
                    case EnemyState.Cooldown:
                    {
                        rb.velocity = MathUtils.Sign(player.transform.position - transform.position) * speed * speedAfterDashScale * Vector2.left;
                        StartCoroutine(StartState(EnemyState.Charge, abilityCooldownTime));
                    } break;
                }
            } break;
            case BrainType.FlyDrone:
            {
                targetDir = (player.transform.position - transform.position).normalized;
                if ((targetDir.x < .5f) && (targetDir.x > -.5f)) // between 60-120 degrees
                    if (targetDir.y > 0)
                    eye.localPosition = lookUpPos;
                else
                    eye.localPosition = lookDownPos;
                else
                    eye.localPosition = Vector3.zero;
                if (IsInRange(distanceToExecute))
                {
                    StartCoroutine(Explode());
                    StartCoroutine(RotateEnemy(0, timeToReachRot));
                }
            } break;
            case BrainType.Bat:
            {
                targetDir = (player.transform.position - transform.position).normalized;
                if (IsInRange(distanceToExecute))
                    StartCoroutine(Explode());
            } break;
            case BrainType.GiantEye:
            {
                /*
                 * Phase 1:
                 * - Moves like no eyes
                 * - Spawns little eyes
                 * - When dashes and hits wall, create a wave of projectile
                 * Phase 2:
                 * - Moves to the center
                 * - Shoots circle waves of projectiles
                 * - Shoots toward the player
                 * - Shoots big projectiles that explode into smaller projectiles
                 */
                
                switch (state)
                {
                    case EnemyState.Normal:
                    {
                        // TODO: Sometimes the enemy already collide with wall. Fix it so that the enemy only dash when doesn't collide with wall.
                        if (IsInRange(distanceToExecute))
                            StartCoroutine(Dash(false)); // NOTE: Maybe enable trail differently.
#if false
                        else if (Time.time > timer)
                        {
                            timer = Time.time + abilityCooldownTime;
                            int enemyCount = numberOfEnemiesToSpawn.randomValue;
                            for (int i = 0; i < enemyCount; i++)
                            {
                                Vector3 pos; // TODO: Get random position, maybe from RoomManager
                                Instantiate(enemyToSpawn, pos, Quaternion.identity);
                            }
                            
                            anim.Play("");
                            StartCoroutine(StartState(EnemyState.Charge, anim.GetCurrentAnimatorStateInfo(0).length));
                        }
#endif
                        else
                            targetDir = (player.transform.position - transform.position).normalized;
                    } break;
                    case EnemyState.Cooldown:
                    {
                        // Spawn wave of bullets
                        {
                            Vector2 dir = MathUtils.Sign(targetDir);
                            // Make sure the enemy isn't inside wall
                            {
                                float magnitude;
                                // Calculate the magnitude of the ray from the center to the egde of the box collider.
                                // https://stackoverflow.com/questions/1343346/calculate-a-vector-from-the-center-of-a-square-to-edge-based-on-radius
                                {
                                    float absCos = Mathf.Abs(Mathf.Cos(targetDir.x));
                                    float absSin = Mathf.Abs(Mathf.Sin(targetDir.y));
                                    if (spriteSize.x * absSin <= spriteSize.y * absCos)
                                        magnitude = spriteSize.x / 2 / absCos;
                                    else
                                        magnitude = spriteSize.y / 2 / absSin;
                                }
                                
                                Vector2 spriteOrigin = (Vector2)transform.position + spriteOffset * dir.x;
                                RaycastHit2D hitInfo = Physics2D.Raycast(spriteOrigin, targetDir, magnitude, LayerMask.GetMask("Ground"));
                                Debug.DrawRay(spriteOrigin, targetDir * magnitude, Color.white);
                                
                                // NOTE: Sometime the enemy stop right before actually touching the ground (because of the raycast's length)
                                // so I will need this check.
                                if (hitInfo)
                                {
                                    transform.position -= (Vector3)dir * (magnitude - hitInfo.distance);
                                    GameDebug.DrawBox(spriteOrigin - dir * (magnitude - hitInfo.distance), spriteSize, Color.white);
                                }
                            }
                            
                            Vector3 center = transform.position + (Vector3)spriteOffset * dir.x;
                            if (wallCheck)
                                center.x += (spriteSize.x - 1.5f) / 2 * dir.x;
                            if (groundCheck)
                                center.y += (spriteSize.y - 1.5f) / 2 * dir.y;
                            
                            Vector2 compareOffset = Vector2.Scale(Vector2.one * .2f, dir); // For precision error
                            MathUtils.GenerateCircleOutlineNonAlloc(center, radius, bulletsPos);
                            foreach (var pos in bulletsPos)
                            {
                                if (wallCheck && (Mathf.Sign(pos.x - (center.x + compareOffset.x)) == dir.x)) continue;
                                if (groundCheck && (Mathf.Sign(pos.y - (center.y + compareOffset.y)) == dir.y)) continue;
                                
                                Vector3 euler = new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, pos - center));
                                if (euler.z > 90 || euler.z < -90)
                                    euler = new Vector3(0, 180f, 180f - euler.z);
                                ObjectPooler.Spawn<MovingEntity>(PoolType.Bullet_Blood, pos, euler).InitBullet(bulletDamage, false, true);
                            }
                        }
                        
                        anim.Play("");
                        StartCoroutine(StartState(EnemyState.Charge, waitTimeAfterExecute));
                    } break;
                }
            } break;
            case BrainType.LittleEye:
            {
                // TODO: Behave like no eyes but explode into projectiles when die
            } break;
        }
    }
    
    IEnumerator RotateEnemy(float rotTo, float timeToReach)
    {
        float deltaRot = rotTo - transform.eulerAngles.z;
        if (deltaRot > 180f)
            deltaRot = 360f - deltaRot;
        else if (deltaRot < -180f)
            deltaRot = 360f + deltaRot;
        float deltaRotPerSec = deltaRot / timeToReach;
        
        while (timeToReach > 0)
        {
            float rotZ = deltaRotPerSec * Time.fixedDeltaTime;
            transform.rotation *= Quaternion.Euler(0, 0, rotZ);
            timeToReach -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }
    
    void Charge(bool flashing = true)
    {
        state = EnemyState.Charge;
        rb.velocity = Vector2.zero;
        rotationType = RotationType.Player;
        if (flashing)
            StartCoroutine(Flashing(abilityChargeTime, timeBtwFlashes, flashColor));
    }
    
    IEnumerator EnableTrail(float waitTime, float emitTime, bool decreaseWidthOverTime = false)
    {
        trail.enabled = true;
        trail.emitting = true;
        
        yield return new WaitForSeconds(waitTime);
        
        if (decreaseWidthOverTime)
            StartCoroutine(DecreaseTrailWidth(emitTime));
        trail.emitting = false;
        yield return new WaitForSeconds(emitTime);
        
        trail.emitting = true;
        trail.Clear();
        trail.enabled = false;
        
        IEnumerator DecreaseTrailWidth(float decreaseTime)
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
    
    IEnumerator Dash(bool stopWhenOutOfTime)
    {
        Charge();
        
        yield return new WaitForSeconds(abilityChargeTime * updatePlayerPosTime);
        targetDir = (player.transform.position - transform.position).normalized;
        rotationType = RotationType.MoveDir;
        yield return new WaitForSeconds(abilityChargeTime * (1 - updatePlayerPosTime));
        
        rb.velocity = targetDir * dashSpeed;
        StartCoroutine(EnableTrail(dashTime, emitTrailTime));
        int baseDamage = collideDamage;
        collideDamage = abilityDamage;
        AudioManager.PlayAudio(abilitySound);
        
        if (stopWhenOutOfTime)
            yield return new WaitForSeconds(dashTime);
        else
        {
            while (!(wallCheck || groundCheck))
                yield return null;
            AudioManager.PlayAudio(hitSFX);
        }
        
        collideDamage = baseDamage;
        rb.velocity = Vector2.zero;
        state = EnemyState.Cooldown;
    }
    
    IEnumerator Explode()
    {
        Charge();
        yield return new WaitForSeconds(abilityChargeTime);
        
        AudioManager.PlayAudio(abilitySound);
        CameraSystem.instance.Shake(cameraShakeMode, MathUtils.SmoothStart3);
        CameraSystem.instance.Shock(2);
        ParticleEffect.instance.SpawnParticle(ParticleType.Explosion, transform.position, explodeRange);
        if (IsInRange(explodeRange))
            player.Hurt(abilityDamage);
        Die(true);
    }
    
    // TODO: Maybe switch to teleport to a tile from a tiles array
    IEnumerator Teleport(float waitTime)
    {
        Vector3 destination;
        {
            destination = player.transform.position;
            destination.y += (sr.bounds.extents.y - playerSr.bounds.extents.y) * player.transform.up.y;
            
            float randomDistance = teleportPosOffset.randomValue * -playerDirX;
            float minX = teleportPosOffset.min * -playerDirX;
            
            // NOTE: Maybe teleport opposite to where the player is heading?
            if (!IsPosValid(randomDistance))
                if (!IsPosValid(minX))
                if (!IsPosValid(-randomDistance))
                if (!IsPosValid(-minX))
                yield break;
            
            bool IsPosValid(float offsetX)
            {
                Vector3 offset = new Vector3(offsetX, 0);
                bool onGround = BoxCast(destination + offset - new Vector3(0, sr.bounds.extents.y * player.transform.up.y), new Vector2(sr.bounds.size.x, .1f), Color.yellow);
                bool insideWall = BoxCast(destination + offset + new Vector3(0, .1f * player.transform.up.y), sr.bounds.size, Color.green);
                if (onGround && !insideWall)
                {
                    destination.x += offsetX;
                    return true;
                }
                return false;
            }
        }
        
        if (player.transform.up.y != transform.up.y)
            transform.Rotate(new Vector3(180, 0, 0), Space.World);
        
        Charge(flashing: false);
        StartCoroutine(EnableTrail(trailTime, emitTrailTime, true));
        
        yield return null;
        transform.position = destination;
        teleportEffect.Play();
        yield return new WaitForSeconds(waitTime);
        
        rotationType = RotationType.MoveDir;
        targetDir.x = transform.right.x;
        StartCoroutine(StartState(EnemyState.Cooldown, abilityCooldownTime));
    }
    
    IEnumerator Flashing(float duration, float flashTime, Color color)
    {
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
    }
    
    IEnumerator StartState(EnemyState state, float time)
    {
        this.state = state;
        yield return new WaitForSeconds(time);
        this.state = EnemyState.Normal;
    }
    
    public void Die(bool explode = false)
    {
        if (!explode)
        {
            CameraSystem.instance.Shake(ShakeMode.Medium);
            ObjectPooler.Spawn(PoolType.VFX_Destroyed_Enemy, transform.position);
        }
        
        // TODO: Spawn splash of blood and small pieces
        
        player.InvokeAfter(.3f, () => player.StartCoroutine(GameUtils.StopTime(.05f)));
        AudioManager.PlayAudio(AudioType.Enemy_Death);
        int dropValue = moneyDrop.randomValue;
        for (int i = 0; i < dropValue; i++)
            ObjectPooler.Spawn(PoolType.Cell, transform.position);
        numberOfEnemiesAlive--;
        Destroy(gameObject);
    }
    
    public void Hurt(int damage)
    {
        if (state == EnemyState.Invincible)
            return;
        // TODO: Have different hurt sound for enemies.
        AudioManager.PlayAudio(AudioType.Player_Hurt);
        health.value -= damage;
        if (health.value > 0f)
            StartCoroutine(GameUtils.StopTime(.02f));
        StartCoroutine(Flashing(.1f, .1f, hurtColor));
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.collider);
    }
    
    void HandleCollision(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            player.Hurt(collideDamage);
    }
    
    bool BoxCast(Vector2 pos, Vector2 size, Color color)
    {
        GameDebug.DrawBox(pos, size, color);
        return Physics2D.BoxCast(pos, size, 0, Vector2.zero, 0, LayerMask.GetMask("Ground"));
    }
    
    bool RayCast(Vector2 offset, Vector2 dir, Color color, float length = .2f)
    {
        InternalDebug.DrawRay((Vector2)transform.position + offset, dir * length, color);
        return Physics2D.Raycast((Vector2)transform.position + offset, dir, length, LayerMask.GetMask("Ground"));
    }
    
    bool IsInRange(float range)
    {
        return (player.transform.position - transform.position).sqrMagnitude < range * range;
    }
    
    bool IsInRangeX(float range)
    {
        return Mathf.Abs(player.transform.position.x - transform.position.x) < range;
    }
    
    bool IsInRangeY(float range)
    {
        return Mathf.Abs(player.transform.position.y - transform.position.y) < range;
    }
}