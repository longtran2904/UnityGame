using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public static int numberOfEnemiesAlive = 0;

    public IntReference health;
    public IntReference damage;
    public FloatReference speed;

    public RangedFloat moneyDrop;
    public Material matWhite;

    public GameObject explosionParitcle;

    public bool lookAtPlayer;
    public bool damageWhenCollide;

    public StateGraph graph;
    [HideInInspector] public EnemyState currentState;
    private EnemyState nextState;
    private EnemyState fromAnyState;

    // This is only for showing in the inspector
#if UNITY_EDITOR
    [Header("Enemy's Ability")]
    public bool canDash;
    public bool canFlash;
    public bool curveMove;
    public bool canSplit;
    public bool canExplode;
    public bool hasWeapon;
#endif

    [Header("Enemy's Value")]
    [ShowWhen("canDash")] public float dashSpeed;

    [ShowWhen("canFlash")] public Material triggerMat;
    [ShowWhen("canFlash")] public float flashTime;
    [ShowWhen("canFlash")] public float timeBtwFlashes;

    [ShowWhen("curveMove")] public Transform curvePoint;

    [ShowWhen("canSplit")] public int numberOfSplits = 2;
    [ShowWhen("canSplit")] public Enemy splitEnemy;

    [ShowWhen("canExplode")] public bool explodeWhenDie;
    [ShowWhen("canExplode")] public bool explodeWhenInRange;
    [ShowWhen("canExplode")] public GameObject explodeParticle;
    [ShowWhen("canExplode")] public float explodeRange;
    [ShowWhen("explodeWhenInRange")] public float explodeTime;
    [ShowWhen("canExplode")] public string explodeSound;

    [ShowWhen("hasWeapon")] public Weapon weapon;

    public Rigidbody2D rb { get; private set; }
    public Player player { get; private set; }
    public Animator anim { get; private set; }
    public SpriteRenderer sr { get; private set; }
    public Material defMat { get; private set; }

    void Start()
    {
        numberOfEnemiesAlive += 1;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        defMat = sr.material;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        graph.startNode.state.Enter(this);
        currentState = graph.startNode.state;

        // Spawn the weapon if has one
        // TODO: Abstract this to a function somewhere
        if (weapon)
        {
            weapon = Instantiate(weapon, transform.position, Quaternion.identity);
            weapon.transform.parent = transform;
            weapon.transform.localPosition = weapon.posOffset;
            // NOTE: The enemy has the exact gun prefab like the player so has to remove unnecessary component
            //       This is just for temporary. Should I make different guns for enemy?
            Destroy(weapon.GetComponent<ActiveReload>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (health.value <= 0)
        {
            if (explodeWhenDie)
                Explode();
            Die();
        }

        if (explodeWhenInRange && IsInRange(explodeRange)) StartCoroutine(StartExploding());

        if (lookAtPlayer) LookAtPlayer();
        else transform.rotation = Quaternion.Euler(0, rb.velocity.x > 0 ? 0 : 180, 0);

        ControlState();
    }

    private void ControlState()
    {
        if (nextState != null)
        {
            nextState.Enter(this);
            currentState = nextState;
        }
        else if (fromAnyState) // The current state was previouslly transit from "Any State" and will transit back to the most recent not-from-any state
        {
            nextState = fromAnyState;
            nextState.Enter(this);
            fromAnyState = null;
        }

        nextState = currentState.UpdateState(this);
        fromAnyState = graph.anyNode?.state.UpdateState(this);
        if (fromAnyState)
            MathUtils.Swap(ref nextState, ref fromAnyState);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (damageWhenCollide && collision.CompareTag("Player")) player.Hurt(damage.value);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (damageWhenCollide && collision.collider.CompareTag("Player")) player.Hurt(damage.value);
    }

    public void Die()
    {
        Instantiate(explosionParitcle, transform.position, Quaternion.identity);

        for (int i = 0; i < moneyDrop.randomValue; i++)
            ObjectPooler.instance.SpawnFromPool("Money", transform.position, Random.rotation);

        if (splitEnemy && numberOfSplits > 0)
            Split();

        numberOfEnemiesAlive--;
        Destroy(gameObject);
    }

    public void Hurt(int _damage)
    {
        health.value -= _damage;
        AudioManager.instance.PlaySfx("GetHit");
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
        AudioManager.instance.PlaySfx(explodeSound);
        EZCameraShake.CameraShaker.Instance.ShakeOnce(8, 5, 0.1f, 0.5f);
        GameObject explodeVFX = Instantiate(explodeParticle, transform.position, Quaternion.identity);
        explodeVFX.transform.localScale = new Vector3(6.25f, 6.25f, 0) * explodeRange;
        Destroy(explodeVFX, .3f);
        if ((player.transform.position - transform.position).sqrMagnitude < explodeRange * explodeRange)
            player.Hurt(damage.value);
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

    // TODO: This is stupid. Need to rework on this (Maybe save all the ground tiles in RoomManager and spawn random there?)
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
        Instantiate(splitEnemy, transform.position + offset, Quaternion.identity);
        Instantiate(splitEnemy, transform.position + offset, Quaternion.identity);
    }

    public bool GroundCheck()
    {
        Vector2 pos = (Vector2)transform.position - new Vector2(0, sr.bounds.extents.y * transform.up.y);
        Vector2 size = new Vector2(sr.bounds.size.x, 0.1f);
        ExtDebug.DrawBox(pos, size / 2, Quaternion.identity, Color.green);
        return Physics2D.BoxCast(pos, size, 0, -transform.up, 0, LayerMask.GetMask("Ground"));
    }

    /// <summary>
    ///  return true when detect a cliff
    /// </summary>
    public bool CliffCheck()
    {
        Vector2 pos = (Vector2)transform.position + new Vector2((sr.bounds.extents.x + .1f) * Mathf.Sign(rb.velocity.x), -sr.bounds.extents.y);
        InternalDebug.DrawRay(pos, Vector2.down, Color.cyan);
        return !Physics2D.Raycast(pos, Vector2.down, .1f, LayerMask.GetMask("Ground"));
    }

    public bool WallCheck(float range)
    {
        Vector2 pos = transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, 0, 0);
        InternalDebug.DrawRay(pos, transform.right * range, Color.yellow);
        return Physics2D.Raycast(pos, transform.right, range, LayerMask.GetMask("Ground"));
    }

    public bool PlayerCheck(float range)
    {
        Vector2 pos = transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, 0, 0);
        InternalDebug.DrawRay(pos, transform.right * range, Color.blue);
        return Physics2D.Raycast(pos, transform.right, range, LayerMask.GetMask("Player"));
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
            transform.eulerAngles = new Vector3(0, 180, 0);
        else if (player.transform.position.x > transform.position.x)
            transform.eulerAngles = Vector3.zero;
    }

    public bool IsInRange(float range)
    {
        return (player.transform.position - transform.position).sqrMagnitude < range * range;
    }
}
