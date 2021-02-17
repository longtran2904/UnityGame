using UnityEngine;

/*[CreateAssetMenu(menuName = "Enemy/Switches/Wait")]
public class WaitSwitch : StateSwitch
{
    public Vector2 waitTimeRange;
    private float waitTime;
    [System.NonSerialized] private bool hasInit;

    public override EnemyState NextState(Enemies enemy)
    {
        if (!hasInit)
        {
            waitTime = Time.time + Random.Range(waitTimeRange.x, waitTimeRange.y);
            hasInit = true;
        }

        if (Time.time >= waitTime)
        {
            hasInit = false;
            return base.NextState(enemy);
        }
        else
            return null;
    }
}*/

public class WaitDecision : EnemyDecision
{
    public Vector2 waitTimeRange;
    private float waitTime;

    private void OnEnable()
    {
        waitTime = Random.Range(waitTimeRange.x, waitTimeRange.y);
    }

    public override bool Decide(Enemy enemy)
    {
        if (enemy.allStates.Peek().elapsedTime >= waitTime)
            return true;
        return false;
    }
}
