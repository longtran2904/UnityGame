//using System.Collections;
//using UnityEngine;

//[CreateAssetMenu(menuName = "Enemy/Action/Teleport")]
//public class TeleportAction : EnemyAction
//{
//    private SpriteRenderer playerSr;
//    private TrailRenderer trail;

//    public override void Act(Enemy enemy)
//    {
//        if (!playerSr) playerSr = enemy.player.GetComponent<SpriteRenderer>();
//        if (!trail) trail = enemy.GetComponent<TrailRenderer>();

//        if (trail)
//        {
//            trail.enabled = true;
//            enemy.StartCoroutine(DisableTrail());
//        }

//        Vector3 offset = new Vector2(Mathf.Sign(enemy.transform.position.x - enemy.player.transform.position.x) * 1.5f, (enemy.sr.bounds.extents.y - playerSr.bounds.extents.y) /* * enemy.transform.up.y*/);
//        enemy.transform.position = enemy.player.transform.position + offset;
//    }

//    IEnumerator DisableTrail()
//    {
//        yield return new WaitForSeconds(.1f);
//        trail.enabled = false;
//    }
//}
