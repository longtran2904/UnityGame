using UnityEngine;

/*[CreateAssetMenu(menuName = "Enemy/States/Charge")]
public class ChargeState : EnemyState
{
    public Vector2 chargeTimeRange;
    private float chargeTime;
    // NOTE: I don't use enemy.canFlash because an enemy can has a charge state that flash and another the one that don't
    public bool canFlash;

    public override void Init(Enemies enemy)
    {
        chargeTime = Random.Range(chargeTimeRange.x, chargeTimeRange.y);
        enemy.rb.velocity = Vector2.zero;
        if (canFlash) enemy.StartCoroutine(enemy.Flashing(chargeTime));
        base.Init(enemy);
    }

    public override EnemyState UpdateState(Enemies enemy)
    {
        if (chargeTime <= 0)
        {
            enemy.sr.material = enemy.defMat;
            return base.UpdateState(enemy);
        }
        chargeTime -= Time.deltaTime;
        enemy.rb.velocity = Vector2.zero;
        return null;
    }
}*/
