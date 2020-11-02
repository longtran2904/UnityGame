using UnityEngine;
using System.Collections;


public class Enemies : MonoBehaviour
{
    public static int numberOfEnemiesAlive = 0;

    public int health;
    public int damage;
    public DropRange moneyDropRange;
    public Material matWhite;

    protected bool hasSpawnVFX;
    public GameObject explosionParitcle;
    public GameObject spawnEffect;
    
    void Start()
    {
        StartCoroutine(SpawnEnemy());
    }

    IEnumerator SpawnEnemy()
    {
        numberOfEnemiesAlive += 1;

        if (hasSpawnVFX)
        {
            // Disable behaviours and child objects
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.enabled = false;
            float startGravityScale = GetComponent<Rigidbody2D>().gravityScale;
            GetComponent<Rigidbody2D>().gravityScale = 0;
            foreach (var component in GetComponents<Behaviour>())
            {
                if (component == this)
                {
                    continue;
                }
                component.enabled = false;
            }
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            Destroy(Instantiate(spawnEffect, transform.position, Quaternion.identity), 1);
            yield return new WaitForSeconds(1);

            // Enable components and child objects
            sr.enabled = true;
            GetComponent<Rigidbody2D>().gravityScale = startGravityScale;
            foreach (var component in GetComponents<Behaviour>())
            {
                component.enabled = true;
            }
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            } 
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        GameObject explosion = explosionParitcle;
        Instantiate(explosion, transform.position, Quaternion.identity);
        DropMoney(moneyDropRange.GetRandom());
        numberOfEnemiesAlive--;
        Destroy(gameObject);
    }

    protected void DropMoney(int dropMoney)
    {
        for (int i = 0; i < dropMoney; i++)
        {
            ObjectPooler.instance.SpawnFromPool("Money", transform.position, UnityEngine.Random.rotation);
        }
    }

    public void Hurt(int _damage, Vector2 _knockbackForce = new Vector2(), Material hurtMat = null)
    {
        health -= _damage;
        AudioManager.instance.Play("GetHit");
        EnemiesMovement movement = GetComponent<EnemiesMovement>();
        movement.KnockBack(_knockbackForce);
        StartCoroutine(ResetMaterial());

        IEnumerator ResetMaterial()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.material = hurtMat ? hurtMat : matWhite;
            yield return new WaitForSeconds(.1f);
            sr.material = movement.defaultMaterial;
        }
    }
}
