using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Decision/Random")]
public class RandomDecision : EnemyDecision
{
    public float probability;

    public override bool Decide(Enemy enemy)
    {
        return MathUtils.RandomBool(probability);
    }
}
