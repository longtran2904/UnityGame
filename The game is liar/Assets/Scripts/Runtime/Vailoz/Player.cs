using UnityEngine;
using System;
using System.Collections;
using TMPro;

public class Player : MonoBehaviour
{
    public IntReference health;

    [HideInInspector] public PlayerController controller;
    private Animator anim;

    bool isInvincible;
    public float invincibleTime;
    [Range(0, 1)] public float invincibleOpacity;

    public Material hurtMat;
    private Material defMat;
    private SpriteRenderer sr;

    public GameObject hitEffect;
    public ParticleSystem deathBurstParticle;
    public ParticleSystem deathFlowParticle;

    private void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        sr = GetComponent<SpriteRenderer>();
        defMat = sr.material;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Handle the case when dying and hurting the same time
        if (health.value <= 0)
        {
            StartCoroutine(Die());
            Destroy(this);
        }
    }

    IEnumerator Die()
    {
        GameInput.EnableAllInputs(false);
        AudioManager.PlayAudio(AudioType.Player_Death);
        anim.Play("Death");
        transform.GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSeconds(.5f);

        Time.timeScale = 0;
        deathBurstParticle.Play();
        yield return new WaitForSecondsRealtime(2);

        Time.timeScale = 1;
        deathFlowParticle.Play();
        yield return new WaitForSeconds(deathFlowParticle.main.duration);
        // TODO: Replay and enable all inputs
    }

    public void Hurt(int damage)
    {
        isInvincible = !controller.groundCheck;
        if (!isInvincible)
        {
            health.value -= damage;
            AudioManager.PlayAudio(AudioType.Player_Hurt);
            anim.Play("Idle");
            CameraSystem.instance.Shake(ShakeMode.Medium);
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
        StartCoroutine(CameraSystem.instance.Flash(.15f, .8f));
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
}
