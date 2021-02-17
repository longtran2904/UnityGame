using UnityEngine;

/*[CreateAssetMenu(menuName = "Enemy/States/Normal")]
public class EnemyState : ScriptableObject
{
    public bool nextIsState;
    [ShowWhen("nextIsState", false)] public StateSwitch Switch;
    [ShowWhen("nextIsState")] public EnemyState state;

    public bool popStateWhenDone;
    public string optionalAnimationToPlay;

    public virtual void Init(Enemies enemy)
    {
        if (optionalAnimationToPlay != "")
            enemy.anim.Play(optionalAnimationToPlay);
    }

    public virtual EnemyState UpdateState(Enemies enemy)
    {
        EnemyState nextState = nextIsState ? state : Switch.NextState(enemy);
        if (nextState && popStateWhenDone) PopState(enemy);
        return nextState;
    }

    private void PopState(Enemies enemy)
    {
        if (enemy.allStates.Peek() == this)
            enemy.allStates.Pop();
    }
}*/

[CreateAssetMenu(menuName = "Enemy/State")]
public class EnemyState : ScriptableObject
{
    public EnemyAction[] actions;
    public EnemyTransition[] transitions;
    public string enterAnimation;
    public float elapsedTime;

    public void Enter(Enemy enemy)
    {
        if (enterAnimation != "")
            enemy.anim.Play(enterAnimation);
        elapsedTime = 0;
    }

    public EnemyState UpdateState(Enemy enemy)
    {
        DoActions(enemy);
        elapsedTime += Time.deltaTime;
        return CheckTransitions(enemy);
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
        EnemyState nextState = null;
        for (int i = transitions.Length - 1; i >= 0; i--) // Top transition get higher priority
        {
            if (transitions[i].decision.Decide(enemy))
                nextState = (transitions[i].trueState != this) ? transitions[i].trueState : null;
            else
                nextState = (transitions[i].falseState != this) ? transitions[i].falseState : null;
        }
        return nextState;
    }
}