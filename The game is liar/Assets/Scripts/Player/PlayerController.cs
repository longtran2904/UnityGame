using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{    
    public float speed;
    public float fallSpeed;
    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private SpriteRenderer sprite;
    private Animator anim;
    public bool top; // True if the player is upside down
    Camera mainCamera;
    Vector3 mousePos;
    public ParticleSystem dust;

    // Jump and ground pressed remember
    public float jumpPressedRemember;
    private float jumpPressedRememberValue;
    public float groundRememberTime;
    private float groundRemember;

    // BoxCast
    Vector2 boxSize;
    Vector3 boxOffset;
    RaycastHit2D groundCheck;

    // Knock back
    public float knockbackTime;
    private float knockbackCounter;
    private Vector2 knockbackForce;
    private bool knockback;

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
        GetMouseAndMoveInput();
        SetAnimation();
        SetBoxSizeAndOffset();
        FlipBoxOffset();
        CastGroundCheckBox();
        CheckForGround();
        CheckForJumpInput();
        FlipPlayer();
    }

    void GetMouseAndMoveInput()
    {
        mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        moveInput = Input.GetAxisRaw("Horizontal");
    }

    void SetAnimation()
    {
        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            anim.SetBool("isRunning", true);
        }
        else
        {
            anim.SetBool("isRunning", false);
        }
    }

    void CheckForGround()
    {
        if (groundCheck && groundCheck.transform.tag == "Ground")
        {
            groundRemember = groundRememberTime;
            if (isGrounded == false)
            {
                CreateDust();
                isGrounded = true;
            }
        }
        else
        {
            isGrounded = false;
        }
    }

    void CastGroundCheckBox()
    {
        groundCheck = Physics2D.BoxCast(transform.position - boxOffset, boxSize, 0, -transform.up, boxSize.y);
        ExtDebug.DrawBoxCastBox(transform.position - boxOffset, boxSize, Quaternion.identity, -transform.up, boxSize.y, Color.red);
        groundRemember -= Time.deltaTime;
    }

    void SetBoxSizeAndOffset()
    {
        boxSize = new Vector2(0.25f, 0.01f);
        boxOffset = new Vector3(0, sprite.bounds.extents.y + boxSize.y + 0.01f, 0);
    }

    void FlipBoxOffset()
    {
        if (top)
        {
            boxOffset = -boxOffset;
        }
    }

    void FlipPlayer()
    {
        if (PauseMenu.isGamePaused)
        {
            return;
        }
        if (top)
        {
            FlipWhenUpsideDown();
        }
        else
        {
            FlipWhenNormal();
        }        
    }

    void FlipWhenUpsideDown()
    {
        if (mousePos.x - transform.position.x > 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 180);
        }
        else if (mousePos.x - transform.position.x < 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 180);
        }
    }

    void FlipWhenNormal()
    {
        if (mousePos.x - transform.position.x > 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (mousePos.x - transform.position.x < 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }

    void CheckForJumpInput()
    {
        DecreaseAndRememberJumpPressedTime();
        if (jumpPressedRememberValue > 0 && groundRemember > 0)
        {
            FlipGravity();
        }
    }

    void DecreaseAndRememberJumpPressedTime()
    {
        jumpPressedRememberValue -= Time.deltaTime;
        if (Input.GetButtonDown("Jump"))
        {
            jumpPressedRememberValue = jumpPressedRemember;
        }
    }

    void FlipGravity()
    {
        ResetVelocityAndJumpInput();
        CreateDust();
        Invoke("SwitchTop", .1f);
        AudioManager.instance.Play("PlayerJump");
    }

    void ResetVelocityAndJumpInput()
    {
        jumpPressedRememberValue = 0;
        rb.velocity = Vector2.zero;
        rb.gravityScale *= -1;
    }

    void SwitchTop()
    {
        top = !top;
    }

    void CreateDust()
    {
        dust.Play();
    }

    private void FixedUpdate()
    {
        // Knock the player back
        if (knockbackCounter < 0)
        {
            knockback = false;
        }
        if (knockback == false)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            knockback = true;
        }
        if (knockbackCounter > 0.5 * knockbackTime)
        {
            rb.velocity = new Vector2(knockbackForce.x, knockbackForce.y) * Time.deltaTime;
            knockbackCounter -= Time.deltaTime;
            return;
        }
        else if (knockbackCounter > 0)
        {
            rb.velocity = new Vector2(knockbackForce.x, SetGravityWhenKnockback()) * Time.deltaTime;
            knockbackCounter -= Time.deltaTime;
            return;
        }
        MovePlayer();
    }

    float SetGravityWhenKnockback()
    {
        float velocityY = rb.velocity.y + transform.up.y * Physics2D.gravity.y * (500 - 1) * Time.deltaTime;
        return velocityY;
    }

    public void KnockBack(Vector2 _knockbackForce)
    {
        if (!groundCheck)
        {
            return;
        }
        knockbackCounter = knockbackTime;
        knockbackForce = _knockbackForce;
        rb.velocity = _knockbackForce;
    }

    void MovePlayer()
    {
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
    }
}
