using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum AIType
{
    Simple,
}

public partial class Enemy : MonoBehaviour
{
    public static int numberOfEnemiesAlive = 0;

    public IntReference health;
    public RangedFloat moneyDrop;
    public bool lookAtPlayer;
    public bool damageWhenCollide;
    [ShowWhen("damageWhenCollide", order = 0)]
    public int collideDamage;
    public GameObject deathParticle;
    public Material hurtMat;
    public AIType AI;

    [Header("Movement Data")]
    public MoveType moveType;
    public TargetType targetType;
    public Vector2 targetDir = Vector2.right;
    private MoveState state = MoveState.Move;

    public bool canStop;
    [ShowWhen("canStop")] public float moveTime;
    [ShowWhen("canStop")] public float waitTime;
    [ShowWhen("canStop")] public bool onlyUseAbilityWhenWait;
    private bool haveUsedAbility;
    [ShowWhen("moveType", new object[] { MoveType.Jump, MoveType.Run })]
    public bool onPlatform;

    [ShowWhen("moveType", MoveType.Run)]
    public RunData run;

    [ShowWhen("moveType", MoveType.Jump)]
    public JumpData jump;
    private Vector2 jumpVelocity;

    [ShowWhen("moveType", MoveType.Fly)]
    public FlyData fly;

    [Header("Ability Data")]
    public Optional<ChargeAttack> chargeAttack;
    public Optional<JumpAttack> jumpAttack;
    private Vector2 jumpAttackVelocity;
    public Optional<ExplodeAbility> explodeAbility;
    public Optional<SplitAbility> splitAbility;
    public Optional<ShootAbility> shootAbility;
    public Optional<TeleportAbility> teleportAbility;

    private MultipleFramesAbility currentAbility;
    private Rigidbody2D rb;
    private Player player;
    private SpriteRenderer playerSr;
    private Animator anim;
    private SpriteRenderer sr;
    private Material defMat;
    private event System.Action collidePlayer;
    private float timer;
    private Optional<bool> groundCheck = new Optional<bool>(false);
    private Optional<bool> cliffCheck = new Optional<bool>(false);
    private Optional<bool> wallCheck = new Optional<bool>(false);

    private List<EnemyTag> tags = new List<EnemyTag>();

    void Start()
    {
        Init();
        InitMovement();
        InitAbility();
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

        if (moveType == MoveType.Fly)
        {
            groundCheck.enabled = false;
            cliffCheck.enabled = false;
            wallCheck.enabled = false;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            if (explodeAbility.enabled && explodeAbility.value.activationType == ActivationType.Die)
            {
                Explode(explodeAbility);
            }
            if (splitAbility.enabled && splitAbility.value.numberOfSplits > 0)
            {
                Split(splitAbility.value.splitEnemy);
            }
            Die();
        }        

        RotateEnemy();
        if (groundCheck.enabled) groundCheck.value = GroundCheck();
        if (wallCheck.enabled) wallCheck.value = WallCheck();
        if (cliffCheck.enabled) cliffCheck.value = CliffCheck();

        Move();
        UseAbility();
        InternalDebug.Log(state);
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
            collidePlayer?.Invoke();
            if (damageWhenCollide)
                player.Hurt(collideDamage);
        }
    }
}
