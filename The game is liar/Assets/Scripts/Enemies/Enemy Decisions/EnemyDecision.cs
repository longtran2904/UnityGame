using UnityEngine;

public abstract class EnemyDecision : ScriptableObject
{
    public bool resetWhenEnter;

    public virtual void Reset() { }

    public abstract bool Decide(Enemy enemy);
}
