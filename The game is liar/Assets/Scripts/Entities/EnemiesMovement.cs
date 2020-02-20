using EZCameraShake;
using UnityEngine;

public class EnemiesMovement : MonoBehaviour
{

    private Rigidbody2D rb;

    public float speed;

    private float timer;

    private Player player;

    private Enemies enemies;
    public EnemyType enemyType;

    public Material triggerMaterial;
    private SpriteRenderer sr;
    private Material defaultMaterial;

    private AudioManager audioManager;

    private Animator anim;
    float currentAngle = 0;

    #region MaggotVariables
    // Maggot
    private bool touchingWall;
    private RaycastHit2D hitInfo;
    BoxCollider2D box;
    

    public Transform wallCheck;
    public float radius;
    public LayerMask whatIsGround;
    #endregion

    #region BatVariables
    // Bat
    public Transform curve_point;
    private Vector2 point;

    public float distanceToExplode;
    public float explodeRange;
    public float timeToExplode;
    public float distanceToChase;
    private float timeToExplodeValue;
    private bool canChase = false;
    private bool canExplode = false;

    public float timeBtwFlash;
    private float timeBtwFlashValue;
    public float flashTime;
    private float flashTimeValue;
    #endregion

    #region Knockback
    // Knock back
    public float knockbackTime;
    private float knockbackCounter;
    private Vector2 knockbackForce;
    private bool knockback;
    #endregion

    private Weapon weapon;
    private Transform arm;

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
        audioManager = FindObjectOfType<AudioManager>();
        if (enemyType == EnemyType.Alien)
        {
            arm = transform.Find("Arm").GetComponent<Transform>();
            weapon = transform.Find("AlienPistol").GetComponent<Weapon>();
            Debug.Log(weapon.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        WallCheck();
    }
    
    void FixedUpdate()
    {
        if (knockbackCounter > 0)
        {
            rb.velocity = new Vector2(knockbackForce.x, rb.velocity.y);
            knockbackCounter -= Time.deltaTime;

            if (knockbackCounter < 0)
            {
                knockback = false;
            }

            if (knockback == false)
            {
                rb.velocity = Vector2.zero;
                knockback = true;
            }

            return;
        }

        if (enemyType == EnemyType.Maggot)
        {
            MaggotMovement();
        }
        else if (enemyType == EnemyType.Bat)
        {
            BatMovement();
        }
        else if (enemyType == EnemyType.Alien)
        {
            AlienMovement();
        }
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(wallCheck.position, radius);
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
            rb.velocity = new Vector2(point.x - transform.position.x, point.y - transform.position.y).normalized * speed;
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
            ExplodeFlashing();
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

    void AlienMovement()
    {
        float rayLength = .4f;

        float attackRange = 8f;

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

        if (player.transform.position.x <= transform.position.x)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }

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

            weapon.ShootProjectile();

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
    }

    #region MaggotLogic
    void MaggotMovement()
    {
        rb.velocity = transform.right * speed;
    }
    void WallCheck()
    {
        if (enemyType == EnemyType.Maggot)
        {
            touchingWall = Physics2D.OverlapCircle(wallCheck.position, radius, whatIsGround);
            //RaycastHit2D hitInfo;
            //hitInfo = Physics2D.Raycast(transform.position - transform.up.normalized - transform.right.normalized, -transform.up, .5f);
            //Debug.DrawRay(transform.position - transform.up.normalized - transform.right.normalized, -transform.up, Color.red);

            if (touchingWall)
            {
                transform.eulerAngles += new Vector3(0, 0, 90);
            }
            //else if (!hitInfo)
            //{
            //    transform.eulerAngles -= new Vector3(0, 0, 90);
            //}
        }
    }
    #endregion

    public void KnockBack(Vector2 _knockbackForce)
    {
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
}
