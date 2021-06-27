//using UnityEngine;

//[CreateAssetMenu(menuName = "Enemy/Decision/Position Difference")]
//public class PositionDecision : EnemyDecision
//{
//    public enum DetectType { X, Y, Distance }
//    public DetectType type;

//    [ShowWhen("type", DetectType.X)]        public float xDiff;
//    [ShowWhen("type", DetectType.Y)]        public float yDiff;
//    [ShowWhen("type", DetectType.Distance)] public float distanceDiff;

//    public override bool Decide(Enemy enemy)
//    {
//        switch (type)
//        {
//            case DetectType.X:
//                return Mathf.Abs(enemy.transform.position.x - enemy.player.transform.position.x) > xDiff ? true : false;
//            case DetectType.Y:
//                return Mathf.Abs(enemy.transform.position.y - enemy.player.transform.position.y) > yDiff ? true : false;
//            case DetectType.Distance:
//                return enemy.IsInRange(distanceDiff)                                                     ? true : false;
//            default:
//                return false;
//        }
//    }
//}
