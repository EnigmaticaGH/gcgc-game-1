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
    private Vector3 startingPosition;
    private Quaternion startingRotation;

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

        inputKeys = new Dictionary<string, KeyCode[]>()
        {
            {"left", new KeyCode[] {KeyCode.A} },
            {"right", new KeyCode[] {KeyCode.D} },
            {"jump", new KeyCode[] {KeyCode.W, KeyCode.Space} },
            {"reset", new KeyCode[] {KeyCode.R} }
        };

        startingPosition = transform.position;
        startingRotation = transform.rotation;
    }

    void Update()
    {
        if ((checkInput("left") || checkInput("right")) && IsGrounded())
        {
            animator.Play("Walk", 0);
        }
        if (!IsGrounded())
        {
            animator.Play("Jump", 0);
        }
        if (IsGrounded() && !checkInput("left") && !checkInput("right"))
        {
            animator.Play("Idle", 0);
        }
    }

    void FixedUpdate()
    {
        if (checkInput("left"))
        {
            Vector3 movement = new Vector3(-1, 0, 0);
            transform.Translate(movement * moveSpeed * Time.deltaTime);
            renderer.flipX = true;
        }
        if (checkInput("right"))
        {
            Vector3 movement = new Vector3(1, 0, 0);
            transform.Translate(movement * moveSpeed * Time.deltaTime);
            renderer.flipX = false;
        }
        if (checkInput("jump") && IsGrounded())
        {
            Vector3 movement = new Vector3(0, 1 * jumpForce, 0);
            rigidbody.velocity = movement;
        }
        if (checkInput("reset"))
        {
            transform.position = startingPosition;
            transform.rotation = startingRotation;
            rigidbody.angularVelocity = 0;
            rigidbody.velocity = new Vector3(0, 0, 0);
        }
    }

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
