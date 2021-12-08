using UnityEngine;
using EZCameraShake;
using System;
using System.Collections;
using TMPro;

public class Player : MonoBehaviour
{
    public IntReference health;
    public IntReference money;
    private TextMeshProUGUI moneyText;

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
    public GameObject deathEffect;
    public GameObject deathParticle;

    public WeaponInventory inventory;
    private ShootAndRotateGun shootAndRotateBehaviour;
    private WeaponSwitching weaponSwitching;
    private PlayerCombat combat;

    private void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        sr = GetComponent<SpriteRenderer>();
        defMat = sr.material;
        moneyText = GameObject.Find("Money")?.GetComponent<TextMeshProUGUI>();

        // For ActivePlayerInput
        shootAndRotateBehaviour = GetComponentInChildren<ShootAndRotateGun>();
        weaponSwitching = GetComponentInChildren<WeaponSwitching>();
        combat = GetComponent<PlayerCombat>();
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
        controller.audioManager.PlaySfx("PlayerDeath");
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
        GameManager.instance.LoadGame((int)SceneIndexes.START_MENU, true);
    }

    public void Hurt(int _damage)
    {
        // NOTE: The CameraFollow2D uses playerPos which gets updated by the PlayerController.
        //       So by disabling the PlayerController, the camera'll stop getting the latest player's pos.
        //       Currently, it doesn't cause any problems because the player's pos will never change when get hit.
        isInvincible = !controller.isGrounded;
        if (!isInvincible)
        {
            health.value -= _damage;
            controller.audioManager.PlaySfx("GetHit");
            CameraShaker.Instance.ShakeOnce(5, 4, .1f, .1f);
            controller.KnockBack();
            anim.Play("Idle");
            StartCoroutine(Hurting());
        }
    }

    IEnumerator Hurting()
    {
        EnableInput(false);
        isInvincible = true;
        anim.speed = 0;
        sr.material = hurtMat;
        yield return new WaitForSeconds(.1f);
        sr.material = defMat;

        Color temp = sr.color;
        temp.a = invincibleOpacity;
        sr.color = temp;

        yield return new WaitForSeconds(invincibleTime);

        temp.a = 1;
        sr.color = temp;
        anim.speed = 1;
        isInvincible = false;
        EnableInput(true);
    }

    public void EnableInput(bool enable, bool onlyWeapon = false)
    {
        shootAndRotateBehaviour.enabled = enable;
        weaponSwitching.enabled = enable;

        if (onlyWeapon) return;

        controller.enabled = enable;
        combat.enabled = enable;
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
