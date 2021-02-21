using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Decision/Detect")]
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
