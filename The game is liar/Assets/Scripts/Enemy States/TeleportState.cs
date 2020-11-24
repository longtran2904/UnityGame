using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy States/Teleport State")]
public class TeleportState : EnemyState
{
    public float timeBtwTeleports;
    private float timeBtwTeleportsValue;
    private SpriteRenderer playerSr;
    private TrailRenderer trail;

    public override void Init(Enemies enemy)
    {
        playerSr = enemy.player.GetComponent<SpriteRenderer>();
        trail = enemy.GetComponent<TrailRenderer>();
        timeBtwTeleportsValue = 0;
    }

    public override EnemyState UpdateState(Enemies enemy)
    {
        bool inRange = (enemy.player.transform.position - enemy.transform.position).sqrMagnitude > enemy.distanceToChase * enemy.distanceToChase;
        float distanceY = Mathf.Abs(enemy.player.transform.position.y - enemy.transform.position.y);
        float distanceX = Mathf.Sign(enemy.transform.position.x - enemy.player.transform.position.x);
        PopState(enemy);
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
            return nextState;
        }
        return null;
    }

    IEnumerator DisableTrail()
    {
        yield return new WaitForSeconds(.1f);
        trail.enabled = false;
    }
}
