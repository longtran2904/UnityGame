using UnityEngine;

[CreateAssetMenu(menuName = "Enemy States/Dash State")]
public class DashState : EnemyState
{
    public float dashTime;
    private float dashTimeValue;

    public override void Init(Enemies enemy)
    {
        dashTimeValue = dashTime;
        enemy.rb.velocity = (Player.player.transform.position - enemy.transform.position).normalized * enemy.dashSpeed;
    }

    public override EnemyState UpdateState(Enemies enemy)
    {
        if (dashTimeValue < 0)
        {
            enemy.rb.velocity = Vector2.zero;
            PopState(enemy);
            return nextState;
        }
        dashTimeValue -= Time.deltaTime;
        return null;
    }
}
