using UnityEngine;
using EZCameraShake;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class Player : MonoBehaviour
{
    public int health;

    public HealthBar healthBar;

    private PlayerController controller;

    [HideInInspector] public Vector2 knockbackForce;

    Animator anim;

    public event Action tpDelegate;

    bool isInvincible;
    public float invincibleTime;
    [Range(0, 1)]
    public float invincibleOpacity;

    public Material hurtMat;
    private Material defMat;
    private SpriteRenderer sr;

    private void Start()
    {
        anim = GetComponent<Animator>();

        if (healthBar == null)
        {
            healthBar = FindObjectOfType<HealthBar>();
        }

        if (healthBar)
        {
            healthBar.SetMaxHealth(health);
        }

        controller = GetComponent<PlayerController>();

        sr = GetComponent<SpriteRenderer>();

        defMat = sr.material;
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        AudioManager.instance.Play("PlayerDeath");
        GameManager.instance.LoadGame((int)SceneIndexes.START_MENU, true);
        Destroy(gameObject);
    }

    public void Hurt(int _damage)
    {
        if (isInvincible)
        {
            return;
        }
        health -= _damage;
        AudioManager.instance.Play("GetHit");
        CameraShaker.Instance.ShakeOnce(5, 4, .1f, .1f);
        healthBar.SetHealth(health);
        controller.KnockBack(knockbackForce);
        isInvincible = true;
        StartCoroutine(Flashing());
    }

    IEnumerator Flashing()
    {
        sr.material = hurtMat;

        yield return new WaitForSeconds(.1f);

        sr.material = defMat;
        Color temp = sr.color;
        temp.a = invincibleOpacity;
        sr.color = temp;

        yield return new WaitForSeconds(invincibleTime);

        temp.a = 1;
        sr.color = temp;
        isInvincible = false;
    }

    public void PlayTeleportAnimation()
    {
        anim.SetTrigger("Teleport");
    }

    void Teleport()
    {
        if (tpDelegate != null)
            tpDelegate();
    }
}
