using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemies : MonoBehaviour
{

    private Rigidbody2D rb;

    public float speed;

    private bool touchingWall;

    private RaycastHit2D hitInfo;

    BoxCollider2D box;

    public Transform wallCheck;

    public float radius;

    public LayerMask whatIsGround;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        touchingWall = Physics2D.OverlapCircle(wallCheck.position, radius, whatIsGround);
        Debug.Log(touchingWall);
        //hitInfo = Physics2D.Raycast(transform.position + new Vector3(box.bounds.extents.x + 0.01f, -box.bounds.extents.y + .3f, 0), transform.right, 0.1f);

        //Debug.DrawRay(transform.position + new Vector3(box.bounds.extents.x + 0.01f, -box.bounds.extents.y + .3f, 0), transform.right, Color.red);

        //if (wallCheck)
        //{
        //    touchingWall = true;
        //}
        //else
        //{
        //    touchingWall = false;
        //}

        if (touchingWall)
        {
            transform.eulerAngles += new Vector3(0, 0, 90);
        }
    }
    
    void FixedUpdate()
    {
        rb.velocity = transform.right * speed;
    }
}
