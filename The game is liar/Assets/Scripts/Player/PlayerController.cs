using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// BoxCast and RayCast -> LayerMask.NameToLayer
// OverLapBox -> LayerMask.GetMask

public class PlayerController : MonoBehaviour
{    
    public float speed;
    public float fallSpeed;
    private Rigidbody2D rb;
    private float moveInput;
    bool isGrounded;
    private SpriteRenderer sprite;
    private Animator anim;    
    public BoolReference onGround; // false if the player is upside down
    Camera mainCamera;
    Vector3 mousePos;
    public ParticleSystem dust;

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
        mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        moveInput = Input.GetAxisRaw("Horizontal");
        if (moveInput != 0)
        {
            anim.SetBool("isRunning", true);
        }
        else
        {
            anim.SetBool("isRunning", false);
        }
        GroundCheck();
        FlipPlayer();
        Jump();
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
        groundCheck = Physics2D.BoxCast(transform.position - boxOffset, boxSize, 0, -transform.up, boxSize.y);
        ExtDebug.DrawBoxCastBox(transform.position - boxOffset, boxSize, Quaternion.identity, -transform.up, boxSize.y, Color.red);
        groundRemember -= Time.deltaTime;

        // Check ground
        if (groundCheck && groundCheck.transform.tag == "Ground")
        {
            groundRemember = groundRememberTime;
            if (isGrounded == false)
            {
                dust.Play();
                isGrounded = true;
            }
        }
        else
        {
            isGrounded = false;
        }
    }

    void FlipPlayer()
    {
        if (PauseMenu.isGamePaused)
        {
            return;
        }
        if (onGround.value)
        {
            // Normal
            if (mousePos.x - transform.position.x > 0)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
            }
            else if (mousePos.x - transform.position.x < 0)
            {
                transform.eulerAngles = new Vector3(0, 180, 0);
            }
        }
        else
        {
            // Upside down
            if (mousePos.x - transform.position.x > 0)
            {
                transform.eulerAngles = new Vector3(0, 180, 180);
            }
            else if (mousePos.x - transform.position.x < 0)
            {
                transform.eulerAngles = new Vector3(0, 0, 180);
            }
        }        
    }

    void Jump()
    {
        jumpPressedRememberValue -= Time.deltaTime;
        if (Input.GetButtonDown("Jump"))
        {
            jumpPressedRememberValue = jumpPressedRemember;
        }
        if (jumpPressedRememberValue > 0 && groundRemember > 0)
        {
            jumpPressedRememberValue = 0;
            rb.velocity = Vector2.zero;
            rb.gravityScale *= -1;
            dust.Play();
            Invoke("SwitchTop", .1f);
            AudioManager.instance.Play("PlayerJump");
        }
    }

    // Invoke by Jump()
    void SwitchTop()
    {
        onGround.value = !onGround.value;
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
            rb.velocity = new Vector2(knockbackForce.x, rb.velocity.y + transform.up.y * Physics2D.gravity.y * (500 - 1) * Time.deltaTime) * Time.deltaTime;
            knockbackCounter -= Time.deltaTime;
            return;
        }
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
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
}
