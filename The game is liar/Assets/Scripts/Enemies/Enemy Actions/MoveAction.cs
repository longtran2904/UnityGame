using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Action/Move")]
public class MoveAction : EnemyAction
{
    public enum GoalDirectionType { Player, Random, Custom }
    public GoalDirectionType directionType;
    [ShowWhen("directionType", GoalDirectionType.Random)] public float probabilityToMoveLeft = .5f;
    [ShowWhen("directionType", GoalDirectionType.Custom)] public bool moveLeft;
    public FloatReference speed;
    public bool multiplyDeltaTime;

    public override void Act(Enemy enemy)
    {
        int dir = MathUtils.RandomBool(probabilityToMoveLeft) ? 1 : -1;
        if (directionType == GoalDirectionType.Player)
            dir = (int)Mathf.Sign(enemy.player.transform.position.x - enemy.transform.position.x);
        else if (directionType == GoalDirectionType.Custom)
            dir = moveLeft ? 1 : -1;
        enemy.rb.velocity = Vector2.right * dir * speed * (multiplyDeltaTime ? Time.deltaTime : 1);
    }
}