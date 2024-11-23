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

    private string playerTag; // Tag to determine the player

    //Duck variables
    public Transform crouchCheckPoint; 
    public float crouchCheckRadius = 0.5f; // 检测半径
    public LayerMask swordLayer;

    private bool isCrouching = false; // 是否处于下蹲状态
    private Sword currentSword; // 当前持有的剑

    void Start()
    {
        vecGravity = new Vector2(0, -Physics2D.gravity.y);
        rb = GetComponent<Rigidbody2D>();

        // Retrieve the tag of the player to distinguish controls
        playerTag = gameObject.tag;
    }

    void Update()
    {
        Move();
        HandleJumpAndDash();
        HandleCrouch();
    }

    void Move()
    {
        float xposition = 0f;

        // Use different inputs based on the player's tag
        if (playerTag == "Player 1")
        {
            xposition = Input.GetKey(KeyCode.A) ? -1 : (Input.GetKey(KeyCode.D) ? 1 : 0);

        }
        else if (playerTag == "Player 2")
        {
            xposition = Input.GetKey(KeyCode.LeftArrow) ? -1 : (Input.GetKey(KeyCode.RightArrow) ? 1 : 0);
        }

        if (Mathf.Abs(xposition) > 0.1f) // Player is pressing a movement key
        {
            isMoving = true;
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdTime)
            {
                currentSpeed = speed + speedIncrease;
            }
             // Flip the sprite to face the moving direction
            if (xposition > 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (xposition < 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        else // Player stopped moving
        {
            isMoving = false;
            holdTimer = 0f;
            currentSpeed = speed; // Reset to base speed
            if (playerTag == "Player 1")
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z); // Face right
            }
            else if (playerTag == "Player 2")
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z); // Face left
            }
        }

        rb.velocity = new Vector2(xposition * currentSpeed, rb.velocity.y);
    }

    void HandleJumpAndDash()
    {
        // Jump input based on player tag
        if (playerTag == "Player 1" && Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            Jump();
        }
        else if (playerTag == "Player 2" && Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
        {
            Jump();
        }

        // Dash input based on player tag
        if (playerTag == "Player 1" && Input.GetKeyDown(KeyCode.LeftShift))
        {
            Dash();
        }
        else if (playerTag == "Player 2" && Input.GetKeyDown(KeyCode.RightShift))
        {
            Dash();
        }

        if (rb.velocity.y > 0 && isJumping)
        {
            jumpCounter += Time.deltaTime;
            if (jumpCounter > jumpTime) isJumping = false;

            rb.velocity += vecGravity * jumpMultiplier * Time.deltaTime;
        }

        if ((playerTag == "Player 1" && Input.GetKeyUp(KeyCode.W)) ||
            (playerTag == "Player 2" && Input.GetKeyUp(KeyCode.UpArrow)))
        {
            isJumping = false;
        }

        if (rb.velocity.y < 0)
        {
            rb.velocity -= vecGravity * fallMultiplier * Time.deltaTime;
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isJumping = true;
        jumpCounter = 0;
    }


    void HandleCrouch()
    {
        // check if press down button
        if ((playerTag == "Player 1" && Input.GetKey(KeyCode.S)) ||
            (playerTag == "Player 2" && Input.GetKey(KeyCode.DownArrow)))
        {
            isCrouching = true;
            

            // Check if there is sword on the ground
            Collider2D swordCollider = Physics2D.OverlapCircle(crouchCheckPoint.position, crouchCheckRadius, swordLayer);
            if (swordCollider != null && swordCollider != this)
            {
                Debug.Log("yes");
                Sword sword = swordCollider.GetComponent<Sword>();
                if (sword != null && currentSword == null) // Check sword and owner status
                {
                    Debug.Log("ready to pick up");
                    PickUpSword(sword);
                }
            }
        }
        else
        {
            isCrouching = false;
        }
    }
    void PickUpSword(Sword sword)
    {
        currentSword = sword;
        sword.PickUp(gameObject); // 更新剑的持有者
        Debug.Log($"{playerTag} picked up the sword!");
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
