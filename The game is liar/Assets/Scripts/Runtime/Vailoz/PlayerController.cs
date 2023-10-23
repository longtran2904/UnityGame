using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float fallSpeed;
    public Vector3Variable position;
    public ParticleSystem leftDust;
    public ParticleSystem rightDust;

    public RangedFloat timeBtwFootsteps;
    private float timeBtwFootstepsValue;

    private float moveInput;
    private Rigidbody2D rb;
    private Animator anim;
    [HideInInspector] public bool groundCheck;
    private Coroutine resetSize;
    private Vector2 spriteExtents;

    // Jump and ground pressed remember
    public float jumpPressedRemember;
    private float jumpPressedRememberValue;
    public float groundRememberTime;
    private float groundRemember;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.drag = MathUtils.GetDragFromAcceleration(Physics2D.gravity.magnitude, fallSpeed);
        spriteExtents = GetComponent<SpriteRenderer>().bounds.extents;
        resetSize = this.EmptyCoroutine();
        groundCheck = CastBox();
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Sign(GameInput.GetDirToMouse(transform.position, 1).x) != transform.right.x)
            transform.Rotate(0, 180, 0);

        float lastMoveInput = moveInput;
        {
            moveInput = GameInput.GetAxis(AxisType.Horizontal);
            rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
        }

        bool lastCheck = groundCheck;
        groundCheck = CastBox();

        if (groundCheck)
        {
            groundRemember = groundRememberTime;
            if (moveInput != 0 && Time.time > timeBtwFootstepsValue)
            {
                AudioManager.PlayAudio(AudioType.Player_Footstep);
                timeBtwFootstepsValue = Time.time + timeBtwFootsteps.randomValue;
            }

            if (!lastCheck)
            {
                StartJumpEffect(false);
                if (moveInput != 0)
                    anim.Play("Move");
            }
            else if (moveInput != lastMoveInput)
            {
                if (lastMoveInput == 0)
                {
                    PlayDust(-moveInput);
                    anim.Play("Move");
                }
                else
                {
                    PlayDust(lastMoveInput);
                    if (moveInput == 0)
                        anim.Play("Idle");
                }
            }
        }
        else
        {
            groundRemember -= Time.deltaTime;
            if (lastCheck)
                anim.Play("Idle");
        }

        if (GameInput.GetInput(InputType.Jump))
            jumpPressedRememberValue = jumpPressedRemember;
        else
            jumpPressedRememberValue -= Time.deltaTime;

        if (jumpPressedRememberValue > 0 && groundRemember > 0)
        {
            jumpPressedRememberValue = 0;
            groundRemember = 0;
            rb.velocity = Vector2.zero;
            rb.gravityScale *= -1;

            this.InvokeAfter(.1f, () => transform.Rotate(180, 0, 0));
            StartJumpEffect(true);
        }

        position.value = transform.position;
    }

    bool CastBox()
    {
        Vector2 boxSize = new Vector2(spriteExtents.x / 1.5f, 0.02f);
        Vector3 boxPos = transform.position - new Vector3(0, spriteExtents.y + boxSize.y + .075f) * Mathf.Sign(rb.gravityScale);
        GameDebug.DrawBox(boxPos, boxSize, Color.red);
        return Physics2D.BoxCast(boxPos, boxSize, 0, Vector2.zero, 0, LayerMask.GetMask("Ground"));
    }

    void StartJumpEffect(bool isJumping)
    {
        CameraSystem.instance.Shake(ShakeMode.PlayerJump);
        PlayDust(-moveInput);
        AudioManager.PlayAudio(isJumping ? AudioType.Player_Jump : AudioType.Player_Land);

        // Change Size
        transform.localScale = isJumping ? new Vector3(.75f, 1.25f) : new Vector3(1.25f, .75f);
        transform.position -= GetPosOnGround();

        StopCoroutine(resetSize);
        resetSize = this.InvokeAfter(.2f, () => {
            transform.localScale = new Vector3(1f, 1f);
            if (!isJumping)
                transform.position -= GetPosOnGround();
        });

        Vector3 GetPosOnGround()
        {
            float groundHeight = Physics2D.BoxCast(transform.position, new Vector2(spriteExtents.x / 2, 0.01f), 0, -transform.up, spriteExtents.y * 2, LayerMask.GetMask("Ground")).distance;
            Vector3 offset = new Vector3(0, groundHeight - spriteExtents.y * transform.localScale.y) * transform.up.y;
            Debug.DrawRay(transform.position, -transform.up * groundHeight, Color.blue);
            return offset;
        }
    }

    void PlayDust(float dir)
    {
        if (dir >= 0)
            rightDust.Play();
        if (dir <= 0)
            leftDust.Play();
    }
}
