using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/State/Normal")]
public class EnemyState : ScriptableObject
{
    public bool repeat;
    public bool wait;

    [ShowWhen("repeat")] public IntReference maxRepeatCount;
    private int maxRepeatValue;
    private int repeatCount;
    [ShowWhen("wait")] public FloatReference waitTime;
    private float waitTimeValue;

    public EnemyAction[] actions;
    public EnemyTransition[] transitions;
    public string enterAnimation;
    [HideInInspector] public float elapsedTime;

    public void Enter(Enemy enemy)
    {
        if (enterAnimation != "")
            enemy.anim.Play(enterAnimation);

        foreach (EnemyTransition transition in transitions)
            foreach (EnemyDecision decision in transition.decisions)
                if (decision.resetWhenEnter)
                    decision.Reset();

        elapsedTime = 0;
        if (wait) waitTimeValue = Time.time + waitTime;
        if (repeat)
        {
            repeatCount = 0;
            maxRepeatValue = maxRepeatCount;
        }
    }

    public EnemyState UpdateState(Enemy enemy)
    {
        elapsedTime += Time.deltaTime;

        if ((wait && Time.time >= waitTimeValue) || !wait)
        {
            DoActions(enemy);
            waitTimeValue = Time.time + waitTime;
        }
        else
            return null;

        if (repeat)
        {
            repeatCount++;
            if (repeatCount >= maxRepeatValue)
                return CheckTransitions(enemy);
        }
        else
            return CheckTransitions(enemy);

        return null;
    }

    private void DoActions(Enemy enemy)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            actions[i].Act(enemy);
        }
    }

    private EnemyState CheckTransitions(Enemy enemy)
    {
        for (int i = 0; i < transitions.Length; i++) // Top transition get higher priority
        {
            if (transitions[i].Result(enemy) && transitions[i].trueState != this && transitions[i].trueState != null)
                return transitions[i].trueState;
            else if (transitions[i].falseState != this && transitions[i].falseState != null)
                return transitions[i].falseState;
        }
        return null;
    }
}