//using UnityEngine;

//[CreateAssetMenu(menuName = "Enemy/Action/Jump")]
//public class JumpAction : EnemyAction
//{
//    public float jumpForce;
//    [Range(0f, 90f)] public float jumpAngle;

//    public override void Act(Enemy enemy)
//    {
//        enemy.rb.velocity = new Vector2(Mathf.Cos(jumpAngle) * Mathf.Sign(enemy.player.transform.position.x - enemy.transform.position.y), Mathf.Sin(jumpAngle)) * jumpForce;
//    }
//}