using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienMovement : EnemiesMovement
{
    float rayLength;
    Vector2 wallRayPos;
    Vector2 groundRayPos;
    RaycastHit2D groundCheck, wallCheck, playerCheck;
    bool _canChase = false;
    public Weapon weapon;
    int groundMask, playerMask;
    bool isPlayerDied;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        rayLength = .5f; // The length of the wall and ground raycast
        groundMask = LayerMask.GetMask("Ground");
        playerMask = LayerMask.GetMask("Player");
    }

    protected override void OnPlayerDeathEvent()
    {
        base.OnPlayerDeathEvent();
        isPlayerDied = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerDied) return;
        CheckForCollision();
        AlienStateMachine();
    }

    void CheckForCollision()
    {
        wallRayPos = transform.position + new Vector3((sr.bounds.extents.x + .01f) * transform.right.x, 0, 0);
        wallCheck = Physics2D.Raycast(wallRayPos, transform.right, rayLength, groundMask);

        groundRayPos = transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, -sr.bounds.extents.y - 0.01f, 0);
        groundCheck = Physics2D.Raycast(groundRayPos, Vector2.down, rayLength, groundMask);

        playerCheck = Physics2D.Raycast(wallRayPos, transform.right, attackRange, playerMask);
    }

    void AlienStateMachine()
    {
        Vector3 lastSeenPos = new Vector3();
        if (playerCheck && groundCheck)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            weapon.ShootProjectileForEnemy("FlyingAlienBullet", "PlayerShoot");
            lastSeenPos = player.transform.position;
            _canChase = true;
        }
        else if (_canChase)
        {
            rb.velocity = (lastSeenPos - transform.position).normalized * speed * Time.deltaTime;
            _canChase = false;
        }
        else
        {
            AlienPatrol();
        }
    }

    int offsetY = 180;
    void AlienPatrol()
    {
        rb.velocity = transform.right * speed;
        if (!groundCheck || wallCheck)
        {
            transform.eulerAngles = new Vector3(0, offsetY, 0);
            offsetY += 180;
        }
    }
}
