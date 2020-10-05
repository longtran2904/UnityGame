using EZCameraShake;
using UnityEngine;

public class EnemiesMovement : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Enemies enemy;
    public float speed;
    protected Player player;
    [HideInInspector] public EnemyType enemyType;
    private Animator anim;
    public float attackRange;
    protected SpriteRenderer sr;
    protected Material defaultMaterial;

    #region Knockback
    public float knockbackTime;
    private float knockbackCounter;
    private Vector2 knockbackForce;
    private bool knockback;
    #endregion

    // Start is called before the first frame update
    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = (GetComponent<SpriteRenderer>() != null) ? GetComponent<SpriteRenderer>() : GetComponentInChildren<SpriteRenderer>();
        defaultMaterial = sr.material;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        enemy = GetComponent<Enemies>();
        player.deathEvent += OnPlayerDeathEvent;
    }
    
    protected virtual void FixedUpdate()
    {
        SetKnockback(); 
    }

    void SetKnockback()
    {
        if (knockbackCounter < 0)
        {
            knockback = true;
        }
        if (knockback == true)
        {
            rb.velocity = Vector2.zero;
            knockbackCounter = 0;
            knockback = false;
        }
        if (knockbackCounter > 0)
        {
            rb.velocity = new Vector2(knockbackForce.x, knockbackForce.y) * Time.deltaTime;
            knockbackCounter -= Time.deltaTime;
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

    protected virtual void OnPlayerDeathEvent()
    {
        player.deathEvent -= OnPlayerDeathEvent;
    }
}
