[System.Serializable]
public class EnemyTransition
{
    public EnemyDecision[] decisions;
    public EnemyState trueState;
    public EnemyState falseState;

    public bool Result(Enemy enemy)
    {
        bool result = true;
        foreach (var decision in decisions)
        {
            result = result && decision.Decide(enemy);
            if (!result)
                return result;
        }
        return result;
    }
}