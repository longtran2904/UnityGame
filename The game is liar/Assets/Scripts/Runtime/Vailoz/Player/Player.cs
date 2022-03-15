using UnityEngine;
using System;
using System.Collections;
using TMPro;

public class Player : MonoBehaviour
{
    public IntReference health;
    public IntReference money;
    private TextMeshProUGUI moneyText;
    private CameraFollow2D cam;

    [HideInInspector] public PlayerController controller;
    private Animator anim;
    public event Action teleportEvent;

    bool isInvincible;
    public float invincibleTime;
    [Range(0, 1)] public float invincibleOpacity;

    public Material hurtMat;
    private Material defMat;
    private SpriteRenderer sr;

    public event Action deathEvent;
    public GameObject hitEffect;
    public GameObject deathEffect;
    public GameObject deathParticle;

    public WeaponInventory inventory;

    private void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        sr = GetComponent<SpriteRenderer>();
        defMat = sr.material;
        moneyText = GameObject.Find("Money")?.GetComponent<TextMeshProUGUI>();
        cam = FindObjectOfType<CameraFollow2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (health.value <= 0)
        {
            StartCoroutine(Die());
            Destroy(this);
        }
        moneyText?.SetText(money.value.ToString());
    }

    IEnumerator Die()
    {
        controller.audioManager.PlayAudio(AudioType.Player_Death);
        anim.Play("Death");
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

    }

    public void Hurt(int _damage)
    {
        isInvincible = !controller.groundCheck;
        if (!isInvincible)
        {
            health.value -= _damage;
            controller.audioManager.PlayAudio(AudioType.Player_Hurt);
            anim.Play("Idle");
            CameraShake.instance.Shake(ShakeMode.Medium);
            //ParticleEffect.instance.PlayParticle(ParticleType.Explosion, transform.position, explodeRange);
            StartCoroutine(Hurting());
        }
    }

    IEnumerator Hurting()
    {
        GameInput.EnableAllInputs(false);
        isInvincible = true;
        anim.speed = 0;
        sr.material = hurtMat;
        hitEffect.SetActive(true);
        transform.localScale = new Vector2(.75f, 1f);

        Time.timeScale = 0f;
        StartCoroutine(cam.Flash(.15f, .8f));
        yield return new WaitForSecondsRealtime(.15f);
        Time.timeScale = 1f;

        yield return new WaitForSeconds(.1f);
        sr.material = defMat;
        hitEffect.SetActive(false);
        transform.localScale = new Vector2(1f, 1f);

        Color temp = sr.color;
        temp.a = invincibleOpacity;
        sr.color = temp;

        yield return new WaitForSeconds(invincibleTime);

        temp.a = 1;
        sr.color = temp;
        anim.speed = 1;
        isInvincible = false;
        GameInput.EnableAllInputs(true);
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
        teleportEvent?.Invoke();
    }
}
