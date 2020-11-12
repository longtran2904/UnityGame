using UnityEngine;

public abstract class EnemyState : ScriptableObject
{
    public EnemyState nextState;

    public abstract void Init(Enemies enemy);

    public abstract EnemyState UpdateState(Enemies enemy);

    protected void PopState(Enemies enemy)
    {
        if (enemy.allStates.Peek() == this)
        {
            enemy.allStates.Pop();
        }
    }
}

public class KnockbackState : EnemyState
{
    Vector2 knockbackForce;
    float knockbackTime;

    public KnockbackState(Vector2 knockbackForce, float knockbackTime)
    {
        this.knockbackForce = knockbackForce;
        this.knockbackTime = knockbackTime;
    }

    public override void Init(Enemies enemy)
    {
        enemy.rb.velocity += knockbackForce;
    }

    public override EnemyState UpdateState(Enemies enemy)
    {
        knockbackTime -= Time.deltaTime;
        if (knockbackTime <= 0)
        {
            enemy.rb.velocity = Vector2.zero;
            PopState(enemy);
        }
        return nextState;
    }
}