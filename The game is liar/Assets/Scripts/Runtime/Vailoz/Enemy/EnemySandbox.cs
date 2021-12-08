using UnityEngine;

public partial class Enemy : MonoBehaviour
{
    bool BoxCast(Vector2 pos, Vector2 size, Color color)
    {
        ExtDebug.DrawBox(pos, size / 2, Quaternion.identity, color);
        return Physics2D.BoxCast(pos, size, 0, Vector2.zero, 0, LayerMask.GetMask("Ground"));
    }

    bool RayCast(Vector2 offset, Vector2 dir, Color color, float length = .2f)
    {
        InternalDebug.DrawRay((Vector2)transform.position + offset, dir * length, color);
        return Physics2D.Raycast((Vector2)transform.position + offset, dir, length, LayerMask.GetMask("Ground"));
    }

    bool IsInRange(float range)
    {
        return (player.transform.position - transform.position).sqrMagnitude < range * range;
    }

    bool IsInRangeX(float range)
    {
        return Mathf.Abs(player.transform.position.x - transform.position.x) < range;
    }

    bool IsInRangeY(float range)
    {
        return Mathf.Abs(player.transform.position.y - transform.position.y) < range;
    }
}
