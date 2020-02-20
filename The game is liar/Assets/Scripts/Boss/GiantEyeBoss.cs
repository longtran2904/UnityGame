using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class GiantEyeBoss : MonoBehaviour
{
    public int health;
    public int damage;
    public int enragedHealth;
    public bool isInvulnerable;
    public Transform enragedPos;
    public Transform[] shootPos = new Transform[4];

    public Material whiteMat;
    private Material defMat;
    private SpriteRenderer sr;
    public GameObject explosion;

    private Animator anim;

    private Rigidbody2D rb;

    private Player player;

    private AudioManager audioManager;
    private bool playSound = true;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        defMat = sr.material;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            player.Hurt(damage);
        }
    }

    public void GetHurt(int _damage)
    {
        if (health <= 0)
        {
            if (playSound == true)
            {
                audioManager.Play("DefeatBoss");
                playSound = false;
            }
        }

        if (isInvulnerable == true || health <= 0)
        {
            return;
        }

        health -= _damage;

        Debug.Log("a");

        sr.material = whiteMat;

        Invoke("ResetMaterial", .1f);

        audioManager.Play("GetHit");
    }

    void ResetMaterial()
    {
        sr.material = defMat;
    }

    void Death()
    {
        audioManager.Play("BossExplosion");

        CameraShaker.Instance.ShakeOnce(8, 5, 0.1f, 0.5f);

        Instantiate(explosion, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    public void LookAtPlayer()
    {
        if (player.transform.position.x < transform.position.x)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (player.transform.position.x > transform.position.x)
        {
            transform.eulerAngles = Vector3.zero;
        }
    }
}
