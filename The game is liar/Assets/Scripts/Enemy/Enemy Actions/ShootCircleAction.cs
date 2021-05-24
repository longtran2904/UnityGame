using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Enemy/Action/Shoot Circle")]
public class ShootCircleAction : EnemyAction
{
    public string bulletName;
    public string sfx;

    public int numberOfBullets;
    public int radius;
    public float delayBulletTime;
    public bool rotate;
    [ShowWhen("rotate")] public float rotateSpeed;
    [ShowWhen("rotate")] public bool clockwise;

    public GameObject bulletHolderObj;
    private Transform bulletHolder;
    private Projectile[] bullets;

    public override void Act(Enemy enemy)
    {
        enemy.StartCoroutine(Shoot(enemy));
    }

    // NOTE: The enemy can have already been destroyed while running the coroutine. Fix this when possible (Maybe call the coroutine on a seperate object?)
    private IEnumerator Shoot(Enemy enemy)
    {
        bulletHolder = Instantiate(bulletHolderObj, enemy.transform.position, Quaternion.identity).transform;
        bullets = new Projectile[numberOfBullets];
        int i = 0;
        foreach (Vector2 pos in MathUtils.GenerateCircleOutline(enemy.transform.position, numberOfBullets, radius))
        {
            bullets[i] = ObjectPooler.instance.SpawnFromPool<Projectile>(bulletName, pos, Quaternion.identity, bulletHolder);
            bullets[i].Init(enemy.damage, 0, 0, true, false);
            bullets[i].SetVelocity(0);
            i++;
        }

        AudioManager.instance.PlaySfx(sfx);
        yield return new WaitForSeconds(delayBulletTime);

        Vector2 dir = (enemy.player.transform.position - bulletHolder.position).normalized;
        bulletHolder.gameObject.GetComponent<Rigidbody2D>().velocity = dir * bullets[0].speed; // All bullets have the same speed
        if (rotate)
            bulletHolder.transform.Rotate(new Vector3(0, 0, rotateSpeed * Time.deltaTime * (clockwise ? 1 : -1)));
    }
}
