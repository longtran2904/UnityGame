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
    public Vector3Variable position;
    public BoolReference onGround; // false if the player is upside down
    public ParticleSystem dust;

    public AudioManager audioManager;
    public SoundCue footStep;
    public SoundCue touchGround;

    [HideInInspector] public bool isGrounded;
    private float moveInput;
    private float lastMoveInput;
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
            audioManager?.PlaySfx(footStep);

        if (PauseMenu.isGamePaused) return;
        transform.eulerAngles = new Vector3(onGround.value ? 0 : 180, mousePos.x - transform.position.x > 0 ? 0 : 180, 0);

        GroundCheck();
        Jump();
        lastMoveInput = moveInput;

        position.value = transform.position;
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
                dust?.Play();
                audioManager?.PlaySfx(touchGround);
                isGrounded = true;
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
            dust?.Play();
            Invoke("SwitchTop", .1f);
            audioManager?.PlaySfx("PlayerJump");
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

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void EnterDemo()
    {
        dust = null;
        audioManager = null;

        GetComponent<SpriteRenderer>().enabled = false;
        Destroy(GetComponent<Player>());
        Destroy(GetComponent<PlayerCombat>());
        Destroy(GetComponent<ItemManager>());

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        transform.DetachChildren();
    }
}
