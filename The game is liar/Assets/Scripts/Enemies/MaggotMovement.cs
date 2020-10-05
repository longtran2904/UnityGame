using UnityEngine;

public class MaggotMovement : EnemiesMovement
{
    private RaycastHit2D hitInfo;
    int groundMask;
    Vector2 wallRayPos;
    Vector2 groundRayPos;
    RaycastHit2D groundCheck, wallCheck;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        groundMask = LayerMask.GetMask("Ground");
    }

    // Update is called once per frame
    void Update()
    {
        CheckForGround();
        CheckForWall();
        Rotate();
        DebugCollision();
        rb.velocity = transform.right * speed * Time.deltaTime;
    }

    void Rotate()
    {
        if (wallCheck)
        {
            RotateUp();
        }
        else if (!groundCheck)
        {
            RotateDown();
        }
    }

    void RotateUp()
    {
        if ((transform.eulerAngles.z / 90) % 2 != 0)
        {
            transform.position += new Vector3(0, sr.bounds.extents.y - sr.bounds.extents.x, 0) * transform.right.y;
        }
        else
        {
            transform.position += new Vector3(sr.bounds.extents.x - sr.bounds.extents.y, 0, 0) * transform.right.x;
        }
        transform.rotation *= Quaternion.Euler(0, 0, 90);
    }

    void RotateDown()
    {
        if ((transform.eulerAngles.z / 90) % 2 != 0)
        {
            transform.position += new Vector3(0, sr.bounds.extents.y + sr.bounds.extents.x, 0) * transform.right.y;
        }
        else
        {
            transform.position += new Vector3(sr.bounds.extents.x + sr.bounds.extents.y, 0, 0) * transform.right.x;
        }
        float z = transform.eulerAngles.z - 90;
        transform.rotation *= Quaternion.Euler(0, 0, -90);
    }

    void CheckForWall()
    {
        float rayLength = .01f;
        if ((transform.eulerAngles.z / 90) % 2 != 0)
        {
            wallRayPos = transform.position + new Vector3(0, sr.bounds.extents.y, 0) * transform.right.y;
            wallCheck = Physics2D.Raycast(wallRayPos, transform.right, rayLength, groundMask);
        }
        else
        {
            wallRayPos = transform.position + new Vector3((sr.bounds.extents.x) * transform.right.x, 0, 0);
            wallCheck = Physics2D.Raycast(wallRayPos, transform.right, rayLength, groundMask);
        }
    }

    void CheckForGround()
    {
        float rayLength = 1;
        if ((transform.eulerAngles.z / 90) % 2 != 0)
        {
            groundRayPos = transform.position + new Vector3(sr.bounds.extents.x * -transform.up.x, sr.bounds.extents.y * transform.right.y, 0);
            groundCheck = Physics2D.Raycast(groundRayPos, -transform.up, rayLength, groundMask);
        }
        else
        {
            groundRayPos = transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, sr.bounds.extents.y * -transform.up.y, 0);
            groundCheck = Physics2D.Raycast(groundRayPos, -transform.up, rayLength, groundMask);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        enemy.DamagePlayerWhenCollide(collision);
    }

    [System.Diagnostics.Conditional("DEBUG_MAGGOT")]
    void DebugCollision()
    {
        Debug.DrawRay(wallRayPos, transform.right, Color.blue);
        Debug.DrawRay(groundRayPos, -transform.up, Color.green);
    }
}
