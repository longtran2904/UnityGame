using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBrain : Boss
{
    /*
     * The boss logic:
     * When prepared to attack will
     *      1. Flash: Done
     *      2. Teleport to above (or below) the player: Done
     *      3. Slam downwards (or upwards): Done
     *      4. Has a chance to fake it and teleport again: Done
     *      5. When hit the ground will create a sock wave to damage the player: Done
     * Can shoot four beam of laser for 4 directions and rotate in a circle: Done
     * Can shoot a laser beam and move left of right while shooting: Done
     * Shoot multiple projectiles in a circle: Done
     */

    enum AttackType { SLAM, LASER, ROTATE, SHOOT }

    private int maxHealth;
    public int damage;
    public GameObject explodeEffect;
    public int projectileDamage;
    public float speed;
    public float slamSpeed;

    public float playerHeight;
    private float playerHeightValue;

    public float timeBtwBullets;
    private bool canShoot;
    private Transform[] positions = new Transform[8];

    public GameObject laser;
    public Laser[] lasers;
    public int laserDamage;

    public Transform shootPos;
    public Transform groundPos;
    public Transform ceilingPos;
    public Transform eyePos;

    public Transform[] restPositions = new Transform[4];

    public float chargeTime;
    private float chargeTimeValue;
    public float flashTime;
    private float flashTimeValue;
    public float timeBtwFlash;
    private float timeBtwFlashValue;

    public float fakeAttackProbability;
    private float timer;
    private bool canFakeAttack;

    Material defMat;
    public Material triggerMat, whiteMat;
    bool canTeleport;
    bool canAttack;
    bool onGround = true; // true if the player is not upside down
    bool canRest;
    AttackType attackType;
    public const float distanceToSizeRatio = 6.25f; // 1 distance unit == 6.25 laser scale unit

    // Start is called before the first frame update
    protected override void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        sr = GetComponent<SpriteRenderer>();
        defMat = sr.material;
        rb = GetComponent<Rigidbody2D>();
        playerHeightValue = playerHeight;
        chargeTimeValue = chargeTime;
        timeBtwFlashValue = timeBtwFlash;
        flashTimeValue = flashTime;
        maxHealth = health;

        // Testing
        ResetSlamAttack();
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (health <= 0)
        {
            Die();
        }
        Attack();
    }

    protected override void Die()
    {
        Instantiate(explodeEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    void Attack()
    {
        if (attackType == AttackType.SLAM)
        {
            if (canTeleport)
            {
                TeleportToPlayer();
            }
            if (!canAttack)
            {
                return;
            }
            if (chargeTimeValue <= 0)
            {
                sr.material = defMat;
                Slam();
                return;
            }
            chargeTimeValue -= Time.deltaTime;
            rb.velocity = Vector2.zero;
            Flashing();
        }
        else if (attackType == AttackType.LASER)
        {
            if (canRest)
            {
                onGround = !player.controller.top;
                TeleportToRestPos();
                int dir = -1;
                if (transform.position == restPositions[0].position || transform.position == restPositions[1].position)
                    dir = 1;
                rb.velocity = Vector2.right * dir * speed;
                AudioManager.instance.Play("Laser");
                canRest = false;
            }
            ShootLaser();
        }
        else if (attackType == AttackType.ROTATE)
        {
            if (canTeleport)
            {
                TeleportToCenter();
                SetupLasers();
                rb.velocity = Vector2.zero;
                timer = Random.Range(4, 6);
                AudioManager.instance.Play("Laser");
            }
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                ResetSlamAttack();
                foreach (var laser in lasers)
                {
                    laser.gameObject.SetActive(false);
                }
                AudioManager.instance.Stop("Laser");
            }
            else
                RotateLasers();
        }
        else if (attackType == AttackType.SHOOT)
        {
            if (canTeleport)
            {
                TeleportToCenter();
                CreateShootPos();
                canShoot = true;
                rb.velocity = Vector2.zero;
                timer = Random.Range(5, 8);
                StartCoroutine(ShootProjectiles());
            }
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                ResetSlamAttack();
                canShoot = false;
            }
        }
    }

    void TeleportToPlayer()
    {
        onGround = !player.controller.top;
        if (!onGround)
        {
            transform.position = new Vector3(player.transform.position.x, ceilingPos.position.y - playerHeightValue, 0);
        }
        else
        {
            transform.position = new Vector3(player.transform.position.x, groundPos.position.y + playerHeightValue, 0);
        }
        canAttack = true;
        canTeleport = false;
        canFakeAttack = MathUtils.RandomBool(fakeAttackProbability);
        AudioManager.instance.Play("Teleport");
    }

    void Flashing()
    {
        if (sr.material.color == triggerMat.color)
        {
            flashTimeValue -= Time.deltaTime;
        }
        if (flashTimeValue <= 0)
        {
            sr.material = defMat;
            flashTimeValue = flashTime;
        }
        if (Time.time >= timeBtwFlashValue)
        {
            sr.material = triggerMat;
            timeBtwFlashValue = Time.time + timeBtwFlash;
        }
    }

    void Slam()
    {
        int dir = 1;
        if (!onGround) dir = -1;
        rb.velocity = Vector2.down * slamSpeed * dir;

        if (canFakeAttack)
        {
            timer += Time.deltaTime;
        }
        if (timer > .25f)
        {
            ResetSlamAttack();
            timer = 0;
            canFakeAttack = false;
        }
    }

    void ResetSlamAttack()
    {
        attackType = AttackType.SLAM;
        canAttack = false;
        canTeleport = true;
        chargeTimeValue = chargeTime;
        timer = 0;
    }

    void CreateShockWave()
    {
        float offsetY = 0;
        float rotationX = 0;
        if (!onGround)
        {
            offsetY = ceilingPos.position.y - (shootPos.position.y - groundPos.position.y); // Spawn at the ceiling minus shootPos
            offsetY -= shootPos.position.y; // I need this because I will addd shootPos.position.y later on
            rotationX = 180f;
        }
        Vector3 position = new Vector3(transform.position.x, shootPos.position.y + offsetY, 0);
        Projectile projectile1 = ObjectPooler.instance.SpawnFromPool<Projectile>("Shockwave", position, Quaternion.Euler(rotationX, 0, 0));
        projectile1.Init(damage, Vector2.zero, null, true, false);
        Projectile projectile2 = ObjectPooler.instance.SpawnFromPool<Projectile>("Shockwave", position, Quaternion.Euler(rotationX, 180, 0));
        projectile2.Init(damage, Vector2.zero, null, true, false);
    }

    void TeleportToRestPos()
    {
        int i = 0;
        if (!onGround)
            i = Random.Range(1, 3);
        else
        {
            if (Random.value < .5f)
                i = 0;
            else
                i = 3;
        }
        transform.position = restPositions[i].position;
        timer = 0;
        AudioManager.instance.Play("Teleport");
    }

    void ShootLaser()
    {
        if (timer == 0)
        {
            laser.SetActive(true);
        }
        timer += Time.deltaTime;
        if (MathUtils.RandomBool(timer / 2.4f) && timer >= 1.2f)
        {
            laser.SetActive(false);
            AudioManager.instance.Stop("Laser");
        }

        if (!onGround)
        {
            Vector3 pos = new Vector3(eyePos.position.x, MathUtils.Average(eyePos.position.y, ceilingPos.position.y), 0);
            float size = Mathf.Abs(eyePos.position.y - ceilingPos.position.y) * distanceToSizeRatio;
            CreateLaser(pos, size);
        }
        else
        {
            Vector3 pos = new Vector3(eyePos.position.x, MathUtils.Average(eyePos.position.y, groundPos.position.y), 0);
            float size = Mathf.Abs(eyePos.position.y - groundPos.position.y) * distanceToSizeRatio;
            CreateLaser(pos, size);
        }
        
        void CreateLaser(Vector3 pos, float size)
        {
            laser.transform.position = pos;
            laser.transform.localScale = new Vector3(laser.transform.localScale.x, size, 1);
            laser.transform.rotation = Quaternion.identity;
        }
    }

    void TeleportToCenter()
    {
        Vector3 center = MathUtils.Average(ceilingPos.position, groundPos.position);
        transform.position = center;
        transform.position += center - eyePos.position; // eyePos is at the center
        canTeleport = false;
        AudioManager.instance.Play("Teleport");
    }

    void SetupLasers()
    {
        Vector3 center = MathUtils.Average(ceilingPos.position, groundPos.position);
        foreach (var laser in lasers)
        {
            laser.transform.position = center;
            laser.transform.localScale = new Vector3(laser.transform.localScale.x, 400);
            laser.damage = laserDamage;
            laser.gameObject.SetActive(true);
        }
        lasers[0].transform.eulerAngles = new Vector3(0, 0, 90);
        lasers[1].transform.rotation = Quaternion.identity;
    }

    void RotateLasers()
    {
        foreach (var laser in lasers)
        {
            laser.transform.Rotate(new Vector3(0, 0, 40 * Time.deltaTime));
        }
    }

    void CreateShootPos()
    {
        float distance = 5;
        Vector3 center = MathUtils.Average(ceilingPos.position, groundPos.position);
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = new GameObject().transform;
            positions[i].SetParent(eyePos);
        }
        positions[0].position = center + new Vector3(0, distance);
        positions[1].position = center + new Vector3(distance, 0);
        positions[2].position = center - new Vector3(0, distance);
        positions[3].position = center - new Vector3(distance, 0);
        positions[4].position = center + new Vector3( 1,  1).normalized * distance;
        positions[5].position = center + new Vector3( 1, -1).normalized * distance;
        positions[6].position = center + new Vector3(-1, -1).normalized * distance;
        positions[7].position = center + new Vector3(-1,  1).normalized * distance;
    }

    IEnumerator ShootProjectiles()
    {
        while (canShoot)
        {
            foreach (var pos in positions)
            {
                pos.right = pos.position - eyePos.position;
                Projectile bullet = ObjectPooler.instance.SpawnFromPool<Projectile>("TurretBullet", pos.position, pos.rotation);
                bullet.Init(damage, Vector2.zero, null, true, false);
            }
            AudioManager.instance.Play("Shoot");
            yield return new WaitForSeconds(timeBtwBullets);
            eyePos.Rotate(new Vector3(0, 0, 10));
        }
    }

    public override void GetHurt(int _damage)
    {
        health -= _damage;
        sr.material = whiteMat;
        Invoke("ResetMaterial", .1f);
        AudioManager.instance.Play("GetHit");
    }

    void ResetMaterial()
    {
        sr.material = defMat;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player.Hurt(damage);
        }

        if (collision.CompareTag("Ground"))
        {
            if (attackType == AttackType.SLAM)
            {
                CreateShockWave();
                AudioManager.instance.Play("Slam");
                int maxAttack = 3;
                if (health <= maxHealth * .5f)
                    maxAttack = 4;
                int randAttack = Random.Range(1, maxAttack);
                if (randAttack == (int)AttackType.LASER)
                {
                    canRest = true;
                    attackType = AttackType.LASER;
                }
                else if (randAttack == (int)AttackType.ROTATE)
                {
                    canTeleport = true;
                    attackType = AttackType.ROTATE;
                }
                else if (randAttack == (int)AttackType.SHOOT)
                {
                    canTeleport = true;
                    attackType = AttackType.SHOOT;
                }
            }
            else if (attackType == AttackType.LASER)
            {
                ResetSlamAttack();
            }
        }
    }
}
