using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    
    public float speed;

    private Rigidbody2D rb;

    private float moveInput;

    private bool isGrounded;

    private SpriteRenderer sprite;

    private Animator anim;

    public RaycastHit2D hitInfo;

    [HideInInspector]
    public bool top;

    Camera mainCamera;
    Vector3 mousePos;

    public ParticleSystem dust;

    public float jumpPressedRemember;
    private float jumpPressedRememberValue;

    public float groundRememberTime;
    private float groundRemember;

    private AudioManager audioManager;

    // Knock back
    public float knockbackTime;
    private float knockbackCounter;
    private Vector2 knockbackForce;
    private bool knockback;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        mainCamera = Camera.main;
        rb.drag = GetDragFromAcceleration(Physics2D.gravity.magnitude, 8);
        audioManager = FindObjectOfType<AudioManager>();
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // play the "run" animation
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
        Vector3 boxOffset = new Vector3(0, sprite.bounds.extents.y + boxSize.y + 0.01f, 0);

        if (top)
        {
            boxOffset = -boxOffset;
        }

        // cast a box under the player
        hitInfo = Physics2D.BoxCast(transform.position - boxOffset, boxSize, 0, -transform.up, boxSize.y);
        ExtDebug.DrawBoxCastBox(transform.position - boxOffset, boxSize, Quaternion.identity, -transform.up, boxSize.y, Color.red);

        groundRemember -= Time.deltaTime;

        // check to see if the player is grounded or not
        if (hitInfo && hitInfo.transform.tag == "Ground")
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

        moveInput = Input.GetAxisRaw("Horizontal");

        SwitchGravity();

        FlipPlayer();
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
            rb.velocity = new Vector2(knockbackForce.x, rb.velocity.y) * Time.deltaTime;

            knockbackCounter -= Time.deltaTime;

            return;
        }

        // Make the player move
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
    }

    #region Drag Caculation
    public static float GetDrag(float aVelocityChange, float aFinalVelocity)
    {
        return aVelocityChange / ((aFinalVelocity + aVelocityChange) * Time.fixedDeltaTime);
    }
    public static float GetDragFromAcceleration(float aAcceleration, float aFinalVelocity)
    {
        return GetDrag(aAcceleration * Time.fixedDeltaTime, aFinalVelocity);
    }
    #endregion

    void FlipPlayer()
    {
        if (PauseMenu.isGamePaused)
        {
            return;
        }

        if (top)
        {
            // flip the player towards the mouse when upside down
            if (mousePos.x - transform.position.x > 0)
            {
                transform.eulerAngles = new Vector3(0, 180, 180);
                //facingRight = true;
            }
            else if (mousePos.x - transform.position.x < 0)
            {
                transform.eulerAngles = new Vector3(0, 0, 180);
                //facingRight = false;
            }
        }
        else
        {
            // flip the player towards the mouse when normal
            if (mousePos.x - transform.position.x > 0)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                //facingRight = true;
            }
            else if (mousePos.x - transform.position.x < 0)
            {
                transform.eulerAngles = new Vector3(0, 180, 0);
                //facingRight = false;
            }
        }        
    }

    void SwitchGravity()
    {
        jumpPressedRememberValue -= Time.deltaTime;

        if (Input.GetButtonDown("Jump"))
        {
            jumpPressedRememberValue = jumpPressedRemember;
        }

        // switch the gravity upside down
        if (jumpPressedRememberValue > 0 && groundRemember > 0)
        {
            jumpPressedRememberValue = 0;

            CreateDust();

            rb.velocity = Vector2.zero;

            rb.gravityScale *= -1;

            Invoke("SwitchTop", .1f);

            audioManager.Play("PlayerJump");
        }
    }

    void SwitchTop()
    {
        top = !top;
    }

    void CreateDust()
    {
        dust.Play();
    }

    public void KnockBack(Vector2 _knockbackForce)
    {
        if (!hitInfo || (hitInfo && hitInfo.collider.CompareTag("Ground")))
        {
            return;
        }
        knockbackCounter = knockbackTime;
        knockbackForce = _knockbackForce;
        rb.velocity = _knockbackForce;
    }
}
