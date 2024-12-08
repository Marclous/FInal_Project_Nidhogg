using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float dashForce = 10f;
    public float dashSpeed = 20f;
    public float dashAngle = 30f; // Dash angle in degrees
    private Vector2 dashDirection; // Direction of the dash
    public float dashDuration = 0.2f; // Duration of the dash
    private float dashStartTime; // When the dash started
    private bool isDashing = false;
    private Rigidbody2D rb;
    public bool isGrounded;
    
    [SerializeField] private float speed;
    public float speedIncrease = 2f; // Additional speed after holding the key
    public float holdTime = 1f; // Time required to hold before speed increases
    private float currentSpeed; // Tracks the current movement speed
    private float holdTimer = 0f; // Tracks how long the key has been held
    private bool isMoving = false;
    public bool isRunning = false;

    public int jumpForce;
    public float fallMultiplier;
    public float jumpTime;
    public float jumpMultiplier;

    Vector2 vecGravity;
    bool isJumping;
    float jumpCounter;

    public string playerTag; // Tag to determine the player

    //Duck variables
    public Transform crouchCheckPoint; 
    public float crouchCheckRadius = 0.5f; 
    public LayerMask swordLayer;

    private bool isCrouching = false; 
    public Sword currentSword; 
    
    public Transform opponent;// Get opponent
    public string opponentTag;
    public bool isDefending = false;

    void Start()
    {
        vecGravity = new Vector2(0, -Physics2D.gravity.y);
        rb = GetComponent<Rigidbody2D>();

        // Retrieve the tag of the player to distinguish controls
        playerTag = gameObject.tag;
    }

    void Update()
    {
       if(playerTag != null){
       if(playerTag == "Player 1"){
            opponentTag =  "Player 2";
        }
       if(playerTag == "Player 2"){
            opponentTag =  "Player 1";
        }


        // 检测场上是否有对手
        DetectOpponent();

        // 如果存在对手，则调整朝向
        if (opponent != null)
        {
            FaceOpponent();
        }

       }


        if(isRunning) {
            Physics2D.IgnoreLayerCollision(6,6);
        }else if(!isRunning){
            Physics2D.IgnoreLayerCollision(6,6,false);
        }

        if (isDashing)
        {
            PerformDash();
        }

        Move();
        HandleJumpAndDash();
        HandleCrouch();


    }

    void Move()
    {
        float xposition = 0f;

        // Use different inputs based on the player's tag
        if (playerTag == "Player 1" && isDefending == false)
        {
            xposition = Input.GetKey(KeyCode.A) ? -1 : (Input.GetKey(KeyCode.D) ? 1 : 0);
            

        }
        else if (playerTag == "Player 2" && isDefending == false)
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
                isRunning = true;

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
            isRunning = false;
            holdTimer = 0f;
            currentSpeed = speed; // Reset to base speed

            // if (opponent.position.x < transform.position.x)
            // {
            //     Debug.Log(opponent.position.x);
            //     Debug.Log(opponent.gameObject.name+" on the Left of" + gameObject.name);
            //     transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z); // Face left
            // }
            // else if (opponent.position.x > transform.position.x)
            // {
            //     Debug.Log(opponent.position.x);
            //     Debug.Log(opponent.gameObject.name+" on the right of" + gameObject.name);
            //     transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z); // Face right
            // }
        }

        rb.velocity = new Vector2(xposition * currentSpeed, rb.velocity.y);
    }

    void HandleJumpAndDash()
    {
        // Jump input based on player tag
        if (playerTag == "Player 1" && Input.GetKeyDown(KeyCode.G) && isGrounded)
        {
            Debug.Log("Jump Sucess");
            Jump();
            
        }
        else if (playerTag == "Player 2" && Input.GetKeyDown(KeyCode.N) && isGrounded)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.F) && !isGrounded && !isDashing && playerTag == "Player 1")
        {
            StartDash();
        }else if (Input.GetKeyDown(KeyCode.M) && !isGrounded && !isDashing && playerTag == "Player 2")
        {
            StartDash();
        }

        if (rb.velocity.y > 0 && isJumping)
        {
            jumpCounter += Time.deltaTime;
            if (jumpCounter > jumpTime) isJumping = false;

            rb.velocity += vecGravity * jumpMultiplier * Time.deltaTime;
        }

        // if ((playerTag == "Player 1" && Input.GetKeyUp(KeyCode.W)) ||
        //     (playerTag == "Player 2" && Input.GetKeyUp(KeyCode.UpArrow)))
        // {
        //     isJumping = false;
        // }

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


    private void StartDash()
    {
        isDashing = true;
        dashStartTime = Time.time;

        // Calculate dash direction
        float horizontalDirection = Mathf.Sign(rb.velocity.x);
        float angleInRadians = dashAngle * Mathf.Deg2Rad;
        dashDirection = new Vector2(horizontalDirection * Mathf.Cos(angleInRadians), -Mathf.Sin(angleInRadians)).normalized;
    }

    private void PerformDash()
    {
        if (Time.time - dashStartTime <= dashDuration)
        {
            // Move the player using MovePosition
            Vector2 newPosition = rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
        else
        {
            EndDash();
        }
    }
    private void EndDash()
    {
        isDashing = false;
       
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        if(isDashing == true && collision.gameObject.CompareTag("Player 1") && gameObject.CompareTag("Player 2")) {
            

        }
        if(isDashing == true && collision.gameObject.CompareTag("Player 2") && gameObject.CompareTag("Player 1")) {
            
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void DetectOpponent()
    {
        GameObject opponentObject = GameObject.FindWithTag(opponentTag); // 查找带有指定标签的对象
        if (opponentObject != null)
        {
            opponent = opponentObject.transform; // 如果找到对手，则更新引用
        }
        else
        {
            opponent = null; // 如果没有找到对手，则清空引用
        }
    }

    // 面向对手
    void FaceOpponent()
    {
        Vector3 direction = (opponent.position - transform.position).normalized;

        // 对于 2D 游戏，翻转玩家朝向
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, transform.localScale.y, 1); // 面向左侧
        }
        else
        {
            transform.localScale = new Vector3(1, transform.localScale.y, 1); // 面向右侧
        }

        // 对于 3D 游戏，使用旋转朝向对手
        // Uncomment the following if working in 3D
        /*
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        */
    }
}
