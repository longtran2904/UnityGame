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
    public bool facingRight = true;
    private Animator anim;
    public RaycastHit2D hitInfo;
    public bool top;
    Camera mainCamera;
    Vector3 mousePos;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        mainCamera = Camera.main;
        rb.drag = GetDragFromAcceleration(Physics.gravity.magnitude, 10);
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

        // cast a box under the player
        hitInfo = Physics2D.BoxCast(transform.position - new Vector3(0, sprite.bounds.extents.y * 1.4f + boxSize.y + 0.01f, 0), boxSize, 0, Vector2.down, boxSize.y);

        // check to see if the player is grounded or not
        if (hitInfo && hitInfo.transform.tag == "Ground")        
            isGrounded = true;                 
        else
            isGrounded = false;

        moveInput = Input.GetAxisRaw("Horizontal");

        SwitchGravity();

        FlipPlayer();
    }

    private void FixedUpdate()
    {
        // make the player move
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
    }

    public static float GetDrag(float aVelocityChange, float aFinalVelocity)
    {
        return aVelocityChange / ((aFinalVelocity + aVelocityChange) * Time.fixedDeltaTime);
    }
    public static float GetDragFromAcceleration(float aAcceleration, float aFinalVelocity)
    {
        return GetDrag(aAcceleration * Time.fixedDeltaTime, aFinalVelocity);
    }

    void FlipPlayer()
    {
        if (top)
        {
            // flip the player towards the mouse when upside down
            if (mousePos.x - transform.position.x > 0)
            {
                transform.eulerAngles = new Vector3(0, 180, 180);
                facingRight = true;
                Debug.Log("upside down right");
            }
            else if (mousePos.x - transform.position.x < 0)
            {
                transform.eulerAngles = new Vector3(0, 0, 180);
                facingRight = false;
                Debug.Log("upside down left");
            }
        }
        else
        {
            // flip the player towards the mouse when normal
            if (mousePos.x - transform.position.x > 0)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                facingRight = true;
            }
            else if (mousePos.x - transform.position.x < 0)
            {
                transform.eulerAngles = new Vector3(0, 180, 0);
                facingRight = false;
            }
        }        
    }

    void SwitchGravity()
    {
        // switch the gravity upside down
        if (Input.GetButtonDown("Jump"))
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
}
