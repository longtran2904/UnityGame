using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Decision/Wait")]
public class WaitDecision : EnemyDecision
{
    public FloatReference waitTime;
    private float time;

    public override void Reset()
    {
        if (waitTime.fieldType == FieldType.Random) time = waitTime.value;
    }

    public override bool Decide(Enemy enemy)
    {
        if (enemy.currentState.elapsedTime >= (resetWhenEnter && waitTime.fieldType == FieldType.Random ? time : waitTime.value))
            return true;
        return false;
    }
}