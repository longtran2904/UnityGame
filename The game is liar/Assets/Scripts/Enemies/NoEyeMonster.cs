using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoEyeMonster : EnemiesMovement
{
    public float dashSpeed;
    bool isAttack;
    bool canCharge = true;
    public float dashTime;
    float dashTimeValue;
    public float chargeTime;
    float chargeTimeValue;
    bool isFlashing;
    public float timeBtwFlash;
    private float timeBtwFlashValue;
    public float flashTime;
    private float flashTimeValue;
    public Material triggerMaterial;
    const float speedMultiplier = 0.025f;
    bool isPlayerDied;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        timeBtwFlashValue = timeBtwFlash;
        flashTimeValue = flashTime;
        dashTimeValue = dashTime;
        chargeTimeValue = chargeTime;
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
        MoveTowardPlayer();
    }

    void MoveTowardPlayer()
    {
        bool isInRange = (player.transform.position - transform.position).sqrMagnitude <= attackRange * attackRange;
        if (isInRange && canCharge)
        {
            ChargeAttack();
        }
        else if ((isFlashing && canCharge))
        {
            ChargeAttack();
        }
        else if (isAttack)
        {
            if (dashTimeValue <= 0)
            {
                MoveAwayPlayer();
                if (!isInRange)
                {
                    isAttack = false;
                    canCharge = true;
                }
            }
            dashTimeValue -= Time.deltaTime;
        }
        else
        {
            LookAtPlayer();
            rb.velocity = (player.transform.position - transform.position).normalized * speed * Time.deltaTime;
        }
    }

    void MoveAwayPlayer()
    {
        rb.velocity = -(player.transform.position - transform.position).normalized * speed * Time.deltaTime;
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

    void Attack()
    {
        Vector2 target = player.transform.position - transform.position;
        rb.velocity = target.normalized * dashSpeed * speedMultiplier;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        enemy.DamagePlayerWhenCollide(collision);
    }
}
