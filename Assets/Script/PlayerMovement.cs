using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float dashForce = 10f;
    private Rigidbody2D rb;
    private bool isGrounded;
    [SerializeField] private float speed;
    public float speedIncrease = 2f; // Additional speed after holding the key
    public float holdTime = 1f; // Time required to hold before speed increases
    private float currentSpeed; // Tracks the current movement speed
    private float holdTimer = 0f; // Tracks how long the key has been held
    private bool isMoving = false;


    public int jumpForce;
    public float fallMultiplier;
    public float jumpTime;
    public float jumpMultiplier;

    Vector2 vecGravity;
    bool isJumping;
    float jumpCounter;
    
    
    void Start()
    {
        vecGravity = new Vector2(0, -Physics2D.gravity.y);
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Move();
        if(Input.GetKeyDown(KeyCode.Space) && isGrounded) {
            Jump();
        }
        if(rb.velocity.y > 0 && isJumping) {
            jumpCounter += Time.deltaTime;
            if(jumpCounter > jumpTime) isJumping = false;

            rb.velocity += vecGravity * jumpMultiplier * Time.deltaTime;
        }
        if(Input.GetKeyUp(KeyCode.Space)) {
            isJumping = false;
        }
        if(rb.velocity.y < 0) {
            rb.velocity -= vecGravity * fallMultiplier * Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift)) // Use Left Shift for a quick dash
        {
            Dash();
        }
    }

    void Move()
    {
        float xposition = Input.GetAxis("Horizontal");

        if (Mathf.Abs(xposition) > 0.1f) // Player is pressing a movement key
        {
            isMoving = true;
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdTime)
            {
                currentSpeed = speed + speedIncrease;
            }
        }
        else // Player stopped moving
        {
            isMoving = false;
            holdTimer = 0f;
            currentSpeed = speed; // Reset to base speed
        }

        rb.velocity = new Vector2(xposition * currentSpeed, rb.velocity.y);

    }


    void Jump()
    {
        
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isJumping = true;
        jumpCounter = 0;
    }
    void Dash()
    {
        rb.AddForce(new Vector2(transform.localScale.x * dashForce, 0), ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
