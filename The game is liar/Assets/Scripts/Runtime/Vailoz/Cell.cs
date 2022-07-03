using UnityEngine;

public class Cell : MonoBehaviour, IPooledObject
{
    public float speed;
    public int addedMoney;
    public IntVariable playerMoney;
    public Vector3Variable playerPos;

    private bool startMoving;
    private Rigidbody2D rb;
    private Collider2D collision;

    public void OnObjectInit()
    {
        rb = GetComponent<Rigidbody2D>();
        collision = GetComponent<Collider2D>();
    }

    public void OnObjectSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        StartMoving(false);
        rb.velocity = new Vector2(Random.Range(-1, 1), Random.value).normalized * speed;
        GameInput.BindEvent(GameEventType.EndRoom, _ => StartMoving(true));
    }

    void Update()
    {
        if (startMoving)
            rb.velocity = (playerPos.value - transform.position).normalized * speed;
        else if (MathUtils.InRange(transform.position, playerPos.value, 5f))
                StartMoving(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.collider);
    }

    void HandleCollision(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AudioManager.PlayAudio(AudioType.Game_Pickup);
            playerMoney.value += addedMoney;
            gameObject.SetActive(false);
        }
    }

    void StartMoving(bool start)
    {
        startMoving = start;
        collision.isTrigger = start;
        collision.attachedRigidbody.bodyType = start ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
    }
}
