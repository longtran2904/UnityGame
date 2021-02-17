using UnityEngine;

/*[CreateAssetMenu(menuName = "Enemy/States/Move")]
public class MoveState : EnemyState
{
    private int dir;

    public override void Init(Enemies enemy)
    {        
        dir = Random.value > .5f ? 1 : -1;
        base.Init(enemy);
    }

    public override EnemyState UpdateState(Enemies enemy)
    {
        if (!enemy.CliffCheck())
            enemy.rb.velocity = Vector2.right * dir * enemy.speed * Time.deltaTime;
        else
            enemy.rb.velocity = Vector2.left * Mathf.Sign(enemy.rb.velocity.x) * enemy.speed * Time.deltaTime;

        return base.UpdateState(enemy);
    }
}*/

public class MoveAction : EnemyAction
{
    public override void Act(Enemy enemy)
    {
        enemy.rb.velocity = Vector2.right * Mathf.Sign(Random.Range(-1, 1)) * enemy.speed;
    }
}