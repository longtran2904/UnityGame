using UnityEngine;

public class BulletHolder : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Move(Vector2 dir, float speed)
    {
        rb.velocity = dir * speed;
    }
}
