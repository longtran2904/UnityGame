#undef Debug
using UnityEngine;
using EZCameraShake;

public class BatMovement : EnemiesMovement
{
    private Transform curve_point;
    private Vector2 point;

    public Material triggerMaterial;
    public GameObject explodeVFX;

    public float distanceToExplode;
    public float explodeRange;
    public float timeToExplode;
    public float distanceToChase;
    private float timeToExplodeValue;
    private bool canChase = false;
    private bool canExplode = false;
    private float timer;

    public float timeBtwFlash;
    private float timeBtwFlashValue;
    public float flashTime;
    private float flashTimeValue;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        timeToExplodeValue = timeToExplode;
        timeBtwFlashValue = timeBtwFlash;
        flashTimeValue = flashTime;
        curve_point = player.transform.Find("Curve_point");
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < 1 && canChase && timeToExplodeValue == timeToExplode)
        {
            Chase();
        }
        else
        {
            timer = 0;
        }
        BatStateMachine();
    }

    protected override void OnPlayerDeathEvent()
    {
        base.OnPlayerDeathEvent();
        distanceToChase = 0;
        distanceToExplode = 0;
    }

    void BatStateMachine()
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
    }

    void Chase()
    {
        Vector2 old_point = point;
        MoveInACurve();
        DrawDebug(old_point);
    }

    void MoveInACurve()
    {
        point = MathUtils.GetBQCPoint(timer, transform.position, curve_point.position, player.transform.position);
        rb.velocity = new Vector2(point.x - transform.position.x, point.y - transform.position.y).normalized * speed * Time.fixedDeltaTime;
        timer += Time.fixedDeltaTime;
    }

    [System.Diagnostics.Conditional("Debug")]
    void DrawDebug(Vector2 _point)
    {
        Debug.DrawRay(transform.position, rb.velocity, Color.blue, .01f);
        Debug.DrawLine(_point, point, Color.red, 3600);
    }

    void BatExplode(float _distanceToPlayer)
    {
        rb.velocity = Vector2.zero;
        if (timeToExplodeValue <= 0)
        {
            AudioManager.instance.Play("BatExplosion");
            CameraShaker.Instance.ShakeOnce(8, 5, 0.1f, 0.5f);
            explodeVFX = Instantiate(explodeVFX, transform.position, Quaternion.identity);
            explodeVFX.transform.localScale = new Vector3(6, 6, 1) * explodeRange;
            Destroy(explodeVFX, .3f);
            if (_distanceToPlayer <= explodeRange)
            {
                player.Hurt(enemy.damage);
            }
            enemy.Death();
        }
        else
        {
            timeToExplodeValue -= Time.deltaTime;
            Flashing();
        }
    }

    void Flashing()
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
        if (Time.time >= timeBtwFlashValue)
        {
            sr.material = triggerMaterial;
            timeBtwFlashValue = Time.time + timeBtwFlash;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanceToExplode);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distanceToChase);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeRange);
    }
}
