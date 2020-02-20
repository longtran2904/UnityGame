using UnityEngine;
using EZCameraShake;

public class Player : MonoBehaviour
{

    public int health;

    private AudioManager audioManager;

    public HealthBar healthBar;

    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        healthBar.SetMaxHealth(health);
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
        Destroy(gameObject);
    }

    public void Hurt(int _damage)
    {
        health -= _damage;
        audioManager.Play("GetHit");
        CameraShaker.Instance.ShakeOnce(5, 4, .1f, .1f);
        healthBar.SetHealth(health);
    }
}
