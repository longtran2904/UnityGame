using EZCameraShake;
using UnityEngine;

public class EnemiesMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    private Enemies enemy;

    public float speed;

    private Player player;

    private Enemies enemies;
    [HideInInspector] public EnemyType enemyType;

    private AudioManager audioManager;

    private Animator anim;

    [HideInInspector] public float attackRange;

    #region MaggotVariables
    // Maggot
    private bool touchingWall;
    private RaycastHit2D hitInfo;
    BoxCollider2D box;
    

    private Transform wallCheckPos;
    [HideInInspector] public float radius;
    [HideInInspector] public LayerMask whatIsGround;
    #endregion

    #region BatVariables
    // Bat
    [HideInInspector] public Material triggerMaterial;
    private SpriteRenderer sr;
    private Material defaultMaterial;

    private Transform curve_point;
    private Vector2 point;

    [HideInInspector] public float distanceToExplode;
    [HideInInspector] public float explodeRange;
    [HideInInspector] public float timeToExplode;
    [HideInInspector] public float distanceToChase;
    private float timeToExplodeValue;
    private bool canChase = false;
    private bool canExplode = false;
    private float timer;
    #endregion    

    #region AlienVariables
    private Weapon weapon;

    private Transform arm;

    float currentAngle = 0;
    #endregion

    #region NoEye Variables
    [HideInInspector] public float dashSpeed;

    bool isAttack;
    bool canCharge = true;

    Vector2 oldPos;

    [HideInInspector] public float dashTime;
    float dashTimeValue;

    [HideInInspector] public float chargeTime;
    float chargeTimeValue;
    #endregion

    #region Knockback
    // Knock back
    public float knockbackTime;
    private float knockbackCounter;
    private Vector2 knockbackForce;
    private bool knockback;
    #endregion

    #region Flash Color Variables
    [HideInInspector] public float timeBtwFlash;
    private float timeBtwFlashValue;
    [HideInInspector] public float flashTime;
    private float flashTimeValue;
    bool isFlashing;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();

        rb = GetComponent<Rigidbody2D>();

        sr = GetComponent<SpriteRenderer>() != null ? GetComponent<SpriteRenderer>() : GetComponentInChildren<SpriteRenderer>();

        defaultMaterial = sr.material;

        box = GetComponent<BoxCollider2D>();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        enemies = GetComponent<Enemies>();

        timeToExplodeValue = timeToExplode;

        timeBtwFlashValue = timeBtwFlash;

        flashTimeValue = flashTime;

        dashTimeValue = dashTime;

        chargeTimeValue = chargeTime;

        audioManager = FindObjectOfType<AudioManager>();

        switch (enemyType)
        {
            case EnemyType.Maggot:
                Transform[] childrens = GetComponentsInChildren<Transform>();

                foreach (Transform child in childrens)
                {
                    if (child.name.Equals("WallCheck"))
                    {
                        wallCheckPos = child;
                    }
                }

                break;
            case EnemyType.Bat:
                curve_point = player.transform.Find("Curve_point");
                break;
            case EnemyType.Alien:
                arm = transform.Find("Arm").GetComponent<Transform>();

                weapon = transform.Find("AlienPistol").GetComponent<Weapon>();

                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        if (enemyType == EnemyType.Maggot)
        {
            WallCheck();
        }
    }
    
    void FixedUpdate()
    {
        if (knockbackCounter < 0)
        {
            knockback = false;
        }

        if (knockback == false)
        {
            rb.velocity = Vector2.zero;
            knockback = true;
        }

        if (knockbackCounter > 0)
        {
            rb.velocity = new Vector2(knockbackForce.x, knockbackForce.y) * Time.deltaTime;

            knockbackCounter -= Time.deltaTime;

            return;
        }

        switch (enemyType)
        {
            case EnemyType.Alien:
                AlienMovement();
                break;
            case EnemyType.Bat:
                BatMovement();
                break;
            case EnemyType.Maggot:
                MaggotMovement();
                break;
            case EnemyType.Jelly:
                JellyMovement();
                break;
            case EnemyType.NoEye:
                MoveTowardPlayer();
                break;
        }        
    }

    void JellyMovement()
    {
        LookAtPlayer();

        if ((player.transform.position - transform.position).sqrMagnitude <= attackRange * attackRange && !enemies.touchingWall)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = (player.transform.position - transform.position).normalized * speed * Time.deltaTime;
    }

    #region BatLogic
    void BatMovement()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= distanceToChase)
        {
            canChase = true;
        }

        if (canExplode == true)
        {
            BatExplode(distanceToPlayer);
        }

        if (distanceToPlayer <= distanceToExplode)
        {
            canChase = false;
            canExplode = true;
        }
        else if (timer < 1 && canChase && timeToExplodeValue == timeToExplode)
        {
            timeToExplodeValue = timeToExplode;
            Vector2 old_point = point;
            point = GetBQCPoint(timer, transform.position, curve_point.position, player.transform.position);
            rb.velocity = new Vector2(point.x - transform.position.x, point.y - transform.position.y).normalized * speed * Time.deltaTime;
            timer += Time.deltaTime;
            Debug.DrawRay(transform.position, rb.velocity, Color.blue, .01f);
            Debug.DrawLine(old_point, point, Color.red, 3600);
        }
        else
        {
            timer = 0;
        }
    }

    void BatExplode(float _distanceToPlayer)
    {
        rb.velocity = Vector2.zero;

        if (timeToExplodeValue <= 0)
        {
            audioManager.Play("BatExplosion");

            CameraShaker.Instance.ShakeOnce(8, 5, 0.1f, 0.5f);

            if (_distanceToPlayer <= explodeRange)
            {
                player.Hurt(enemies.damage);
            }
            Destroy(gameObject);
        }
        else
        {
            timeToExplodeValue -= Time.deltaTime;
            Flashing();
        }
    }

    void ExplodeFlashing()
    {
        if (sr.material.color == triggerMaterial.color)
        {
            flashTimeValue -= Time.deltaTime;
        }

        if (flashTimeValue <= 0)
        {
            sr.material = defaultMaterial;
            flashTimeValue = flashTime;
        }

        if (timeBtwFlashValue <= 0)
        {
            sr.material = triggerMaterial;
            timeBtwFlashValue = timeBtwFlash;
        }
        else
        {
            timeBtwFlashValue -= Time.deltaTime;
        }
    }
    #endregion

    /*void AlienMovement()
    {
        int groundMask = LayerMask.GetMask("Ground");

        int playerMask = LayerMask.GetMask("Player");

        Vector2 difference = player.transform.position - weapon.transform.position;

        RaycastHit2D groundCheck = Physics2D.Raycast(transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, -sr.bounds.extents.y - 0.01f, 0),
            -transform.up, rayLength, groundMask);
        Debug.DrawLine(transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, -sr.bounds.extents.y - 0.01f, 0),
            transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, -sr.bounds.extents.y - .01f - rayLength, 0), Color.red);

        RaycastHit2D wallCheck = Physics2D.Raycast(transform.position + new Vector3((sr.bounds.extents.x + .01f) * transform.right.x, 0, 0), transform.right, rayLength, groundMask);
        Debug.DrawLine(transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, 0, 0),
            transform.position + new Vector3((sr.bounds.extents.x + rayLength) * transform.right.x, 0, 0), Color.green);

        RaycastHit2D playerCheck = Physics2D.Raycast(transform.position + new Vector3((sr.bounds.extents.x + .01f) * transform.right.x, 0, 0), 
            player.transform.position - transform.position, attackRange);
        Debug.DrawLine(transform.position + new Vector3((sr.bounds.extents.x + .01f) * transform.right.x, 0, 0), 
            transform.position + new Vector3((sr.bounds.extents.x + .01f + attackRange) * transform.right.x, 0, 0), Color.green);

        LookAtPlayer();

        if (playerCheck && playerCheck.transform.tag == "Player" && groundCheck == true)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);

            float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

            if (player.transform.position.x <= transform.position.x)
            {
                currentAngle = Mathf.MoveTowardsAngle(currentAngle, 180 - rotationZ, 360);
            }
            else
                currentAngle = Mathf.MoveTowardsAngle(currentAngle, rotationZ, 360);

            weapon.transform.localEulerAngles = new Vector3(0, 0, currentAngle);

            weapon.transform.position = new Vector3(
             arm.transform.position.x + Mathf.Sin(currentAngle * Mathf.Deg2Rad) * Vector2.Distance(arm.transform.position, weapon.transform.position),
             arm.transform.position.y + Mathf.Cos(currentAngle * Mathf.Deg2Rad) * Vector2.Distance(arm.transform.position, weapon.transform.position)
         );

            anim.SetBool("isRunning", false);
            anim.SetBool("isShooting", true);

            weapon.projectilePrefab.isEnemy = true;

            weapon.ShootProjectile("FlyingAlienBullet", "PlayerShoot");

            return;
        }
        else
        {
            weapon.transform.localEulerAngles = Vector3.zero;
            anim.SetBool("isShooting", false);
        }

        if (wallCheck || player.transform.position.x == transform.position.x)
        {
            rb.velocity = Vector2.zero;

            anim.SetBool("isRunning", false);

            return;
        }

        rb.velocity = new Vector2(transform.right.x * speed, rb.velocity.y);

        anim.SetBool("isRunning", true);
    }*/


    int groundMask;
    float rayLength;
    Vector2 wallRayPos;
    Vector2 groundRayPos;
    RaycastHit2D groundCheck, wallCheck, playerCheck;
    bool _canChase = false;

    void AlienMovement()
    {
        AlienSetup();
        AlienStateMachine();
    }

    void AlienSetup()
    {
        groundMask = LayerMask.GetMask("Ground");
        rayLength = .5f;
        wallRayPos = transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, -sr.bounds.extents.y - 0.01f, 0);
        groundCheck = Physics2D.Raycast(wallRayPos, Vector2.down, rayLength, groundMask);
        groundRayPos = transform.position + new Vector3((sr.bounds.extents.x + .01f) * transform.right.x, 0, 0);
        wallCheck = Physics2D.Raycast(groundRayPos, transform.right, rayLength, groundMask);
    }

    void AlienStateMachine()
    {
        Vector3 lastSeenPos = new Vector3();
        if (playerCheck && groundCheck)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            AlienAttack();
            lastSeenPos = player.transform.position;
            _canChase = true;
        }
        else if (_canChase)
        {
            AlienChase(lastSeenPos);
            _canChase = false;
        }
        else
        {
            AlienPatrol();
        }
    }

    void AlienPatrol()
    {
        rb.velocity = transform.right * speed;
        if (!groundCheck || wallCheck)
        {
            transform.eulerAngles = new Vector3(0, 0, 180);
        }
    }

    void AlienChase(Vector3 _lastSeenPos)
    {
        rb.velocity = (_lastSeenPos - transform.position).normalized * speed * Time.deltaTime;
    }

    void AlienAttack()
    {
        Vector2 difference = player.transform.position - weapon.transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        weapon.projectilePrefab.isEnemy = true;
        weapon.ShootProjectile("FlyingAlienBullet", "PlayerShoot");
        return;
    }

    #region MaggotLogic
    void MaggotMovement()
    {
        rb.velocity = transform.right * speed;
    }
    void WallCheck()
    {
        touchingWall = Physics2D.OverlapCircle(wallCheckPos.position, radius, whatIsGround);

        if (touchingWall)
        {
            transform.eulerAngles += new Vector3(0, 0, 90);
        }
    }
    #endregion

    void MoveTowardPlayer()
    {
        if (((player.transform.position - transform.position).sqrMagnitude <= attackRange * attackRange && canCharge))
        {
            ChargeAttack();

            return;
        }
        else if ((isFlashing && canCharge))
        {
            ChargeAttack();

            return;
        }
        else if (isAttack)
        {
            if (dashTimeValue <= 0)
            {
                isAttack = false;
                canCharge = true;
            }

            dashTimeValue -= Time.deltaTime;

            return;
        }

        LookAtPlayer();

        rb.velocity = (player.transform.position - transform.position).normalized * speed * Time.deltaTime;
    }

    void ChargeAttack()
    {
        if (chargeTimeValue <= 0)
        {
            sr.material = defaultMaterial;
            Attack();
            return;
        }

        chargeTimeValue -= Time.deltaTime;

        rb.velocity = Vector2.zero;

        Flashing();
    }

    void Attack()
    {
        Vector2 target = player.transform.position - rb.transform.position;

        rb.velocity = target.normalized * dashSpeed * Time.deltaTime;

        canCharge = false;

        isAttack = true;

        isFlashing = false;

        dashTimeValue = dashTime;

        chargeTimeValue = chargeTime;
    }

    public void LookAtPlayer()
    {
        if (player.transform.position.x <= transform.position.x)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (player.transform.position.x > transform.position.x)
        {
            transform.eulerAngles = Vector3.zero;
        }
    }

    void Flashing()
    {
        isFlashing = true;

        if (sr.material.color == triggerMaterial.color)
        {
            flashTimeValue -= Time.deltaTime;
        }

        if (flashTimeValue <= 0)
        {
            sr.material = defaultMaterial;
            flashTimeValue = flashTime;
        }

        if (Time.time >= timeBtwFlashValue)
        {
            sr.material = triggerMaterial;
            timeBtwFlashValue = Time.time + timeBtwFlash;
        }
    }

    public void KnockBack(Vector2 _knockbackForce)
    {
        if (knockbackTime == 0)
        {
            return;
        }
        knockbackCounter = knockbackTime;
        knockbackForce = _knockbackForce;
        rb.velocity = _knockbackForce;
    }

    Vector2 GetBQCPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector2 p = (uu * p0) + (2 * u * t * p1) + (tt * p2);
        return p;
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyType == EnemyType.Maggot)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheckPos.position, radius);
        }
    }
}
