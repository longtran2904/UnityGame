using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Switches/Position Difference")]
public class SwitchByPos : StateSwitch
{
    public enum DetectType { X, Y, Distance }
    public DetectType type;

    [ShowWhen("type", DetectType.X)]        public float xDiff;
    [ShowWhen("type", DetectType.Y)]        public float yDiff;
    [ShowWhen("type", DetectType.Distance)] public float distanceDiff;

    public override EnemyState NextState(Enemy enemy)
    {
        switch (type)
        {
            case DetectType.X:
                return enemy.IsInRange(xDiff)        ? base.NextState(enemy) : null;
            case DetectType.Y:
                return enemy.IsInRange(yDiff)        ? base.NextState(enemy) : null;
            case DetectType.Distance:
                return enemy.IsInRange(distanceDiff) ? base.NextState(enemy) : null;
        }
        return null;
    }
}
