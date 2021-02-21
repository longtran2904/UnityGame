using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Decision/Random")]
public class RandomDecision : EnemyDecision
{
    [Range(0f, 1f)] public float probability = .5f;

    public override bool Decide(Enemy enemy)
    {
        return MathUtils.RandomBool(probability);
    }
}
