using UnityEngine;

/*[CreateAssetMenu(menuName = "Enemy/States/Dash")]
public class DashState : EnemyState
{
    public float dashTime;
    private float dashTimeValue;

    public override void Init(Enemies enemy)
    {
        dashTimeValue = dashTime + Time.time;
        enemy.rb.velocity = (enemy.player.transform.position - enemy.transform.position).normalized * enemy.dashSpeed;
        base.Init(enemy);
    }

    public override EnemyState UpdateState(Enemies enemy)
    {
        if (Time.time > dashTimeValue)
        {
            enemy.rb.velocity = Vector2.zero;
            return base.UpdateState(enemy);
        }
        return null;
    }
}*/
