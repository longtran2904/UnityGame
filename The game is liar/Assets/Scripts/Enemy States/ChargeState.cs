using UnityEngine;

[CreateAssetMenu(menuName = "Enemy States/Charge State")]
public class ChargeState : EnemyState
{
    public float chargeTime;

    public override void Init(Enemies enemy)
    {
        enemy.StartCoroutine(enemy.Flashing(chargeTime));
    }

    public override EnemyState UpdateState(Enemies enemy)
    {
        if (chargeTime <= 0)
        {
            enemy.sr.material = enemy.defMat;
            PopState(enemy);
            return nextState;
        }
        chargeTime -= Time.deltaTime;
        enemy.rb.velocity = Vector2.zero;
        return null;
    }
}
