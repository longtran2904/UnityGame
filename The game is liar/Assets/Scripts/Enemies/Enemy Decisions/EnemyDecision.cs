using UnityEngine;

public abstract class EnemyDecision : ScriptableObject
{
    public abstract bool Decide(Enemy enemy);
}
