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

    enum EnemyState { Laser, Rotate, Shoot, Charge, Slam }
    EnemyState state;

    [Header("General Stats")]
    public int damage;
    private int maxHealth;
    public GameObject explodeEffect;
    public int projectileDamage;
    public float speed;
    public float slamSpeed;
    public float knockbackForce;

    Material defMat;
    public Material triggerMat, whiteMat;
    public BoolReference onGround; // true when player's gravity is normal

    public float timeBtwBullets;
    private bool canShoot;
    private Transform[] positions = new Transform[8];

    [Header("Laser Value")]
    public GameObject laser; // For EnemyState.Laser
    public Laser[] lasers; // For EnemyState.Rotate
    public int laserDamage;

    [Header("Positions")]
    public Transform groundPos;
    public Transform ceilingPos;
    public Transform eyePos;
    public Transform[] restPositions = new Transform[4];

    [Header("Time Value")]
    public float chargeTime;
    private float chargeTimeValue;
    public float flashTime;
    public float timeBtwFlashes;
    private IEnumerator flashCoroutine;

    [Header("Other Value")]
    public float fakeAttackProbability;
    private float timer;
    private bool canFakeAttack;

    public float playerHeight;
    public float shockWaveHeight;

    public const float distanceToSizeRatio = 6.25f; // 1 distance unit == 6.25 laser scale unit

    bool canSetup = true;
    int dir; // Direction (up or down) when the boss slap
    float posY; // ground or ceiling position, only calculate this one time before teleport to player

    // Start is called before the first frame update
    protected override void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        sr = GetComponent<SpriteRenderer>();
        defMat = sr.material;
        rb = GetComponent<Rigidbody2D>();
        chargeTimeValue = chargeTime;
        maxHealth = health;
        flashCoroutine = Flashing();

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
        switch (state)
        {
            case EnemyState.Charge:
                if (chargeTimeValue == chargeTime)
                    dir = onGround.value ? 1 : -1;

                rb.velocity = Vector2.zero;
                chargeTimeValue -= Time.deltaTime;

                if (chargeTimeValue <= 0)
                {
                    chargeTimeValue = chargeTime;
                    StopCoroutine(flashCoroutine);
                    sr.material = defMat;
                    state = EnemyState.Slam;
                }
                break;

            case EnemyState.Slam:
                Slam();
                break;

            case EnemyState.Laser:
                ShootLaser();
                break;

            case EnemyState.Rotate:
                if (canSetup)
                {
                    TeleportToCenter();
                    SetupLasers();

                    rb.velocity = Vector2.zero;
                    timer = Random.Range(4, 6);
                    AudioManager.instance.PlayMusic("Laser");
                    canSetup = false;
                }

                timer -= Time.deltaTime;

                if (timer <= 0)
                {
                    foreach (var laser in lasers)
                        laser.gameObject.SetActive(false);

                    AudioManager.instance.StopMusic();
                    canSetup = true;
                    ResetSlamAttack();
                }
                else
                    foreach (var laser in lasers)
                        laser.transform.Rotate(new Vector3(0, 0, 40 * Time.deltaTime));
                break;
            case EnemyState.Shoot:
                if (!canShoot)
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
                    eyePos.rotation = Quaternion.identity;
                }
                break;
        }
    }

    void TeleportToPlayer()
    {
        float positionY = onGround.value ? groundPos.position.y + playerHeight : ceilingPos.position.y - playerHeight;
        transform.position = new Vector3(player.transform.position.x, positionY, 0);
        AudioManager.instance.PlaySfx("Teleport");
        StartCoroutine(flashCoroutine);
        canFakeAttack = MathUtils.RandomBool(fakeAttackProbability);
        state = EnemyState.Charge;
    }

    public IEnumerator Flashing()
    {
        while (true)
        {
            sr.material = triggerMat;
            yield return new WaitForSeconds(flashTime);
            sr.material = defMat;
            yield return new WaitForSeconds(timeBtwFlashes);
        }
    }

    void Slam()
    {
        rb.velocity = Vector2.down * slamSpeed * dir;

        if (canFakeAttack)
        {
            timer += Time.deltaTime;
        }
        if (timer > .25f)
        {
            if ((onGround.value && dir == -1) || (!onGround.value && dir == 1))
            {
                canFakeAttack = false;
                timer = 0;
            }
            else
            {
                ResetSlamAttack();
            }
        }
    }

    void ResetSlamAttack()
    {
        timer = 0;
        TeleportToPlayer();
    }

    void CreateShockWave()
    {
        float positionY = (dir > 0) ? groundPos.position.y + shockWaveHeight/2 : ceilingPos.position.y - shockWaveHeight/2;
        float rotationX = onGround.value ? 0 : 180;
        Vector3 position = new Vector3(transform.position.x, positionY, 0);
        Projectile projectile1 = ObjectPooler.instance.SpawnFromPool<Projectile>("Shockwave", position, Quaternion.Euler(rotationX, 0, 0));
        projectile1.Init(damage, 0, 0, true, false);
        Projectile projectile2 = ObjectPooler.instance.SpawnFromPool<Projectile>("Shockwave", position, Quaternion.Euler(rotationX, 180, 0));
        projectile2.Init(damage, 0, 0, true, false);
    }

    void TeleportToRestPos()
    {
        int i = onGround.value ? (Random.value < .5f ? 0 : 3) : Random.Range(1, 3);
        transform.position = restPositions[i].position;
        AudioManager.instance.PlaySfx("Teleport");
    }

    void ShootLaser()
    {
        if (timer == 0)
        {
            TeleportToRestPos();
            rb.velocity = Mathf.Sign(player.transform.position.x - transform.position.x) * new Vector2(speed, 0);
            AudioManager.instance.PlayMusic("Laser");
            laser.SetActive(true);
            posY = onGround.value ? groundPos.position.y : ceilingPos.position.y;
        }
        timer += Time.deltaTime;
        bool stopLaser = MathUtils.RandomBool(timer / 2.4f) && timer >= 1.2f;
        if (stopLaser)
        {
            laser.SetActive(false);
            AudioManager.instance.StopMusic();
        }

        laser.transform.position = new Vector3(eyePos.position.x, MathUtils.Average(eyePos.position.y, posY), 0);
        laser.transform.localScale = new Vector3(laser.transform.localScale.x, Mathf.Abs(eyePos.position.y - posY) * distanceToSizeRatio, 1);
        laser.transform.rotation = Quaternion.identity;
    }

    void TeleportToCenter()
    {
        Vector3 center = MathUtils.Average(ceilingPos.position, groundPos.position);
        transform.position = center;
        transform.position += center - eyePos.position; // eyePos is at the center
        AudioManager.instance.PlaySfx("Teleport");
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
                bullet.Init(damage, 0, 0, true, false);
            }
            AudioManager.instance.PlaySfx("Shoot");
            yield return new WaitForSeconds(timeBtwBullets);
            eyePos.Rotate(new Vector3(0, 0, 10));
        }
    }

    public override void Hurt(int _damage)
    {
        health -= _damage;
        sr.material = whiteMat;
        Invoke("ResetMaterial", .1f);
        AudioManager.instance.PlaySfx("GetHit");
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
            if (state == EnemyState.Slam)
            {
                CreateShockWave();
                AudioManager.instance.PlaySfx("Slam");
                int maxAttack = health <= maxHealth * .5f ? 3 : 2;
                int randAttack = Random.Range(0, maxAttack);
                switch (randAttack)
                {
                    case (int)EnemyState.Laser:
                        timer = 0;
                        state = EnemyState.Laser;
                        break;
                    case (int)EnemyState.Rotate:
                        state = EnemyState.Rotate;
                        break;
                    case (int)EnemyState.Shoot:
                        state = EnemyState.Shoot;
                        break;
                }
            }
            else if (state == EnemyState.Laser)
            {
                ResetSlamAttack();
            }
        }
    }
}
