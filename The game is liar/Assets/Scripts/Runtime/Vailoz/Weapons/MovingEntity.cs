using UnityEngine;

public class MovingEntity : MonoBehaviour, IPooledObject
{
    public float moveTime;
    public float speed;
    private float timer;
    private Vector2 dir;

    [Header("Damage Popup")]
    public float increaseScaleAmount;
    public float decreaseScaleAmount;
    public float dAlpha;
    private TMPro.TMP_Text text;

    [Header("Bullet Holder")]
    public float dRotate;

    [Header("Bullet")]
    public bool bounce;
    private int damage;
    private PoolType damagePopup;
    private bool hitPlayer;

    public void OnObjectInit()
    {
        text = GetComponent<TMPro.TMP_Text>();
    }

    public void OnObjectSpawn()
    {
        timer = moveTime;
        this.DeactiveGameObject(moveTime);
    }

    public void InitBullet(int damage, bool critical, bool hitPlayer)
    {
        this.damage = damage;
        damagePopup = critical ? PoolType.DamagePopup_Critical : PoolType.DamagePopup;
        this.hitPlayer = hitPlayer;
        dir = transform.right * speed;
    }

    public void InitDamagePopup(int damage)
    {
        text.text = damage.ToString();
        dir = Vector2.one;
    }

    public void InitMoving(Vector2 moving)
    {
        dir = moving * speed;
    }

    void Update()
    {
        transform.position += (Vector3)dir * Time.deltaTime;
        transform.Rotate(0, 0, dRotate * Time.deltaTime);

        if (timer > moveTime / 2)
            transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
        else
            transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;

        if (timer < 0 && text)
        {
            text.alpha -= dAlpha * Time.deltaTime;
            if (text.alpha < 0)
                gameObject.SetActive(false);
        }

        timer -= Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (bounce && collision.collider.CompareTag("BounceMat"))
        {
            transform.right = dir = Vector2.Reflect(dir, collision.GetContact(0).normal);
            ObjectPooler.Spawn(PoolType.VFX_Destroyed_Bullet_Bounce, transform.position);
            AudioManager.PlayAudio(AudioType.Weapon_Bounce);
        }
        else
            Hit(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Hit(collision);
    }

    void Hit(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
            SpawnVFX();
        else if (hitPlayer && collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().Hurt(damage);
            SpawnVFX(true);
        }
        else if (!hitPlayer && collision.CompareTag("Enemy"))
        {
            collision.GetComponent<Enemy>().Hurt(damage);
            SpawnVFX(true);
        }

        void SpawnVFX(bool spawnDamagePopup = false)
        {
            if (spawnDamagePopup)
                ObjectPooler.Spawn(damagePopup, collision.transform.position).GetComponent<MovingEntity>().InitDamagePopup(damage);
            ObjectPooler.Spawn(PoolType.VFX_Destroyed_Bullet, transform.position);
            AudioManager.PlayAudio(AudioType.Weapon_Hit_Wall);
            gameObject.SetActive(false);
        }
    }
}
