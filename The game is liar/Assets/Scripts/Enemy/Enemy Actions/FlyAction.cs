//using UnityEngine;

//[CreateAssetMenu(menuName = "Enemy/Action/Fly")]
//public class FlyAction : EnemyAction
//{
//    public enum FlyType { Normal, Away };

//    public FlyType flyType;

//    public override void Act(Enemy enemy)
//    {
//        enemy.rb.velocity = (enemy.player.transform.position - enemy.transform.position).normalized * enemy.speed * Time.deltaTime * (flyType == FlyType.Away ? -1 : 1);
//    }
//}