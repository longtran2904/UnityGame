using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/State/Any")]
public class AnyState : ScriptableObject
{
    public bool callEnterWhenDone;
    public EnemyTransition[] transitions;

    public EnemyState CheckTransitions(Enemy enemy)
    {
        EnemyState nextState = null;
        for (int i = transitions.Length - 1; i >= 0; i--) // Top transition get higher priority
        {
            if (transitions[i].Result(enemy) && transitions[i].trueState != this && transitions[i].trueState != null)
                nextState = transitions[i].trueState;
            else if (transitions[i].falseState != this && transitions[i].falseState != null)
                nextState = transitions[i].falseState;
        }
        return nextState;
    }
}
