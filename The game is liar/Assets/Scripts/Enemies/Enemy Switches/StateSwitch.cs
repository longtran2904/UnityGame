using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Switches/Normal")]
public class StateSwitch : ScriptableObject
{
    public bool nextIsSwitch;
    [ShowWhen("nextIsSwitch", false)] public EnemyState nextState;
    [ShowWhen("nextIsSwitch")] public StateSwitch nextSwitch;

    public virtual EnemyState NextState(Enemy enemy)
    {
        return nextIsSwitch ? nextSwitch.NextState(enemy) : nextState;
    }
}
