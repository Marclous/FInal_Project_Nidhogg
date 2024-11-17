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

    public float minJumpForce = 5f; // Minimum jump force
    public float maxJumpForce = 10f; // Maximum jump force
    private float jumpChargeTime = 0f; // How long the space bar has been held
    public float maxChargeTime = 1f; // Max time for charging the jump
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Move();
        HandleJumpInput();
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

    void HandleJumpInput()
    {
        if (Input.GetKey(KeyCode.Space) && isGrounded) // While space is held
        {
            jumpChargeTime += Time.deltaTime;
            jumpChargeTime = Mathf.Clamp(jumpChargeTime, 0, maxChargeTime); // Clamp the charge time
        }

        if (Input.GetKeyUp(KeyCode.Space) && isGrounded) // On space release
        {
            Jump();
            jumpChargeTime = 0f; // Reset jump charge time
        }
    }

    void Jump()
    {
        float jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, jumpChargeTime / maxChargeTime);
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
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
