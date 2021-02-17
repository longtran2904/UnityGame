using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*[CreateAssetMenu(menuName = "Enemy/States/Jump")]
public class JumpState : EnemyState
{
    public float jumpForce;
    public float timeBtwJumps;
    private float timeBtwJumpsValue;

    public override EnemyState UpdateState(Enemies enemy)
    {
        if (Time.time > timeBtwJumpsValue && enemy.GroundCheck())
        {
            enemy.rb.velocity = new Vector2(Mathf.Sign(enemy.player.transform.position.x - enemy.transform.position.x), 1).normalized * jumpForce;
            timeBtwJumpsValue = Time.time + timeBtwJumps;
        }
        return base.UpdateState(enemy);
    }
}*/


public class JumpAction : EnemyAction
{
    public float jumpForce;
    [Range(0f, 90f)] public float jumpAngle;

    public override void Act(Enemy enemy)
    {
        enemy.rb.velocity = new Vector2(Mathf.Cos(jumpAngle) * Mathf.Sign(enemy.player.transform.position.x - enemy.transform.position.y), Mathf.Sin(jumpAngle)) * jumpForce;
    }
}