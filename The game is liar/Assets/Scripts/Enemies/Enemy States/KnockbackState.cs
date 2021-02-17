using UnityEngine;

/*public class KnockbackState : EnemyState
{
    Vector2 knockbackForce;
    float knockbackTime;

    public KnockbackState(Vector2 knockbackForce, float knockbackTime)
    {
        this.knockbackForce = knockbackForce;
        this.knockbackTime = knockbackTime;
    }

    public override void Init(Enemy enemy)
    {
        enemy.rb.velocity += knockbackForce;
        popStateWhenDone = true;
    }

    public override EnemyState UpdateState(Enemy enemy)
    {
        knockbackTime -= Time.deltaTime;
        if (knockbackTime <= 0)
        {
            enemy.rb.velocity = Vector2.zero;
            return base.UpdateState(enemy);
        }
        return null;
    }
}*/
