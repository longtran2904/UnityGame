using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesMovement : MonoBehaviour
{

    private Rigidbody2D rb;

    public float speed;

    private bool touchingWall;

    private RaycastHit2D hitInfo;

    BoxCollider2D box;

    public Transform wallCheck;

    public float radius;

    public LayerMask whatIsGround;

    public EnemyType enemyType;

    private float timer;

    private Player player;

    public Transform curve_point;

    private Vector2 point;

    private bool touchPlayer;

    public float distanceToExplode;

    public float explodeRange;

    public float timeToExplode;

    private float timeToExplodeValue;

    private Enemies enemies;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        enemies = GetComponent<Enemies>();
        timeToExplodeValue = timeToExplode;
    }

    // Update is called once per frame
    void Update()
    {
        WallCheck();
    }
    
    void FixedUpdate()
    {
        if (enemyType == EnemyType.Maggot)
        {
            MaggotMovement();
        }
        else if (enemyType == EnemyType.Bat)
        {
            BatMovement();
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            touchPlayer = true;
        }
    }

    void WallCheck()
    {
        if (enemyType == EnemyType.Maggot)
        {
            touchingWall = Physics2D.OverlapCircle(wallCheck.position, radius, whatIsGround);
            if (touchingWall)
            {
                transform.eulerAngles += new Vector3(0, 0, 90);
            }
        }
    }

    void BatMovement()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= distanceToExplode || timeToExplodeValue <= 1)
        {
            BatExplode(distanceToPlayer);
        }
        else if (timer < 1 && timeToExplodeValue >= .8f)
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
            if (_distanceToPlayer <= explodeRange)
            {
                player.health -= enemies.damage;
            }
            Destroy(gameObject);
        }
        else
        {
            timeToExplodeValue -= Time.deltaTime;
        }
    }

    void MaggotMovement()
    {
        rb.velocity = transform.right * speed;
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
