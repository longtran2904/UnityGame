using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{

    public float speed;
    public float jumpForce;
    public float extraJumps;
    private float extraJumpsValue;
    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private SpriteRenderer sprite;
    public float dashSpeed;
    private bool isDashing;
    public bool facingRight = true;
    private bool jumpFromGround;
    private float dashTime;
    public float startDashTime;
    private bool canDash;
    private Animator anim;
    public RaycastHit2D hitInfo;
    private int jumpCount;
    bool top;

    // Start is called before the first frame update
    void Start()
    {
        extraJumpsValue = extraJumps;
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        dashTime = startDashTime;
        anim = GetComponent<Animator>();
        rb.drag = GetDragFromAcceleration(Physics.gravity.magnitude, 10);
        Debug.Log(rb.drag);
    }

    // Update is called once per frame
    void Update()
    {
        if (anim)
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

        Vector2 boxSize = new Vector2(0.25f, 0.01f);

        hitInfo = Physics2D.BoxCast(transform.position - new Vector3(0, sprite.bounds.extents.y * 1.4f + boxSize.y + 0.01f, 0), boxSize, 0, Vector2.down, boxSize.y);

        if (hitInfo && hitInfo.transform.tag == "Ground")        
            isGrounded = true;                 
        else
            isGrounded = false;

        moveInput = Input.GetAxisRaw("Horizontal");

        JumpInput();

        SwitchGravity();

        FlipPlayer();

        DashInput();

    }

    public static float GetDrag(float aVelocityChange, float aFinalVelocity)
    {
        return aVelocityChange / ((aFinalVelocity + aVelocityChange) * Time.fixedDeltaTime);
    }
    public static float GetDragFromAcceleration(float aAcceleration, float aFinalVelocity)
    {
        return GetDrag(aAcceleration * Time.fixedDeltaTime, aFinalVelocity);
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);

        if (isDashing == true)
        {
            if (facingRight == true)
            {
                rb.velocity = Vector2.right * dashSpeed;
            }
            else
            {
                rb.velocity = Vector2.left * dashSpeed;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Hook")
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
            extraJumpsValue = extraJumps;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Hook")
        {
            rb.gravityScale = 3;
        }
    }

    void DashInput()
    {

        if (isGrounded)
        {
            canDash = true;
        }

        // Press Space to dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            isDashing = true;
        }

        // Make the player don't have gravity while dashing
        if (isDashing == true && dashTime > 0)
        {
            dashTime -= Time.deltaTime;
            rb.gravityScale = 0;
        }
        else if (dashTime <= 0)
        {
            dashTime = startDashTime;
            rb.gravityScale = 3;
            isDashing = false;
            canDash = false;
        }
    }


    void FlipPlayer()
    {
        if (top)
        {
            // flip the player when move right or left
            if (moveInput > 0 && isDashing == false)
            {
                transform.eulerAngles = new Vector3(0, 180, 180);
                facingRight = true;
            }
            else if (moveInput < 0 && isDashing == false)
            {
                transform.eulerAngles = new Vector3(0, 0, 180);
                facingRight = false;
            }
        }
        else
        {
            // flip the player when move right or left
            if (moveInput > 0 && isDashing == false)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                facingRight = true;
            }
            else if (moveInput < 0 && isDashing == false)
            {
                transform.eulerAngles = new Vector3(0, 180, 0);
                facingRight = false;
            }
        }        
    }

    void SwitchGravity()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {

            rb.gravityScale *= -1;

            if (top == false)
            {
                transform.eulerAngles = new Vector3(0, 0, 180);
            }
            else
            {
                transform.eulerAngles = Vector3.zero;
            }
            
            top = !top;
        }
    }

    void JumpInput()
    {
        if (isGrounded && extraJumpsValue < extraJumps)
        {
            extraJumpsValue = extraJumps;
        }

        if (isGrounded && jumpFromGround)
        {
            jumpCount += 1;
        }

        if (jumpCount > 1 && isGrounded)
        {
            jumpFromGround = false;
            jumpCount = 0;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.velocity = Vector2.up * jumpForce;
                jumpFromGround = true;
            }
            else if (!isGrounded && extraJumpsValue > 0)
            {
                rb.velocity = Vector2.up * jumpForce;
                extraJumpsValue--;
            }
            else if (!isGrounded && extraJumpsValue == 0 && !jumpFromGround)
            {
                rb.velocity = Vector2.up * jumpForce;
                jumpFromGround = true;
            }
        }
    }
}
