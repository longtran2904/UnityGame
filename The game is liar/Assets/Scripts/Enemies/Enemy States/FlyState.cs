using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*[CreateAssetMenu(menuName = "Enemy/States/Fly")]
public class FlyState : EnemyState
{
    public enum FlyType { Normal, Away, InCurve };

    public FlyType flyType;
    private float timer;

    public override void Init(Enemies enemy)
    {
        if (flyType == FlyType.InCurve) timer = 0;
        base.Init(enemy);
    }

    public override EnemyState UpdateState(Enemies enemy)
    {
        if (flyType == FlyType.InCurve)
        {
            timer = Mathf.Repeat(timer, 1);
            Vector2 point = MathUtils.GetBQCPoint(timer, enemy.transform.position, enemy.curvePoint.position, enemy.player.transform.position);
            enemy.rb.velocity = new Vector2(point.x - enemy.transform.position.x, point.y - enemy.transform.position.y).normalized * enemy.speed * Time.fixedDeltaTime;
            timer += Time.fixedDeltaTime;
        }
        else
        {
            enemy.rb.velocity = (enemy.player.transform.position - enemy.transform.position).normalized * enemy.speed * Time.deltaTime * (flyType == FlyType.Away ? -1 : 1);
        }
        return base.UpdateState(enemy);
    }
}
*/

public class FlyAction : EnemyAction
{
    public enum FlyType { Normal, Away };

    public FlyType flyType;

    public override void Act(Enemy enemy)
    {
        enemy.rb.velocity = (enemy.player.transform.position - enemy.transform.position).normalized * enemy.speed * Time.deltaTime * (flyType == FlyType.Away ? -1 : 1);
    }
}