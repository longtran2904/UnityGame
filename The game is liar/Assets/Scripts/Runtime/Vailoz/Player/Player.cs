﻿using UnityEngine;
using EZCameraShake;
using System;
using System.Collections;
using TMPro;

public class Player : MonoBehaviour
{
    public IntReference health;
    public Vector3Variable position;
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
        position.value = transform.position;
        isInvincible = controller.isJumping;
    }

    IEnumerator Die()
    {
        AudioManager.instance.PlaySfx("PlayerDeath");
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

    public void Hurt(int _damage)
    {
        if (!isInvincible)
        {
            health.value -= _damage;
            AudioManager.instance.PlaySfx("GetHit");
            CameraShaker.Instance.ShakeOnce(5, 4, .1f, .1f);
            controller.KnockBack();
            isInvincible = true;
            ActivatePlayerInput(false);
            StartCoroutine(Flashing());
        }
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
        ActivatePlayerInput(true);
    }

    public void ActivatePlayerInput(bool active)
    {
        // Weapon
        shootAndRotateBehaviour.enabled = active;
        weaponSwitching.enabled = active;

        //Movement
        controller.enabled = active;
        combat.enabled = active;
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
