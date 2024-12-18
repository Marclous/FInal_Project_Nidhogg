﻿using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public AudioClip[] sounds = {};
    private AudioSource audioSource;
    public float dashForce = 10f;
    public float dashSpeed = 20f;
    public float dashAngle = 30f; // Dash angle in degrees
    private Vector2 dashDirection; // Direction of the dash
    public float dashDuration = 0.2f; // Duration of the dash
    private float dashStartTime; // When the dash started
    private bool isDashing = false;
    private Rigidbody2D rb;
    public bool isGrounded;
    public bool allowMove = true;
    public GameObject deathObject;
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

    public Animator animator;
    Vector2 vecGravity;
    bool isJumping;
    float jumpCounter;

    public string playerTag; // Tag to determine the player

    //Duck variables
    public Transform crouchCheckPoint; 
    public float crouchCheckRadius = 0.5f; 
    public LayerMask swordLayer;
    private new CapsuleCollider2D collider2D;

    private bool isCrouching = false; 
    public Sword currentSword; 
    
    public Transform opponent;// Get opponent
    public string opponentTag;
    public bool isDefending = false;
    private bool isPlaying;
    void Start()
    {
        vecGravity = new Vector2(0, -Physics2D.gravity.y);
        rb = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
        // Retrieve the tag of the player to distinguish control
        playerTag = gameObject.tag;
        animator = GetComponent<Animator>();
        
    }

    void Update()
    {
        if(currentSword != null) {
            animator.SetBool("hasSword", true);
            animator.SetInteger("SwordPos", currentSword.positionIndex);
        }else{
            animator.SetBool("hasSword", false);
        }
        if(currentSword != null) {
            
        }
        if(allowMove == false) {
            animator.Play("Stun");
            isPlaying = true;
        }
        if (isPlaying)
        {
            // 检测动画是否播放完成
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1f && !animator.IsInTransition(0))
            {
                // 恢复状态机逻辑
                animator.Rebind();
                animator.Update(0);
                isPlaying = false;
            }
        }
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
        if(Input.GetKey(KeyCode.Q) && playerTag == "Player 1") {
            StartParalyze();
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
        //TryPickUpSword();

    }
    private IEnumerator ResetToStateMachine()
    {
        // 等待一帧，确保 Play 动画执行
        yield return null;

        // 重置状态机逻辑
        animator.Rebind();
        animator.Update(0);
    }
    void Move()
    {
        float xposition = 0f;

        // Use different inputs based on the player's tag
        if (playerTag == "Player 1" && isDefending == false && allowMove == true)
        {
            xposition = Input.GetKey(KeyCode.A) ? -1 : (Input.GetKey(KeyCode.D) ? 1 : 0);
            

        }
        else if (playerTag == "Player 2" && isDefending == false && allowMove == true)
        {
            xposition = Input.GetKey(KeyCode.LeftArrow) ? -1 : (Input.GetKey(KeyCode.RightArrow) ? 1 : 0);
            
        }

        if (Mathf.Abs(xposition) > 0.1f) // Player is pressing a movement key
        {
            isMoving = true;
            holdTimer += Time.deltaTime;
            
            audioSource.Play();
            if (holdTimer >= holdTime)
            {
                currentSpeed = speed + speedIncrease;
                isRunning = true;
                animator.SetBool("isRunning", isRunning);

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
            animator.SetBool("isRunning", isRunning);
            holdTimer = 0f;
            currentSpeed = speed; // Reset to base speed
            audioSource.Stop();
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
        animator.SetFloat("xVelocity", Math.Abs(rb.velocity.x));
        animator.SetFloat("yVelocity", rb.velocity.y);
    }
    public void StartParalyze() {
        Debug.Log(playerTag + " Not Moving");
        allowMove = false;
        

        StartCoroutine(StopParalyze());
    }
    private IEnumerator StopParalyze()
    {
        yield return new WaitForSeconds(0.8f); // 等待指定时间
        allowMove = true;
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
            //animator.SetBool("isJumping", !isGrounded);
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
            if (jumpCounter > jumpTime) {
                isJumping = false;
                
            }
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
        animator.SetBool("isJumping", isJumping);
        jumpCounter = 0;
    }

    
    void HandleCrouch()
    {
        if (currentSword)
        {
            
        }
        
        // check if press down button
        if ((playerTag == "Player 1" && Input.GetKey(KeyCode.S))||
            (playerTag == "Player 2" && Input.GetKey(KeyCode.DownArrow)))
        {
            if (currentSword == null)
            {
                isCrouching = true;
                animator.SetBool("isCrouching", true);
                collider2D.size = new Vector2(0.25f, 0.4f);
                collider2D.offset = new Vector2(0f, -0.06f);
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
            else if (currentSword != null) 
            {
                Sword ss = currentSword.GetComponent<Sword>();
                
                if(ss.positionIndex == 2)
                {

                    Debug.Log("DUNDUN" + ss.positionIndex);
                    isCrouching = true;
                    animator.SetBool("isCrouching", true);
                    collider2D.size = new Vector2(0.25f, 0.4f);
                    collider2D.offset = new Vector2(0f, -0.06f);
                    
                }
            }
            
        }
        else
        {
            isCrouching = false;
            animator.SetBool("isCrouching", false);
            collider2D.size = new Vector2(0.25f, 0.55f);
            collider2D.offset = new Vector2(0f, 0f);
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
    private void TryPickUpSword()
    {
        // 检查当前是否没有持有剑
        if (currentSword == null)
        {
            // 在玩家范围内寻找所有的剑
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 2f); // 2f 为检测范围，可根据需求调整

            foreach (Collider2D collider in colliders)
            {
                Sword sword = collider.GetComponent<Sword>();

                // 检查是否找到掉落状态的剑，且不在地面上
                if (sword != null && sword.currentState == Sword.SwordState.Dropped && !IsGrounded(sword.gameObject))
                {
                    // 拾取剑
                    sword.PickUp(gameObject); // 绑定当前玩家为剑的持有者
                    currentSword = sword; // 设置为当前玩家的剑
                    Debug.Log($"{gameObject.name} picked up {sword.name}");
                    return; // 成功拾取后退出循环
                }
            }
        }
    }

    // 检查目标物体是否在地面上
    private bool IsGrounded(GameObject obj)
    {
        // 使用 Physics2D.Raycast 检测物体底部是否接触地面
        float extraHeight = 0.1f; // 检测范围的额外高度
        RaycastHit2D hit = Physics2D.Raycast(obj.transform.position, Vector2.down, extraHeight);
        return hit.collider != null && hit.collider.CompareTag("Ground");
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("isJumping", !isGrounded);
        }
        if(isDashing == true && collision.gameObject.CompareTag("Player 1") && gameObject.CompareTag("Player 2")) {
            
            Sword otherSword = collision.gameObject.GetComponent<PlayerMovement>().currentSword.GetComponent<Sword>();
            if(otherSword != null) {
                otherSword.DetachSelfSword();
            }

        }
        if(isDashing == true && collision.gameObject.CompareTag("Player 2") && gameObject.CompareTag("Player 1")) {
            Debug.Log("Drop sword");
            collision.gameObject.GetComponent<PlayerMovement>().StartParalyze();
            Sword otherSword = collision.gameObject.GetComponent<PlayerMovement>().currentSword.GetComponent<Sword>();
            
            
            if(otherSword != null) {
                otherSword.DetachSelfSword();
            }
            
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
            transform.localScale = new Vector3(-3, transform.localScale.y, 1); // 面向左侧
        }
        else
        {
            transform.localScale = new Vector3(3, transform.localScale.y, 1); // 面向右侧
        }

        // 对于 3D 游戏，使用旋转朝向对手
        // Uncomment the following if working in 3D
        /*
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        */
    }
}
