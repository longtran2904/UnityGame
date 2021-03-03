using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Enemy/State/Any")]
public class AnyState : ScriptableObject
{
    public string Name; // For displaying in the graph editor
    public bool callEnterWhenDone;
    public EnemyAction[] actions;
    public List<EnemyTransition> transitions = new List<EnemyTransition>();

    private void OnEnable()
    {
        Name = name;
    }

    public virtual EnemyState UpdateState(Enemy enemy)
    {
        DoActions(enemy);
        return CheckTransitions(enemy);
    }

    protected void DoActions(Enemy enemy)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            actions[i].Act(enemy);
        }
    }

    protected virtual EnemyState CheckTransitions(Enemy enemy)
    {
        for (int i = 0; i < transitions.Count; i++) // Top transition get higher priority
        {
            if (IsTransitionValid(enemy, transitions[i]))
                return transitions[i].nextState;
        }
        return null;
    }

    protected bool IsTransitionValid(Enemy enemy, EnemyTransition transition)
    {
        if (transition.Result(enemy) && transition.nextState != this && transition.nextState != null)
            return true;
        return false;
    }
}
