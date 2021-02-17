using UnityEngine;

/*[CreateAssetMenu(menuName = "Enemy/Switches/When Detect")]
public class SwitchWhenDetect : StateSwitch
{
    public bool notDetectIsSwitch;
    [ShowWhen("notDetectIsSwitch", false)] public EnemyState notDetectState;
    [ShowWhen("notDetectIsSwitch")] public StateSwitch notDetectSwitch;

    public bool detectWall;
    public bool detectCliff;
    public bool detectGround;
    public bool detectPlayer;

    [ShowWhen("detectGround")]public float wallRange;
    [ShowWhen("detectPlayer")]public float playerRange;

    public override EnemyState NextState(Enemies enemy)
    {
        bool wall = true, cliff = true, ground = true, player = true;
        if (detectWall)
            wall = enemy.WallCheck(wallRange);
        if (detectCliff)
            cliff = enemy.CliffCheck();
        if (detectGround)
            ground = enemy.GroundCheck();
        if (detectPlayer)
            player = enemy.PlayerCheck(playerRange);
        return (wall && cliff && ground && player) ? base.NextState(enemy) : (notDetectIsSwitch ? notDetectSwitch.NextState(enemy) : notDetectState);
    }
}*/

public class DetectDecision : EnemyDecision
{
    public bool detectWall;
    public bool detectCliff;
    public bool detectGround;
    public bool detectPlayer;

    [ShowWhen("detectGround")] public float wallRange;
    [ShowWhen("detectPlayer")] public float playerRange;

    public override bool Decide(Enemy enemy)
    {
        bool wall = true, cliff = true, ground = true, player = true;
        if (detectWall)
            wall = enemy.WallCheck(wallRange);
        if (detectCliff)
            cliff = enemy.CliffCheck();
        if (detectGround)
            ground = enemy.GroundCheck();
        if (detectPlayer)
            player = enemy.PlayerCheck(playerRange);
        return wall && cliff && ground && player;
    }
}
