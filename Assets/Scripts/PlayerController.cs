using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private new SpriteRenderer renderer;
    private new Rigidbody2D rigidbody;
    private BoxCollider2D playerCollider;

    private int layerMaskGround;
    private float heightTestPlayer;
    private float floatingPointErrorTolerance;

    private Vector3 startingPosition;

    public float moveSpeed = 1;
    public float jumpForce = 1;
    public Dictionary<string, KeyCode[]> inputKeys = new Dictionary<string, KeyCode[]>();

    void Start()
    {
        animator = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
        rigidbody = GetComponent<Rigidbody2D>();

        playerCollider = GetComponent<BoxCollider2D>();
        heightTestPlayer = playerCollider.bounds.extents.y + 0.05f;
        layerMaskGround = LayerMask.GetMask("Ground");

        // Floating point (decimal) numbers aren't super precise so we add an acceptable error tolerance to our checks
        floatingPointErrorTolerance = 0.01f;

        inputKeys = new Dictionary<string, KeyCode[]>()
        {
            {"left", new KeyCode[] {KeyCode.A} },
            {"right", new KeyCode[] {KeyCode.D} },
            {"jump", new KeyCode[] {KeyCode.W, KeyCode.Space} },
            {"reset", new KeyCode[] {KeyCode.R} }
        };

        startingPosition = transform.position;
    }

    // Runs once per graphical frame. Great for animation behavior, but steady FPS is not guaranteed so not good for physics
    void Update()
    {
        // Animation control
        // Do the walking animation if the player is moving and on the ground
        if (Math.Abs(rigidbody.velocity.x) > floatingPointErrorTolerance && IsGrounded())
        {
            animator.Play("Walk", 0);
        }
        // Do idle animation if player isn't moving and on the ground
        if (Math.Abs(rigidbody.velocity.x) <= floatingPointErrorTolerance && IsGrounded())
        {
            animator.Play("Idle", 0);
        }
        // Do jump animation if player is off the ground
        if (!IsGrounded())
        {
            animator.Play("Jump", 0);
        }
    }

    // Always runs at a consistent FPS as long as it isn't slowed down by too much computation. Good for physics calculations
    void FixedUpdate()
    {
        // Remember the speed and direction of the player in the previous frame
        Vector3 velocityVector = rigidbody.velocity;

        // This will allow both left/right keys to be pressed and intuitively cancel each other out
        float horizontalMoveSpeed = 0;

        if (checkInput("right"))
        {
            horizontalMoveSpeed += moveSpeed;
        }
        if (checkInput("left"))
        {
            horizontalMoveSpeed -= moveSpeed;
        }

        // Add jump force when player is on the ground
        if (checkInput("jump") && IsGrounded())
        {
            var currentVerticalSpeed = velocityVector.y;
            var targetVerticalSpeed = jumpForce - currentVerticalSpeed;
            velocityVector += new Vector3(0, targetVerticalSpeed, 0);
        }

        // Modify player speed and direction after input checks
        rigidbody.velocity = new Vector3(horizontalMoveSpeed, velocityVector.y, 0);

        // Flip the sprite if the player changes direction, but don't change direction just because the player stops moving
        if (horizontalMoveSpeed != 0)
        {
            renderer.flipX = horizontalMoveSpeed < 0;
        }

        if (checkInput("reset"))
        {
            transform.position = startingPosition;
            rigidbody.velocity = new Vector3(0, 0, 0);
        }
    }

    // Cast 2 rays on the sides of the player facing downward. If these rays collide with a solid ground object, the player is considered to be grounded
    private bool IsGrounded()
    {
        var ray1Origin = playerCollider.bounds.center - new Vector3(playerCollider.bounds.extents.x, 0, 0);
        var ray2Origin = playerCollider.bounds.center + new Vector3(playerCollider.bounds.extents.x, 0, 0);
        RaycastHit2D hit1 = Physics2D.Raycast(ray1Origin, Vector2.down, heightTestPlayer, layerMaskGround);
        RaycastHit2D hit2 = Physics2D.Raycast(ray2Origin, Vector2.down, heightTestPlayer, layerMaskGround);
        bool isGrounded = hit1.collider != null || hit2.collider != null;
        Debug.DrawRay(ray1Origin, Vector2.down * heightTestPlayer, isGrounded ? Color.green : Color.red, Time.deltaTime * 2);
        Debug.DrawRay(ray2Origin, Vector2.down * heightTestPlayer, isGrounded ? Color.green : Color.red, Time.deltaTime * 2);
        return isGrounded;
    }

    // Function to check if any valid input for a given action is pressed. Supports multiple input bindings
    private bool checkInput(string type)
    {
        if (inputKeys.TryGetValue(type, out var keys))
        {
            return keys.Any(key => Input.GetKey(key));
        }
        else
        {
            return false;
        }
    }
}
