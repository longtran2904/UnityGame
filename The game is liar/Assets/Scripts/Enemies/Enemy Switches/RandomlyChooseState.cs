using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Switches/Random")]
public class RandomlyChooseState : StateSwitch
{
    public EnemyStateProb[] states;

    [System.Serializable]
    public class EnemyStateProb
    {
        public EnemyState state;
        public float prob;
    }

    public override EnemyState NextState(Enemy enemy)
    {
        float[] probs = new float[states.Length];
        for (int i = 0; i < states.Length; i++)
        {
            probs[i] = states[i].prob;
        }
        return states[MathUtils.Choose(probs)].state;
    }
}
