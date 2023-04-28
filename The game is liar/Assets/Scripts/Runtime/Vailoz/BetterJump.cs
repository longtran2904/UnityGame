using UnityEngine;

public class BetterJump : MonoBehaviour
{
    public float fallMultiplier = 8f;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.velocity.y < 0)
            rb.velocity += transform.up * Physics2D.gravity * rb.gravityScale * (fallMultiplier - 1) * Time.deltaTime;
    }
}
