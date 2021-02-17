using System.Collections;
using UnityEngine;

/*[CreateAssetMenu(menuName = "Enemy/States/Teleport")]
public class TeleportState : EnemyState
{
    public float distanceToChase;
    public float timeBtwTeleports;
    private float timeBtwTeleportsValue;
    private SpriteRenderer playerSr;
    private TrailRenderer trail;

    public override void Init(Enemies enemy)
    {
        playerSr = enemy.player.GetComponent<SpriteRenderer>();
        trail = enemy.GetComponent<TrailRenderer>();
        timeBtwTeleportsValue = 0;
        base.Init(enemy);
    }

    public override EnemyState UpdateState(Enemies enemy)
    {
        bool inRange = (enemy.player.transform.position - enemy.transform.position).sqrMagnitude > distanceToChase * distanceToChase;
        float distanceY = Mathf.Abs(enemy.player.transform.position.y - enemy.transform.position.y);
        float distanceX = Mathf.Sign(enemy.transform.position.x - enemy.player.transform.position.x);
        if (((distanceY > 2) || inRange) && enemy.player.controller.onGround.value && Time.time > timeBtwTeleportsValue && enemy.GroundCheck())
        {
            if (trail)
            {
                trail.enabled = true;
                enemy.StartCoroutine(DisableTrail());
            }
            Vector3 offset = new Vector2(Mathf.Sign(distanceX) * 1.5f, (enemy.sr.bounds.extents.y - playerSr.bounds.extents.y) * enemy.transform.up.y);
            enemy.transform.position = enemy.player.transform.position + offset;
            timeBtwTeleportsValue = Time.time + timeBtwTeleports;
        }

        return base.UpdateState(enemy);
    }

    IEnumerator DisableTrail()
    {
        yield return new WaitForSeconds(.1f);
        trail.enabled = false;
    }
}*/

public class TeleportAction : EnemyAction
{
    private SpriteRenderer playerSr;
    private TrailRenderer trail;

    public override void Act(Enemy enemy)
    {
        if (!playerSr) playerSr = enemy.player.GetComponent<SpriteRenderer>();
        if (!trail) trail = enemy.GetComponent<TrailRenderer>();

        if (trail)
        {
            trail.enabled = true;
            enemy.StartCoroutine(DisableTrail());
        }

        Vector3 offset = new Vector2(Mathf.Sign(enemy.transform.position.x - enemy.player.transform.position.x) * 1.5f, (enemy.sr.bounds.extents.y - playerSr.bounds.extents.y) /* * enemy.transform.up.y*/);
        enemy.transform.position = enemy.player.transform.position + offset;
    }

    IEnumerator DisableTrail()
    {
        yield return new WaitForSeconds(.1f);
        trail.enabled = false;
    }
}
