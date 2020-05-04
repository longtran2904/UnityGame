using UnityEngine;
using EZCameraShake;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{

    public int health;

    private AudioManager audioManager;

    public HealthBar healthBar;

    private PlayerController controller;

    [HideInInspector] public Vector2 knockbackForce;

    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();

        if (healthBar == null)
        {
            healthBar = FindObjectOfType<HealthBar>();
        }

        if (healthBar)
        {
            healthBar.SetMaxHealth(health);
        }

        controller = GetComponent<PlayerController>();
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
        audioManager.Play("PlayerDeath");
        GameManager.instance.LoadGame((int)SceneIndexes.START_MENU, true);
        Destroy(gameObject);
    }

    public void Hurt(int _damage)
    {
        health -= _damage;
        audioManager.Play("GetHit");
        CameraShaker.Instance.ShakeOnce(5, 4, .1f, .1f);
        healthBar.SetHealth(health);
        controller.KnockBack(knockbackForce);
    }
}
