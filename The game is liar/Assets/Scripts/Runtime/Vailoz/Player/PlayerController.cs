using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// NOTE: Physics2D cast -> GetMask
//       Physics2D cast always return a RaycastHit2D object (implicit convert to bool), the collider property of the object will equal to null if it didn't hit anything
//       Physics2D cast will ignore any collider that have the layer mask different the layerMask property
//       transform.right change when rotation/eulerAngle change (angle = (0, 180, 0) -> transform.right = (-1, 0) )

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float fallSpeed;
    public BoolReference onGround; // false if the player is upside down
    public ParticleSystem dust;
    [HideInInspector] public bool isJumping;

    public SoundCue footStep;
    public SoundCue touchGround;

    private float moveInput;
    private float lastMoveInput;
    private bool isGrounded;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;    
    private Camera mainCamera;
    private Vector3 mousePos;

    // Jump and ground pressed remember
    public float jumpPressedRemember;
    private float jumpPressedRememberValue;
    public float groundRememberTime;
    private float groundRemember;

    // BoxCast
    Vector3 boxOffset;
    RaycastHit2D groundCheck;

    // Knock back
    public float knockbackTime;
    private float knockbackCounter;

    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    void Setup()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        mainCamera = Camera.main;
        rb.drag = MathUtils.GetDragFromAcceleration(Physics2D.gravity.magnitude, fallSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        if (knockbackCounter > 0)
        {
            knockbackCounter -= Time.deltaTime;
        }
        else
        {
            moveInput = Input.GetAxisRaw("Horizontal");
        }
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);

        if (moveInput != lastMoveInput)
        {
            if (moveInput != 0)
                anim.SetBool("isRunning", true);
            else
                anim.SetBool("isRunning", false);
        }

        if (moveInput != 0 && isGrounded)
            AudioManager.instance?.PlaySfx(footStep);

        if (PauseMenu.isGamePaused) return;
        transform.eulerAngles = new Vector3(onGround.value ? 0 : 180, mousePos.x - transform.position.x > 0 ? 0 : 180, 0);

        GroundCheck();
        Jump();
        lastMoveInput = moveInput;
    }

    void GroundCheck()
    {
        // Cast box
        Vector2 boxSize = new Vector2(0.25f, 0.01f);
        boxOffset = new Vector3(0, sprite.bounds.extents.y + boxSize.y + 0.01f, 0);
        if (!onGround.value)
        {
            boxOffset = -boxOffset;
        }
        groundCheck = Physics2D.BoxCast(transform.position - boxOffset, boxSize, 0, -transform.up, boxSize.y, LayerMask.GetMask("Ground"));
        ExtDebug.DrawBoxCastBox(transform.position - boxOffset, boxSize, Quaternion.identity, -transform.up, boxSize.y, Color.red);
        groundRemember -= Time.deltaTime;

        // Check ground
        if (groundCheck)
        {
            groundRemember = groundRememberTime;
            if (isGrounded == false) // When fall and touch ground
            {
                dust.Play();
                AudioManager.instance?.PlaySfx(touchGround);
                isGrounded = true;
                isJumping = false;
            }
        }
        else
            isGrounded = false;
    }

    void Jump()
    {
        jumpPressedRememberValue -= Time.deltaTime;
        if (Input.GetButtonDown("Jump"))
            jumpPressedRememberValue = jumpPressedRemember;

        if (jumpPressedRememberValue > 0 && groundRemember > 0)
        {
            jumpPressedRememberValue = 0;
            rb.velocity = Vector2.zero;
            rb.gravityScale *= -1;
            dust.Play();
            isJumping = true;
            Invoke("SwitchTop", .1f);
            AudioManager.instance?.PlaySfx("PlayerJump");
        }
    }

    // Invoke by Jump()
    void SwitchTop()
    {
        onGround.value = !onGround.value;
    }

    public void KnockBack()
    {
        knockbackCounter = knockbackTime;
        rb.velocity = Vector2.zero;
        moveInput = 0;
    }
}
