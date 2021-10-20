using UnityEngine;

public partial class Enemy : MonoBehaviour
{
    void RotateEnemy()
    {
        if (lookAtPlayer)
        {
            float dir = 1;
            if (transform.right.x != Mathf.Sign(player.transform.position.x - transform.position.x))
                dir = -1;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * dir, transform.localScale.y, transform.localScale.z);
        }
        transform.rotation = Quaternion.Euler(transform.up.y > 0 ? 0 : 180, targetDir.x > 0 ? 0 : 180, 0);
    }

    bool IsInWall()
    {
        ExtDebug.DrawBox(transform.position, sr.bounds.extents, Quaternion.identity, Color.red);
        return Physics2D.BoxCast(transform.position, sr.bounds.size, 0, Vector2.zero, 0, LayerMask.GetMask("Ground"));
    }

    bool GroundCheck()
    {
        Vector2 pos = (Vector2)transform.position - new Vector2(0, sr.bounds.extents.y * transform.up.y);
        Vector2 size = new Vector2(sr.bounds.size.x, 0.1f);
        ExtDebug.DrawBox(pos, size / 2, Quaternion.identity, Color.green);
        return Physics2D.BoxCast(pos, size, 0, -transform.up, 0, LayerMask.GetMask("Ground"));
    }

    bool CliffCheck()
    {
        float offset = .2f;
        float length = .1f;
        Vector2 pos = (Vector2)transform.position + new Vector2((sr.bounds.extents.x + offset) * Mathf.Sign(rb.velocity.x), -sr.bounds.extents.y * transform.up.y);
        InternalDebug.DrawRay(pos, -transform.up * length, Color.cyan);
        return !Physics2D.Raycast(pos, -transform.up, length, LayerMask.GetMask("Ground"));
    }

    bool WallCheck()
    {
        float length = .2f;
        Vector2 pos = transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, 0, 0);
        InternalDebug.DrawRay(pos, transform.right * length, Color.yellow);
        RaycastHit2D hit = Physics2D.Raycast(pos, transform.right, length, LayerMask.GetMask("Ground"));
        return hit;
    }

    bool PlayerCheck(float range)
    {
        Vector2 pos = transform.position + new Vector3(sr.bounds.extents.x * transform.right.x, 0, 0);
        InternalDebug.DrawRay(pos, transform.right * range, Color.blue);
        return Physics2D.Raycast(pos, transform.right, range, LayerMask.GetMask("Player"));
    }

    float CaculateRotationToPlayer()
    {
        Vector2 difference = -transform.position + player.transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        return rotationZ;
    }


    bool IsInRange(float range)
    {
        return (player.transform.position - transform.position).sqrMagnitude < range * range;
    }

    bool InRangeX(float range)
    {
        return Mathf.Abs(player.transform.position.x - transform.position.x) < range;
    }

    bool InRangeY(float range)
    {
        return Mathf.Abs(player.transform.position.y - transform.position.y) < range;
    }
}
