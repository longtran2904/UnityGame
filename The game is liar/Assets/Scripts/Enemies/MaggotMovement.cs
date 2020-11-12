//using UnityEngine;

//public class MaggotMovement : EnemiesMovement
//{
//    public float jumpForce;
//    bool isAttacking;
//    public float distanceToChase;
//    public float timeBtwJumps;
//    private float timeBtwJumpsValue;
//    public float timeBtwTeleports;
//    private float timeBtwTeleportsValue;

//    protected override void Start()
//    {
//        base.Start();
//        timeBtwJumpsValue = timeBtwJumps;
//    }

//    void Update()
//    {
//        Vector2 pos = (Vector2)transform.position - new Vector2(0, sr.bounds.extents.y * transform.up.y);
//        Vector2 size = new Vector2(sr.bounds.size.x, 0.1f);
//        RaycastHit2D groundCheck = Physics2D.BoxCast(pos, size, 0, -transform.up, 0, LayerMask.GetMask("Ground"));
//        ExtDebug.DrawBoxCastBox(pos, size / 2, Quaternion.identity, -transform.up, 0, Color.cyan);
//        InternalDebug.Log((bool)groundCheck);

//        float dirX = Mathf.Sign(player.transform.position.x - transform.position.x);
//        float distanceToPlayer = (player.transform.position - transform.position).sqrMagnitude;
//        InternalDebug.Log(distanceToPlayer);

//        if (groundCheck)
//        {
//            if (distanceToPlayer <= attackRange * attackRange && !isAttacking && Time.time >= timeBtwJumpsValue)
//            {
//                //InternalDebug.Log("Jump!");
//                rb.velocity = new Vector2(dirX, 1) * jumpForce;
//                isAttacking = true;
//                timeBtwJumpsValue = Time.time + timeBtwJumps;
//            }
//            else if (((Mathf.Abs(player.transform.position.y - transform.position.y) > 2) || distanceToPlayer > distanceToChase * distanceToChase) && 
//                player.controller.isGrounded && Time.time > timeBtwTeleportsValue)
//            {
//                //InternalDebug.Log("Teleport!");
//                Vector3 offset = new Vector2(-dirX * 1.5f, (sr.bounds.extents.y - player.GetComponent<SpriteRenderer>().bounds.extents.y) * transform.up.y);
//                transform.position = player.transform.position + offset;
//                timeBtwTeleportsValue = Time.time + timeBtwTeleports;
//            }
//            else
//            {
//                //InternalDebug.Log("Move: " + dirX);
//                isAttacking = false;
//                rb.velocity = new Vector2(dirX, 0) * speed * Time.deltaTime;
//            }
//        }

//        if (dirX > 0)
//        {
//            transform.eulerAngles = Vector3.zero;
//        }
//        else if (dirX < 0)
//        {
//            transform.eulerAngles = new Vector3(0, 180, 0);
//        }
//    }
//}
