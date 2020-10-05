using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;
using System;

public class GiantEyeBoss : Boss
{
    public int damage;
    public int enragedHealth;
    public bool isInvulnerable;
    public Transform enragedPos;
    public Transform[] shootPos = new Transform[4];

    public Material whiteMat;
    private Material defMat;
    public GameObject explosion;

    private Animator anim;
    private bool playSound = true;
    public GameObject endScreen;

    // Start is called before the first frame update
    protected override void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        defMat = sr.material;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    protected override void Update()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && anim.GetBool("isDied"))
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            player.Hurt(damage);
        }
    }

    public override void GetHurt(int _damage)
    {
        if (health <= 0)
        {
            if (playSound == true)
            {
                AudioManager.instance.Play("DefeatBoss");
                playSound = false;
            }
        }

        if (isInvulnerable == true || health <= 0)
        {
            return;
        }

        health -= _damage;

        sr.material = whiteMat;

        Invoke("ResetMaterial", .1f);

        AudioManager.instance.Play("GetHit");
    }

    void ResetMaterial()
    {
        sr.material = defMat;
    }

    protected override void Die()
    {
        AudioManager.instance.Play("BossExplosion");

        CameraShaker.Instance.ShakeOnce(8, 5, 0.1f, 0.5f);

        Instantiate(explosion, transform.position, Quaternion.identity);

        AudioManager.instance.Stop("BossFight");

        Invoke("EndDemo", 1);

        gameObject.SetActive(false);

        Destroy(gameObject, 1);
    }

    void EndDemo()
    {
        endScreen.SetActive(true);

        Time.timeScale = 0;
    }

    public void LookAtPlayer()
    {
        if (player.transform.position.x > transform.position.x)
        {
            transform.eulerAngles = Vector3.zero;
        }
        else if (player.transform.position.x < transform.position.x)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }
}
