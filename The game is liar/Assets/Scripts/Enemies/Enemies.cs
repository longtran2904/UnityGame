using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Enemies : MonoBehaviour
{
    public static int numberOfEnemiesAlive = 0;

    public int health;
    public int damage;
    public float speed;
    public float attackRange;
    public float distanceToChase;

    public DropRange moneyDropRange;
    public Material matWhite;

    public bool hasSpawnVFX;
    public GameObject explosionParitcle;
    public GameObject spawnEffect;

    public EnemyState startState;
    public EnemyState optionalState;
    public Stack<EnemyState> allStates = new Stack<EnemyState>();

#if UNITY_EDITOR
    [Header("Enemy's Ability")]
    public bool canDash;
    public bool canShoot;
    public bool canFlash;
    public bool curveMove;
    public bool canSplit;
    public bool canExplode;
#endif

    [Header("Enemy's Value")]
    [ShowWhen("canDash")] public float dashSpeed;

    [ShowWhen("canShoot")] public Vector3 shootPos;

    [ShowWhen("canFlash")] public Material triggerMat;
    [ShowWhen("canFlash")] public float flashTime;
    [ShowWhen("canFlash")] public float timeBtwFlashes;

    [ShowWhen("curveMove")] public Transform curvePoint;

    [ShowWhen("canSplit")] public int numberOfSplits = 2;
    [ShowWhen("canSplit")] public Enemies splitEnemy;

    [ShowWhen("canExplode")] public bool explodeWhenDie;
    [ShowWhen("canExplode")] public bool explodeWhenInRange;
    [ShowWhen("canExplode")] public GameObject explodeParticle;
    [ShowWhen("canExplode")] public float explodeRange;
    [ShowWhen("explodeWhenInRange")] public float explodeTime;
    [ShowWhen("canExplode")] public string explodeSound;

    public Rigidbody2D rb { get; private set; }
    public Player player { get; private set; }
    public Animator anim { get; private set; }
    public SpriteRenderer sr { get; private set; }
    public Material defMat { get; private set; }

    private EnemyState nextState;

    void Start()
    {
        numberOfEnemiesAlive += 1;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = (GetComponent<SpriteRenderer>() != null) ? GetComponent<SpriteRenderer>() : GetComponentInChildren<SpriteRenderer>();
        defMat = sr.material;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        allStates.Push(startState);
        startState.Init(this);
        optionalState?.Init(this);
        StartCoroutine(SpawnEnemy());
    }

    IEnumerator SpawnEnemy()
    {
        if (hasSpawnVFX)
        {
            // Disable behaviours and child objects
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.enabled = false;
            float startGravityScale = GetComponent<Rigidbody2D>().gravityScale;
            GetComponent<Rigidbody2D>().gravityScale = 0;
            foreach (var component in GetComponents<Behaviour>())
            {
                if (component == this)
                {
                    continue;
                }
                if (!component)
                {
                    InternalDebug.Log(component);
                }
                component.enabled = false;
            }
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            Destroy(Instantiate(spawnEffect, transform.position, Quaternion.identity), 1);
            yield return new WaitForSeconds(1);

            // Enable components and child objects
            sr.enabled = true;
            GetComponent<Rigidbody2D>().gravityScale = startGravityScale;
            foreach (var component in GetComponents<Behaviour>())
            {
                component.enabled = true;
            }
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
            hasSpawnVFX = false;
            InternalDebug.Break();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            if (explodeWhenDie)
            {
                Explode();
            }
            Die();
        }

        if (explodeWhenInRange && IsInRange())
        {
            StartCoroutine(StartExploding());
        }

        LookAtPlayer();

        if (!hasSpawnVFX)
        {
            InternalDebug.Log(allStates.Peek());
            if (nextState != null)
            {
                allStates.Push(nextState);
                nextState.Init(this);
            }
            nextState = allStates.Peek().UpdateState(this);
            optionalState?.UpdateState(this);
        }
    }

    public void Die()
    {
        GameObject explosion = explosionParitcle;
        Instantiate(explosion, transform.position, Quaternion.identity);
        DropMoney(moneyDropRange.GetRandom());
        if (splitEnemy && numberOfSplits > 0)
        {
            Split();
        }
        numberOfEnemiesAlive--;
        Destroy(gameObject);
    }

    protected void DropMoney(int dropMoney)
    {
        for (int i = 0; i < dropMoney; i++)
        {
            ObjectPooler.instance.SpawnFromPool("Money", transform.position, UnityEngine.Random.rotation);
        }
    }

    public void Hurt(int _damage, Vector2 knockbackForce, float knockbackTime)
    {
        health -= _damage;
        AudioManager.instance.Play("GetHit");
        KnockbackState knockback = new KnockbackState(knockbackForce, knockbackTime);
        allStates.Push(knockback);
        knockback.Init(this);
        StartCoroutine(ResetMaterial());

        IEnumerator ResetMaterial()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.material = matWhite;
            yield return new WaitForSeconds(.1f);
            sr.material = defMat;
        }
    }

    public IEnumerator StartExploding()
    {
        rb.velocity = Vector2.zero;
        StartCoroutine(Flashing(explodeTime));
        yield return new WaitForSeconds(explodeTime);
        Explode();
        Die();
    }

    private void Explode()
    {
        AudioManager.instance.Play(explodeSound);
        EZCameraShake.CameraShaker.Instance.ShakeOnce(8, 5, 0.1f, 0.5f);
        GameObject explodeVFX = Instantiate(explodeParticle, transform.position, Quaternion.identity);
        explodeVFX.transform.localScale = new Vector3(6.25f, 6.25f, 0) * explodeRange;
        Destroy(explodeVFX, .3f);
        if ((player.transform.position - transform.position).sqrMagnitude < explodeRange * explodeRange)
        {
            player.Hurt(damage);
        }
    }

    public IEnumerator Flashing(float duration)
    {
        while (duration <= 0)
        {
            sr.material = triggerMat;
            yield return new WaitForSeconds(flashTime);

            sr.material = defMat;
            yield return new WaitForSeconds(timeBtwFlashes);

            duration -= Time.deltaTime;
        }
    }

    public void TeleportToRandomPos(int maxTry, float minDistance, float maxDistance)
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

    public void Split()
    {
        Vector3 offset = new Vector3(.5f, 0, 0);
        Object.Instantiate(splitEnemy, transform.position + offset, Quaternion.identity).hasSpawnVFX = false;
        Object.Instantiate(splitEnemy, transform.position + offset, Quaternion.identity).hasSpawnVFX = false;
    }

    public bool GroundCheck()
    {
        Vector2 pos = (Vector2)transform.position - new Vector2(0, sr.bounds.extents.y * transform.up.y);
        Vector2 size = new Vector2(sr.bounds.size.x, 0.1f);
        return Physics2D.BoxCast(pos, size, 0, -transform.up, 0, LayerMask.GetMask("Ground"));
    }

    public bool WallCheck(float range)
    {
        Vector2 pos = transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, 0, 0);
        return Physics2D.Raycast(pos, transform.right, range, LayerMask.GetMask("Ground"));
    }

    public bool PlayerCheck()
    {
        Vector2 pos = transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, 0, 0);
        return Physics2D.Raycast(pos, transform.right, attackRange, LayerMask.GetMask("Player"));
    }

    public float CaculateRotationToPlayer()
    {
        Vector2 difference = -transform.position + player.transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        return rotationZ;
    }

    void LookAtPlayer()
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

    public bool IsInRange()
    {
        return (player.transform.position - transform.position).sqrMagnitude < attackRange * attackRange;
    }
}
