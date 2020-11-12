//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class SlimeMovement : EnemiesMovement
//{
//    public float timeBtwJumps;
//    private float timeBtwJumpsValue;
//    private bool hasJump;

//    void Update()
//    {
//        if (GroundCheck())
//        {
//            if (!hasJump) rb.velocity = Vector2.zero;
//            JumpToPlayer();
//        }
//        else if (hasJump)
//        {
//            hasJump = false;
//        }

//        if (player.transform.position.x - transform.position.x < 0)
//        {
//            transform.eulerAngles = new Vector3(0, 180, 0);
//        }
//        else
//        {
//            transform.eulerAngles = Vector3.zero;
//        }
//    }

//    bool GroundCheck()
//    {
//        Vector3 offset = new Vector3(0, -sr.bounds.extents.y - .01f);
//        Vector2 size = new Vector2(sr.bounds.extents.x, .01f);
//        ExtDebug.DrawBoxCastBox(transform.position + offset, size / 2, Quaternion.identity, Vector2.down, 0, Color.red);
//        return Physics2D.BoxCast(transform.position + offset, size, 0, Vector2.down, 0, LayerMask.GetMask("Ground"));
//    }

//    void JumpToPlayer()
//    {
//        if (Time.time > timeBtwJumpsValue)
//        {
//            float distanceX = player.transform.position.x - transform.position.x;
//            rb.velocity = new Vector2(MathUtils.Signed(distanceX), 1).normalized * speed;
//            timeBtwJumpsValue = Time.time + timeBtwJumps;
//            hasJump = true;
//        }
//    }

//    private void OnCollisionEnter2D(Collision2D collision)
//    {
//        if (collision.transform.CompareTag("Player"))
//        {
//            player.Hurt(enemy.damage);
//        }
//    }
//}
