using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EnemyType
{
    Alien,
    Bat,
    Maggot,
    Slime,
    NoEye,
    Ghost,
}

public enum EnemyState
{
    Idle,
    Move,
    Attack,
    Explode,
}

public class Enemy : MonoBehaviour
{
    public static int numberOfEnemiesAlive = 0;

    public VariableReference<float> floatvalue;
    public VariableReference<int> intvalue;

    public IntReference health;
    public IntReference lowHP;
    public RangedFloat moneyDrop;
    public bool lookAtPlayer;
    public bool damageWhenCollide;
    [ShowWhen("damageWhenCollide", order = 0)] public IntReference collideDamage;
    public GameObject deathParticle;
    public Material hurtMat;
    public EnemyType enemyType;

    public float moveTime;
    public float attackTime;

    [Header("Movement Data")]
    public Vector2 targetDir;
    public RunData run;
    public JumpData jump;
    public FlyData fly;

    [Header("Ability Data")]
    public DashAttack dashAttack;
    public JumpAttack jumpAttack;
    public ExplodeAbility explodeAbility;
    public SplitAbility splitAbility;
    public ShootAbility shootAbility;
    public TeleportAbility teleportAbility;

    private ActivationType activationType = ActivationType.None;
    private EnemyState state;

    private Rigidbody2D rb;
    private Player player;
    private SpriteRenderer playerSr;
    private Animator anim;
    private SpriteRenderer sr;
    private Material defMat;
    private float timer = 0;

    private List<EnemyTag> tags = new List<EnemyTag>();
    private Dictionary<ActivationType, System.Action> abilityTable;

    void Start()
    {
        Init();
        Execute(enemyType);
    }

    void Init()
    {
        numberOfEnemiesAlive += 1;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        defMat = sr.material;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        playerSr = player.GetComponent<SpriteRenderer>();

        switch (enemyType)
        {
            case EnemyType.Alien:
                {
                    CreateWeapon(ref shootAbility.weapon);
                } break;
            case EnemyType.Bat:
                {
                    state = EnemyState.Move;
                } break;
            case EnemyType.Maggot:
                {
                    teleportAbility.trail = GetComponent<TrailRenderer>();
                } break;
        }
    }

    void CreateWeapon(ref Weapon weapon)
    {
        weapon = Instantiate(weapon, transform.position, Quaternion.identity);
        weapon.transform.parent = transform;
        weapon.transform.localPosition = weapon.posOffset;
        // NOTE: The enemy has the exact gun prefab like the player so need to remove unnecessary component
        //       This is just for temporary. Should I make different guns for enemy?
        Destroy(weapon.GetComponent<ActiveReload>());
    }

    // Update is called once per frame
    void Update()
    {
        InternalDebug.Log(state);
        if (health <= 0)
        {
            activationType = ActivationType.Die;
        }
        else if (health < lowHP)
        {
            activationType = ActivationType.LowHP;
        }

        if (lookAtPlayer) LookAtPlayer();
        else transform.rotation = Quaternion.Euler(0, rb.velocity.x > 0 ? 0 : 180, 0);

        switch (activationType)
        {
            case ActivationType.LowHP:
                {
                    abilityTable[ActivationType.LowHP]?.Invoke();
                } break;
            case ActivationType.Die:
                {
                    abilityTable[ActivationType.Die]?.Invoke();
                    Die();
                } break;
        }
    }

    void Execute(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Alien:
                StartCoroutine(AlienLogic());
                break;
            case EnemyType.Bat:
                StartCoroutine(BatLogic());
                break;
            case EnemyType.Maggot:
                StartCoroutine(MaggotLogic());
                break;
            case EnemyType.Slime:
                StartCoroutine(SlimeLogic());
                break;
        }
    }

    IEnumerator AlienLogic()
    {
        while (true)
        {
            switch (state)
            {
                case EnemyState.Idle:
                    {
                        rb.velocity = Vector2.zero;
                        yield return new WaitForSeconds(Random.Range(2.5f, 4.5f));
                        SetupTarget(TargetType.Player);
                        ChangeState(EnemyState.Move);
                    }
                    break;
                case EnemyState.Move:
                    {
                        if (!GroundCheck() || CliffCheck() || WallCheck())
                        {
                            targetDir.x *= -1;
                        }
                        Run();
                        timer += Time.deltaTime;
                        if (timer >= moveTime)
                        {
                            ChangeState(MathUtils.RandomBool(.75f) ? EnemyState.Attack : EnemyState.Idle);
                            timer = 0;
                        }
                    }
                    break;
                case EnemyState.Attack:
                    {
                        float timeBtwBullets = 1 / shootAbility.weapon.stat.fireRate;
                        int numberOfBullets = 5;
                        StartCoroutine(Shoot(numberOfBullets, timeBtwBullets, 3, 1, shootAbility));
                        yield return new WaitForSeconds(4);
                        ChangeState(EnemyState.Move);
                        timer = 0;
                    }
                    break;
            }
            yield return null;
        }
    }

    IEnumerator BatLogic()
    {
        bool isExploding = false;
        while (!isExploding)
        {
            switch (state)
            {
                case EnemyState.Move:
                    SetupTarget(TargetType.Player);
                    Fly(fly.type);
                    if (IsInRange(explodeAbility.distanceToExplode))
                        ChangeState(EnemyState.Explode);
                    break;
                case EnemyState.Explode:
                    StartCoroutine(StartExploding(explodeAbility));
                    isExploding = true;
                    break;
            }
            yield return null;
        }
    }

    IEnumerator MaggotLogic()
    {
        while (true)
        {
            SetupTarget(TargetType.Player);
            Run();
            if ((teleportAbility.DistanceToTeleportX.Enabled && !IsInRange(teleportAbility.DistanceToTeleportX))
                || (teleportAbility.DistanceToTeleportY.Enabled && !IsInRange(teleportAbility.DistanceToTeleportY)))
            {
                StartCoroutine(Teleport(teleportAbility.trail, TargetType.AroundPlayer, teleportAbility.distanceToPlayer));
            }
            if (IsInRange(explodeAbility.distanceToExplode))
            {
                StartCoroutine(StartExploding(explodeAbility));
                break;
            }
            yield return null;
        }
    }

    IEnumerator SlimeLogic()
    {
        while (true)
        {
            Jump();
            yield return null;
        }
    }

    void ChangeState(EnemyState nextState)
    {
        state = nextState;
        switch (nextState)
        {
            case EnemyState.Idle:
                anim.Play("Idle");
                break;
            case EnemyState.Move:
                anim.Play("Move");
                break;
            case EnemyState.Attack:
                anim.Play("Shoot");
                break;
        }
    }

    void SetupTarget(TargetType targetType)
    {
        switch (targetType)
        {
            case TargetType.Randomly:
                {
                    targetDir = MathUtils.RandomVector2().normalized;
                }
                break;
            case TargetType.Player:
                {
                    targetDir = (player.transform.position - transform.position).normalized;
                }
                break;
            case TargetType.AroundPlayer:
                {

                }
                break;
        }
    }

    void Run()
    {
        rb.velocity = Vector2.right * Mathf.Sign(targetDir.x) * run.runSpeed * Time.deltaTime;
    }

    void Jump()
    {
        rb.velocity = new Vector2(Mathf.Cos(jump.jumpAngle) * Mathf.Sign(targetDir.x), Mathf.Sin(jump.jumpAngle)) * jump.jumpForce;
    }

    void Fly(FlyPattern pattern)
    {
        switch (pattern)
        {
            case FlyPattern.Linear:
                {
                    rb.velocity = targetDir * fly.flySpeed * Time.deltaTime;
                } break;
        }
    }

    IEnumerator Shoot(int numberOfBullets, float timeBtwShots, int numberOfBursts, float timeBtwBursts, ShootAbility shooter)
    {
        while (numberOfBursts > 0)
        {
            while (numberOfBursts > 0)
            {
                SpawnProjectile(shooter.weapon.stat, shooter.weapon.shootPos.position);
                numberOfBullets--;
                yield return new WaitForSeconds(timeBtwShots);
            }
            numberOfBursts--;
            yield return new WaitForSeconds(timeBtwBursts);
        }
    }

    void SpawnProjectile(WeaponStat stat, Vector3 shootPos)
    {
        AudioManager.instance.PlaySfx(stat.sfx);
        Projectile projectile = ObjectPooler.instance.SpawnFromPool<Projectile>(stat.projectile, 
            shootPos, Quaternion.Euler(0, 0, CaculateRotationToPlayer()));
        projectile.Init(stat.damage, 0, 0, true, false);
    }

    // NOTE: The enemy can have already been destroyed while running the coroutine. Fix this when possible (Maybe call the coroutine on a seperate object?)
    private IEnumerator ShootCircle(WeaponStat stat, ShootPatternData patternData, float radius, Vector2 targetDir)
    {
        patternData.bulletHolder = Instantiate(patternData.bulletHolderObj, transform.position, Quaternion.identity).transform;
        patternData.bullets = new Projectile[patternData.numberOfBullets];
        int i = 0;
        foreach (Vector2 pos in MathUtils.GenerateCircleOutline(transform.position, patternData.numberOfBullets, radius))
        {
            patternData.bullets[i] = ObjectPooler.instance.SpawnFromPool<Projectile>(stat.weaponName,
                pos, Quaternion.identity, patternData.bulletHolder);
            patternData.bullets[i].Init(stat.damage, 0, 0, true, false);
            patternData.bullets[i].SetVelocity(0);
            i++;
        }

        AudioManager.instance.PlaySfx(stat.sfx);
        yield return new WaitForSeconds(patternData.delayBulletTime);

        patternData.bulletHolder.gameObject.GetComponent<Rigidbody2D>().velocity = targetDir * patternData.bullets[0].speed; // All bullets have the same speed
        Destroy(patternData.bulletHolder.gameObject, patternData.bullets[0].timer);
        if (patternData.rotate)
            while (true)
            {
                patternData.bulletHolder.transform.Rotate(new Vector3(0, 0, patternData.rotateSpeed * Time.deltaTime * (patternData.clockwise ? 1 : -1)));
                yield return null;
            }
    }

    IEnumerator Teleport(TrailRenderer trail, TargetType targetType, float distanceToPlayer)
    {
        Vector3 destination = player.transform.position;
        switch (targetType)
        {
            case TargetType.Randomly:
                {

                } break;
            case TargetType.AroundPlayer:
                {
                    Vector3 offset = new Vector2(Mathf.Sign(transform.position.x - player.transform.position.x) * distanceToPlayer,
                        (sr.bounds.extents.y - playerSr.bounds.extents.y) /* * enemy.transform.up.y*/);
                    destination += offset;
                } break;
        }

        trail.enabled = true;
        transform.position = destination;
        yield return new WaitForSeconds(.1f);
        trail.enabled = false;
    }    

    public bool CheckTag(EnemyTag tag)
    {
        if (tags.Contains(tag))
            return true;
        else
            return HandleTag(tag);
    }

    bool HandleTag(EnemyTag tag)
    {
        switch (tag)
        {
            case EnemyTag.HitWall:
                return WallCheck();
            case EnemyTag.HitCliff:
                return CliffCheck();
            case EnemyTag.HitPlayer:
                Vector2 pos = transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, 0, 0);
                InternalDebug.DrawRay(pos, transform.right * 30, Color.blue);
                return Physics2D.Raycast(pos, transform.right, 30, LayerMask.GetMask("Player"));
            case EnemyTag.OnGround:
                return GroundCheck();
            case EnemyTag.LineOfSight:
                Vector2 dir = player.position.value - transform.position;
                return Physics2D.Raycast(transform.position, dir, dir.magnitude).collider.CompareTag("Player");
        }
        return false;
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
        if (damageWhenCollide && collision.CompareTag("Player")) player.Hurt(collideDamage);
    }

    public void Die()
    {
        Instantiate(deathParticle, transform.position, Quaternion.identity);
        for (int i = 0; i < moneyDrop.randomValue; i++)
            ObjectPooler.instance.SpawnFromPool("Money", transform.position, Random.rotation);
        numberOfEnemiesAlive--;
        Destroy(gameObject);
    }

    public void Hurt(int damage)
    {
        health.value -= damage;
        AudioManager.instance.PlaySfx("GetHit");
        StartCoroutine(ResetMaterial());

        IEnumerator ResetMaterial()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.material = hurtMat;
            yield return new WaitForSeconds(.1f);
            sr.material = defMat;
        }
    }

    IEnumerator StartExploding(ExplodeAbility explodesion)
    {
        StartCoroutine(Flashing(explodesion.flashData, explodesion.explodeTime));
        yield return new WaitForSeconds(explodesion.explodeTime);
        Explode(explodesion);
        Die();
    }

    void Explode(ExplodeAbility explodesion)
    {
        AudioManager.instance.PlaySfx(explodesion.explodeSound);
        EZCameraShake.CameraShaker.Instance.ShakeOnce(8, 5, 0.1f, 0.5f);
        GameObject explodeVFX = Instantiate(explodesion.explodeParticle, transform.position, Quaternion.identity);
        explodeVFX.transform.localScale = new Vector3(6.25f, 6.25f, 0) * explodesion.explodeRange;
        Destroy(explodeVFX, .3f);
        if ((player.transform.position - transform.position).sqrMagnitude < explodesion.explodeRange * explodesion.explodeRange)
            player.Hurt(explodesion.explodeDamage);
    }

    IEnumerator Flashing(FlashAbility flash, float duration)
    {
        if (flash.stopWhileFlashing)
            rb.velocity = Vector2.zero;
        flash.triggerMat.color = flash.color;
        while (duration > 0)
        {
            sr.material = flash.triggerMat;
            yield return new WaitForSeconds(flash.flashTime);

            sr.material = defMat;
            yield return new WaitForSeconds(flash.timeBtwFlashes);

            duration -= Time.deltaTime;
        }
    }

    // TODO: This is stupid. Need to rework on this (Maybe save all the ground tiles in RoomManager and spawn random there?)
    void TeleportToRandomPos(int maxTry, float minDistance, float maxDistance)
    {
        int numberOfTryLeft = maxTry;
        Vector2Int newPos = GetRandomPosition();
        while (RoomManager.allGroundTiles.Contains(newPos) && numberOfTryLeft > 0)
        {
            newPos = GetRandomPosition();
            numberOfTryLeft--;
        }
        if (RoomManager.allGroundTiles.Contains(newPos)) // We need this to check if newPos is actually valid or the number of tries left is zero
            transform.position = (Vector2)newPos;


        Vector2Int GetRandomPosition()
        {
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 randomDir = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
            float randomDistance = Random.Range(minDistance, maxDistance);
            return MathUtils.ToVector2Int((Vector2)transform.position + randomDir * randomDistance);
        }
    }

    void Split(GameObject splitEnemy)
    {
        Vector3 offset = new Vector3(.5f, 0, 0);
        Instantiate(splitEnemy, transform.position + offset, Quaternion.identity);
        Instantiate(splitEnemy, transform.position + offset, Quaternion.identity);
    }

    bool GroundCheck()
    {
        Vector2 pos = (Vector2)transform.position - new Vector2(0, sr.bounds.extents.y * transform.up.y);
        Vector2 size = new Vector2(sr.bounds.size.x, 0.1f);
        ExtDebug.DrawBox(pos, size / 2, Quaternion.identity, Color.green);
        return Physics2D.BoxCast(pos, size, 0, -transform.up, 0, LayerMask.GetMask("Ground"));
    }

    /// <summary>
    ///  return true when detect a cliff
    /// </summary>
    bool CliffCheck()
    {
        Vector2 pos = (Vector2)transform.position + new Vector2((sr.bounds.extents.x + .1f) * Mathf.Sign(rb.velocity.x), -sr.bounds.extents.y);
        InternalDebug.DrawRay(pos, Vector2.down, Color.cyan);
        return !Physics2D.Raycast(pos, Vector2.down, .1f, LayerMask.GetMask("Ground"));
    }

    bool WallCheck()
    {
        Vector2 pos = transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, 0, 0);
        InternalDebug.DrawRay(pos, transform.right * .1f, Color.yellow);
        RaycastHit2D hit = Physics2D.Raycast(pos, transform.right, .1f, LayerMask.GetMask("Ground"));
        return hit;
    }

    bool PlayerCheck()
    {
        Vector2 pos = transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, 0, 0);
        InternalDebug.DrawRay(pos, transform.right * float.MaxValue, Color.blue);
        return Physics2D.Raycast(pos, transform.right, float.MaxValue, LayerMask.GetMask("Player"));
    }

    float CaculateRotationToPlayer()
    {
        Vector2 difference = -transform.position + player.transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        return rotationZ;
    }

    void LookAtPlayer()
    {
        if (player.transform.position.x <= transform.position.x)
            transform.eulerAngles = new Vector3(0, 180, 0);
        else if (player.transform.position.x > transform.position.x)
            transform.eulerAngles = Vector3.zero;
    }

    bool IsInRange(float range)
    {
        return (player.transform.position - transform.position).sqrMagnitude < range * range;
    }
}
