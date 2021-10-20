using UnityEngine;
using System.Collections;

public enum BrainType
{
    Slime,
    Maggot,
    NoEyes,
    Bat,

    BrainCount
}

public partial class Enemy : MonoBehaviour
{
    public static int numberOfEnemiesAlive = 0;

    [Header("Enemy Data")]
    public IntReference health;
    [MinMax(0, 5)] public RangedInt moneyDrop;
    public bool lookAtPlayer;
    public int collideDamage;
    public Animator spawnVFX;
    public Material hurtMat;
    public GameObject deathParticle;

    public AudioManager audioManager;
    public ObjectPooler pooler;

    [Header("Movement Data")]
    public MoveType moveType;
    private MoveState moveState;
    private float waitTime;
    private Vector2 targetDir = Vector2.right;

    [ShowWhen("moveType", MoveType.Run)]
    public RunData run;

    [ShowWhen("moveType", MoveType.Jump)]
    public JumpData jump;
    private Vector2 jumpVelocity;

    [ShowWhen("moveType", MoveType.Fly)]
    public FlyData fly;

    [Header("Ability Data")]
    public Optional<ChargeAttack> chargeAttack;
    public Optional<ExplodeAbility> explodeAbility;
    public Optional<SplitAbility> splitAbility;
    public Optional<ShootAbility> shootAbility;
    private float shootCooldown;
    public Optional<TeleportAbility> teleportAbility;

    private Rigidbody2D rb;
    private Player player;
    private SpriteRenderer playerSr;
    private Animator anim;
    private SpriteRenderer sr;
    private Material defMat;
    private float timer;
    private Optional<bool> groundCheck = new Optional<bool>(false);
    private Optional<bool> cliffCheck = new Optional<bool>(false);
    private Optional<bool> wallCheck = new Optional<bool>(false);
    private Optional<bool> isInWall = new Optional<bool>(false);
    private EnemyState state;

    private enum EnemyState
    {
        Normal,
        Stop,
        Invincible,
    }

    void Start()
    {
        Init();
        InitMovement();
        InitAbility();
        if (spawnVFX)
            StartCoroutine(StartSpawnVFX());
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
    }

    private IEnumerator StartSpawnVFX()
    {
        float vfxTime = Instantiate(spawnVFX, transform.position, Quaternion.identity).GetCurrentAnimatorStateInfo(0).length;
        enabled = false;
        sr.enabled = false;
        yield return new WaitForSeconds(vfxTime);
        enabled = true;
        sr.enabled = true;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (state == EnemyState.Invincible) return; 

        if (health <= 0)
        {
            if (explodeAbility.enabled && explodeAbility.value.activationType == ActivationType.Die)
            {
                StartCoroutine(StartExploding(explodeAbility));
                return;
            }
            if (splitAbility.enabled && splitAbility.value.splitEnemy)
            {
                Split(splitAbility.value.splitEnemy);
            }
            Die();
        }

        if (state == EnemyState.Stop) return;

        RotateEnemy();
        if (groundCheck.enabled) groundCheck.value = GroundCheck();
        if (wallCheck.enabled) wallCheck.value = WallCheck();
        if (cliffCheck.enabled) cliffCheck.value = CliffCheck();
        if (isInWall.enabled) isInWall.value = IsInWall();

        Move();
        UseAbility();
    }

    public void Die()
    {
        Instantiate(deathParticle, transform.position, Quaternion.identity);
        int dropValue = moneyDrop.randomValue;
        for (int i = 0; i < dropValue; i++)
        {
            ObjectPooler.instance.SpawnFromPool("Money", transform.position, Quaternion.identity);
        }
        numberOfEnemiesAlive--;
        Destroy(gameObject);
    }

    public void Hurt(int damage)
    {
        audioManager.PlaySfx("GetHit");
        health.value -= damage;
        StartCoroutine(ResetMaterial());

        IEnumerator ResetMaterial()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.material = hurtMat;
            yield return new WaitForSeconds(.1f);
            sr.material = defMat;
        }
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
        {
            player.Hurt(collideDamage);
        }
    }
}
