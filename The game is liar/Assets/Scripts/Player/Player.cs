using UnityEngine;
using EZCameraShake;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using TMPro;

public class Player : MonoBehaviour
{
    public int health;
    public HealthBar healthBar;
    public PlayerController controller;
    public TextMeshProUGUI moneyText;
    public int money;
    Animator anim;
    public event Action teleportEvent;
    bool isInvincible;
    public float invincibleTime;
    [Range(0, 1)]
    public float invincibleOpacity;
    public Material hurtMat;
    private Material defMat;
    private SpriteRenderer sr;
    public event Action deathEvent;
    public GameObject deathEffect;
    public GameObject deathParticle;
    public Weapon currentWeapon; // Only change by switching or buying new weapon (WeaponSwitching and WeaponManager)
    public static Player player;

    void Awake()
    {
        player = this;
    }

    private void Start()
    {
        SetupComponent();
        FindAndSetUI();
    }

    void FindAndSetUI()
    {
        if (!healthBar)
        {
            healthBar = FindObjectOfType<HealthBar>();
        }
        healthBar.SetMaxHealth(health);

        if (!moneyText)
        {
            moneyText = GameObject.Find("Money").GetComponent<TextMeshProUGUI>();
        }
    }

    void SetupComponent()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        sr = GetComponent<SpriteRenderer>();
        defMat = sr.material;
    }


    bool hasNotBeenDeath = true;
    // Update is called once per frame
    void Update()
    {
        if (health <= 0 && hasNotBeenDeath)
        {
            StartCoroutine(Die());
            hasNotBeenDeath = false;
        }
        moneyText.text = money.ToString();
    }

    IEnumerator Die()
    {
        AudioManager.instance.Play("PlayerDeath");
        anim.SetTrigger("Death");
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(.5f);

        Time.timeScale = 0;
        Instantiate(deathParticle, transform.position, Quaternion.identity);
        yield return new WaitForSecondsRealtime(2);

        Time.timeScale = 1;
        Instantiate(deathEffect, transform.position, deathEffect.transform.rotation);
        deathEvent?.Invoke();

        yield return new WaitForSeconds(1);
        GameManager.instance.LoadGame((int)SceneIndexes.START_MENU, true);
    }

    public void Hurt(int _damage, Vector2 _knockbackForce = new Vector2())
    {
        if (!isInvincible)
        {
            health -= _damage;
            AudioManager.instance.Play("GetHit");
            CameraShaker.Instance.ShakeOnce(5, 4, .1f, .1f);
            healthBar.SetHealth(health);
            controller.KnockBack(_knockbackForce);
            isInvincible = true;
            StartCoroutine(Flashing());
        }
    }

    IEnumerator Flashing()
    {
        sr.material = hurtMat;
        yield return new WaitForSeconds(.1f);
        sr.material = defMat;
        ChangeSpriteRendererAlpha();
        yield return new WaitForSeconds(invincibleTime);
        ResetSpriteRendererAlpha();
        isInvincible = false;
    }

    void ChangeSpriteRendererAlpha()
    {
        Color temp = sr.color;
        temp.a = invincibleOpacity;
        sr.color = temp;
    }

    void ResetSpriteRendererAlpha()
    {
        Color temp = sr.color;
        temp.a = 1;
        sr.color = temp;
    }

    // Call this to teleport (Play tp animation -> Animation event get called -> tpDelegate)
    public void PlayTeleportAnimation()
    {
        anim.SetTrigger("Teleport");
        transform.GetChild(0).gameObject.SetActive(false);
    }

    // Call in animation event
    void Teleport()
    {
        if (teleportEvent != null)
            teleportEvent();
    }
}
