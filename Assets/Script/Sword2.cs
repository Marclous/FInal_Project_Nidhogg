using System;
using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

public class Sword : MonoBehaviour
{
    // 剑的状态枚举
    public enum SwordState { Held, Dropped, Thrown,Defend }
    public SwordState currentState = SwordState.Dropped;

    // 组件引用
    private Rigidbody2D rigidSword;
    private Collider2D swordCollider;
    private Rigidbody2D rigidHolder;

    // 持有相关
    public GameObject holder; // 当前持有者
    public Vector3 padding;   // 剑相对于持有者的位置偏移
    public Vector3[] PositionOffsets =
    {
        new Vector3(0.5f, 1f, 0), // 高
        new Vector3(0.5f, 0.5f, 0), // 中
        new Vector3(0.5f, 0f, 0) // 低
    };
    private int positionIndex = 1; // 当前位置索引 (中)
    private Vector3 currentOffset; // 当前偏移量
    public float forceAmount = 10f;
    public float moveDistance = 1f; // Distance to move backward
    public float moveSpeed = 5f;  // Speed of the movement

    // 投掷相关
    [SerializeField] private float throwSpeed = 10f;
    private bool isAiming = false;
    public Vector3 aimOffset = new Vector3(-1f, 1f, 0);
    public GameObject previousHolder; // Save the current holder

    // 动作相关
    private bool isAttacking = false;
    private PlayerMovement holderScript;
    [SerializeField] private float thrustDuration = 0.2f;
    private bool isDefending = false;
    public float frontAngle = 115f;
    public float backAngle = -70f;
    public bool moved;//玩家是否上下调整了剑的位置
    private Coroutine resetMovedCoroutine; // 用于跟踪当前重置协程


    //xiangji

    public Camera cam;
    public new DynamicCamera camera;

    private void Awake()
    {
        camera = cam.GetComponent<DynamicCamera>();
        rigidSword = GetComponent<Rigidbody2D>();
        swordCollider = GetComponent<Collider2D>();
        
        currentOffset = PositionOffsets[positionIndex];
    }

    private void Update()
    {
        
        switch (currentState)
        {
            case SwordState.Held:
                HandleHeldState();
                break;
            case SwordState.Dropped:
                HandleDroppedState();
                break;
            case SwordState.Thrown:
                HandleThrownState();
                break;
            case SwordState.Defend:
                HandleDefendState();
                break;
            
        }
    }

    /// <summary>
    /// 处理持有状态的逻辑
    /// </summary>
    private void HandleHeldState()
    {
        if (holder == null) 
        {
            currentState = SwordState.Dropped;
            return;
        }
        

        FollowHolder();
        rigidHolder = holder.GetComponent<Rigidbody2D>();
        holderScript = holder.GetComponent<PlayerMovement>();
        // 切换瞄准状态
        if (Input.GetKeyDown(KeyCode.W) && holder.CompareTag("Player 1") && positionIndex == 0)
        {
            isAiming = true;
        }
        else if (Input.GetKeyUp(KeyCode.S) && holder.CompareTag("Player 1") && isAiming == true)
        {
            isAiming = false;
            // 恢复剑的默认旋转
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }else if (Input.GetKeyDown(KeyCode.UpArrow) && holder.CompareTag("Player 2") && positionIndex == 0)
        {
            isAiming = true;
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow) && holder.CompareTag("Player 2") && isAiming == true)
        {
            isAiming = false;
            // 恢复剑的默认旋转
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        
         if (Input.GetKeyDown(KeyCode.V) && holder.CompareTag("Player 1") && isAiming == false)
        {
            StartDefending();
            
        }

        //if (isDefending)
        //{
        //    HandleDefendingRotation();

        //}
        if (isAiming)
        {
            HandleAimingRotation();
            if (Input.GetKeyDown(KeyCode.F) && holder.CompareTag("Player 1"))
            {
                Throw(new Vector2(Mathf.Sign(holder.transform.localScale.x), 0));
                isAiming = false;
            }else if (Input.GetKeyDown(KeyCode.M) && holder.CompareTag("Player 2"))
            {
                Throw(new Vector2(Mathf.Sign(holder.transform.localScale.x), 0));
                isAiming = false;
            }
        }
        else
        {
            HandlePositionAdjustment();
            // 攻击操作
            if (Input.GetKeyDown(KeyCode.F) && !isAttacking && holder.CompareTag("Player 1") && !isAiming && holderScript.isGrounded != false)
            {
                Debug.Log(this.name + "戳了");
                StartCoroutine(Thrust());
            }
            else if (Input.GetKeyDown(KeyCode.M) && !isAttacking && holder.CompareTag("Player 2") && !isAiming && holderScript.isGrounded != false)
            {
                Debug.Log(this.name + "戳了");
                StartCoroutine(Thrust());
            }
        }

        
    }

    private void HandleDefendState()
    {
        HandleDefendingRotation();
        DenfenseKnock();
    }
    private void StartDefending() {
        currentState = SwordState.Defend;
        holderScript.isDefending = true;
        

        StartCoroutine(ResetDefense());
    }
    private void HandleDefendingRotation()
    {
        float direction = Mathf.Sign(holder.transform.localScale.x);
        
        if(direction < 0) {
            transform.rotation = Quaternion.Euler(0, 0, backAngle * direction);
            aimOffset = new Vector3(-1f,1f, 0);
            rigidSword.MovePosition(holder.transform.position + aimOffset);
            //transform.position = holder.transform.position + aimOffset ;
        }else if(direction > 0) {
            transform.rotation = Quaternion.Euler(0, 0, frontAngle * direction);
            aimOffset = new Vector3(1f,1f, 0);
            rigidSword.MovePosition(holder.transform.position + aimOffset);
            //transform.position = holder.transform.position + aimOffset ;
        }
    }
    private IEnumerator ResetDefense()
    {
        yield return new WaitForSeconds(0.8f); // 等待指定时间
        currentState = SwordState.Held;
        holderScript.isDefending = false;
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    /// <summary>
    /// 处理掉落状态的逻辑
    /// </summary>
    private void HandleDroppedState()
    {
        // 开启物理效果并解锁 Z 轴旋转
        rigidSword.gravityScale = 1f;
        rigidSword.isKinematic = false;
        rigidSword.constraints = RigidbodyConstraints2D.None;
        swordCollider.enabled = true;
        Vector3 uplocation = new Vector3(0, 10f, 0);
        
        if(holder != null)
        {
            holder = null;
        }
        // 开启旋转逻辑
        StartCoroutine(RotateSwordWhileFalling());

        DisableNonGroundCollisions();
    }

    /// <summary>
    /// 处理投掷状态的逻辑
    /// </summary>
    private void HandleThrownState()
    {
        // 投掷过程中保持物理行为
        if (currentState == SwordState.Thrown)
        {
            Rigidbody2D previousRb = previousHolder.GetComponent<Rigidbody2D>();
            float horizontalDirection = Mathf.Sign(previousRb.velocity.x);
            rigidSword.velocity = new Vector2(horizontalDirection * Mathf.Sign(previousHolder.transform.localScale.x), 0) * throwSpeed;

        }
    }

    /// <summary>
    /// 跟随持有者
    /// </summary>
    private void FollowHolder()
    {
        
        float direction = Mathf.Sign(holder.transform.localScale.x); // +1 for right, -1 for left
        Vector3 targetPosition = holder.transform.position + new Vector3(padding.x * direction, padding.y, padding.z);
        //transform.position = targetPosition;
        rigidSword.MovePosition(targetPosition);
        // 禁用与持有者的碰撞
        Physics2D.IgnoreCollision(holder.GetComponent<Collider2D>(), swordCollider, true);

        if (holder.GetComponent<PlayerMovement>().isRunning)
        {
            SetCollisionEnabled(false);

            if (!isAiming)
            {
                float rotationAngle = 45f * direction; // 根据朝向设置旋转角度
                transform.position = holder.GetComponent<PlayerMovement>().transform.position;
                transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
            }
        }
        else 
        {
            SetCollisionEnabled(true);
            if (!isAiming)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }

        }
    }

    /// <summary>
    /// 调整剑的高、中、低位置
    /// </summary>
    private void HandlePositionAdjustment()
    {
        
        if (Input.GetKeyDown(KeyCode.W) && positionIndex > 0 && holder.CompareTag("Player 1"))
        {
            positionIndex--;
            moved = true;
            if(resetMovedCoroutine != null)
            {
                StopCoroutine(resetMovedCoroutine);
            }
            // 启动新的协程以延迟重置 moved
            resetMovedCoroutine = StartCoroutine(ResetMoved());
        }
        else if (Input.GetKeyDown(KeyCode.S) && positionIndex < PositionOffsets.Length - 1 && holder.CompareTag("Player 1"))
        {
            positionIndex++;
            moved = true;
            if (resetMovedCoroutine != null)
            {
                StopCoroutine(resetMovedCoroutine);
            }
            // 启动新的协程以延迟重置 moved
            resetMovedCoroutine = StartCoroutine(ResetMoved());
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && positionIndex < PositionOffsets.Length - 1 && holder.CompareTag("Player 2"))
        {
            positionIndex++;
            moved = true;
            if (resetMovedCoroutine != null)
            {
                StopCoroutine(resetMovedCoroutine);
            }
            // 启动新的协程以延迟重置 moved
            resetMovedCoroutine = StartCoroutine(ResetMoved());
        }
        else if(Input.GetKeyDown(KeyCode.UpArrow) && positionIndex > 0 && holder.CompareTag("Player 2"))
        {
            positionIndex--;
            moved = true;
            if (resetMovedCoroutine != null)
            {
                StopCoroutine(resetMovedCoroutine);
            }
            // 启动新的协程以延迟重置 moved
            resetMovedCoroutine = StartCoroutine(ResetMoved());
        }

        if (moved)
        {
            // 检测是否可以打掉另一把剑
            TryKnockOtherSword();
        }

        if (isDefending)
        {
            Debug.Log("hahaha");
            moved = false;
        }


        currentOffset = PositionOffsets[positionIndex];
        float facingDirection = holder.transform.localScale.x;
        Vector3 target = new Vector3(
            holder.transform.position.x + currentOffset.x * facingDirection,
            holder.transform.position.y + currentOffset.y,
            holder.transform.position.z + currentOffset.z
        );

        // Adjust the sword's position based on the current offset and facing direction
        rigidSword.MovePosition(target);

       
    }
    private IEnumerator ResetMoved()
    {
        yield return new WaitForSeconds(0.7f); // 等待半秒
        moved = false;
    }

    /// <summary>
    /// 处理瞄准时的旋转
    /// </summary>
    private void HandleAimingRotation()
    {
        float direction = Mathf.Sign(holder.transform.localScale.x);
        
        if(direction < 0) {
            transform.rotation = Quaternion.Euler(0, 0, 70f * direction);
            aimOffset = new Vector3(1f,1f, 0);
            rigidSword.MovePosition(holder.transform.position + aimOffset);
            //transform.position = holder.transform.position + aimOffset ;
        }else if(direction > 0) {
            transform.rotation = Quaternion.Euler(0, 0, -105f * direction);
            aimOffset = new Vector3(-1f,1f, 0);
            rigidSword.MovePosition(holder.transform.position + aimOffset);
            //transform.position = holder.transform.position + aimOffset ;
        }
        
    }

    /// <summary>
    /// 执行攻击动作
    /// </summary>
    private IEnumerator Thrust()
    {
        float elapsedTime = 0f;
        float duration = 0.2f; // Attack duration

        Vector3 targetPosition = transform.localPosition + new Vector3(holder.transform.localScale.x * 1.5f, 0, 0);

        while (elapsedTime < duration)
        {
            rigidSword.MovePosition(targetPosition);
            //transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }


        isAttacking = false;
    }
    public void PickUp(GameObject newHolder)
    {
        holder = newHolder;
        transform.position = holder.transform.position;
        transform.rotation = Quaternion.Euler(0, 0, 0); // 重置旋转
        //transform.rotation = holder.transform.rotation;
        
        // 锁定 Z 轴旋转
        rigidSword.constraints = RigidbodyConstraints2D.FreezeRotation;

        currentState = SwordState.Held;
        // 确保与所有对象（除了当前持有者）恢复正常碰撞
        Collider2D swordCollider = GetComponent<Collider2D>();
        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();

        foreach (Collider2D col in allColliders)
        {
            // 忽略与持有者的碰撞，恢复与其他对象（包括敌人）的碰撞
            if (col.gameObject != holder)
            {
                Physics2D.IgnoreCollision(swordCollider, col, false);
            }
            else
            {
                Physics2D.IgnoreCollision(swordCollider, col, true);
            }
        }
        Debug.Log($"{holder.name} picked up the sword!");
    }

    /// <summary>
    /// 投掷剑
    /// </summary>
    private void Throw(Vector2 direction)
    {
        previousHolder = holder; // Save the current holder
        currentState = SwordState.Thrown;
        // 清除玩家的 currentSword
        PlayerMovement playerMovement = holder.GetComponent<PlayerMovement>();
        playerMovement.currentSword = null; // 清除当前剑引用
        
        holder = null;

        rigidSword.gravityScale = 0f; // 禁用重力
        rigidSword.constraints = RigidbodyConstraints2D.None; // 解锁旋转
        rigidSword.isKinematic = false;
        rigidSword.velocity = direction * throwSpeed;
        

        if (previousHolder != null)
        {
            Debug.Log("成功忽略！");
            Physics2D.IgnoreCollision(previousHolder.GetComponent<Collider2D>(), swordCollider, true);
            StartCoroutine(ReenablePlayerCollisionAfterThrow(previousHolder));
        }
        // 确保与其他物体（例如敌人）正常碰撞
        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();
        foreach (var collider in allColliders)
        {
            if (collider.CompareTag("sword"))
            {
                Physics2D.IgnoreCollision(swordCollider, collider, false);
            }
        }
        if (currentState == SwordState.Thrown && rigidSword.velocity.magnitude <= 0)
        {
            Vector2 padding = new Vector2(0,1);
            rigidSword.velocity = ( padding  ) * throwSpeed;
            Debug.Log("Correcting sword velocity to maintain throwing motion.");
        }
        // 让剑在空中旋转
        StartCoroutine(SpinSwordInAir());

    }

    /// <summary>
    /// 在投掷后恢复与玩家的碰撞
    /// </summary>
    private IEnumerator ReenablePlayerCollisionAfterThrow(GameObject previousHolder)
    {
        yield return new WaitForSeconds(0.5f);
        if (previousHolder != null)
        {
            Physics2D.IgnoreCollision(previousHolder.GetComponent<Collider2D>(), swordCollider, false);
        }
        // 让剑在空中旋转
        //StartCoroutine(SpinSwordInAir());
    }

    //扔的时候旋转
    private IEnumerator SpinSwordInAir()
    {
        while (currentState == SwordState.Thrown)
        {
            transform.Rotate(0, 0, 360 * Time.deltaTime); // 每秒旋转360度
            yield return null;
        }
    }
    //掉落的时候旋转
    private IEnumerator RotateSwordWhileFalling()
    {
        while (!IsGrounded()) // 检查是否落地
        {
            transform.Rotate(0, 0, 10 * Time.deltaTime); // 每秒旋转360度
            yield return null;
        }

        // 落地后停止旋转
        rigidSword.angularVelocity = 0f;
        transform.rotation = Quaternion.Euler(0, 0, 90); // 复位旋转
    }


    /// <summary>
    /// 使剑仅与地面碰撞
    /// </summary>
    private void DisableNonGroundCollisions()
    {
        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();
        foreach (var col in allColliders)
        {
            if (!col.CompareTag("Ground") && col != swordCollider)
            {
                Physics2D.IgnoreCollision(swordCollider, col, true);
            }
        }
    }
    private IEnumerator MoveToPosition(Rigidbody2D rb, Vector2 targetPosition, float speed)
    {
        while ((targetPosition - rb.position).sqrMagnitude > 0.01f)
        {
            rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, speed * Time.deltaTime));
            yield return null;
        }
    }

    
    //除你武器部分
    private void TryKnockOtherSword()
    {
        // 获取当前剑的碰撞范围内的所有碰撞体
        Vector2 detectionSize = new Vector2(swordCollider.bounds.size.x, swordCollider.bounds.size.y * 2);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, detectionSize, 0);

        Debug.Log(this.name + "尝试判断上下是否有剑");

        foreach (var collider in colliders)
        {
            Sword otherSword = collider.GetComponent<Sword>();

            if (otherSword != null && otherSword != this)
            {
                // 检查上下位置关系
                float yDifference = transform.position.y - otherSword.transform.position.y;
                Debug.Log(this.name + "正在检查上下关系");
                if (yDifference > 0.1f) // 当前剑在上方并向下调整
                {
                    Debug.Log("Knocked the other sword from above!");

                    DetachOtherSword(otherSword); // 调用解绑逻辑
                    moved = false;
                }
                else if (yDifference < -0.1f) // 当前剑在下方并向上调整
                {

                    Debug.Log("Knocked the other sword from below!");
                    DetachOtherSword(otherSword); // 调用解绑逻辑
                    moved = false;
                }
                
            }
           
        }
       
    }
    private void DenfenseKnock()
    {
        // 获取当前剑的碰撞范围内的所有碰撞体
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, swordCollider.bounds.size, 0);
        Debug.Log("Try to defense down");

        foreach (var collider in colliders)
        {
            Sword otherSword = collider.GetComponent<Sword>();
            if (otherSword != null && otherSword != this)
            {

                float horizontalDirection = otherSword.transform.position.x - transform.position.x;

                // Normalize to determine the direction (-1 for left, 1 for right)
                float directionSign = Mathf.Sign(horizontalDirection);

                // Calculate target positions for both objects
                Vector2 thisTargetPosition = rigidHolder.position - new Vector2(directionSign * moveDistance, 0f);
                // Apply horizontal forces to both objects

                StartCoroutine(MoveToPosition(rigidHolder, thisTargetPosition, moveSpeed));
                DetachOtherSword(otherSword); // 调用解绑逻辑
                //Vector3 uplocation = new Vector3(0, 2f, 0);
               // otherSword.transform.localPosition = Vector3.Lerp(transform.localPosition, uplocation, 1f);
                //otherSword.rigidSword.MovePosition(uplocation);
            }
        }
    }


    public void DetachOtherSword(Sword otherSword)
    {
        if (otherSword.holder != null)
        {
            //otherSword.previousHolder = holder;
            // 获取持有者的 PlayerMovement 脚本
            PlayerMovement otherHolderScript = otherSword.holder.GetComponent<PlayerMovement>();

            if (otherHolderScript != null)
            {
                // 清除持有者对剑的引用
                otherHolderScript.currentSword = null;
            }

            // 清除剑对持有者的引用
            otherSword.holder = null;
        }

        // 更新剑的状态为 Dropped
        otherSword.currentState = SwordState.Dropped;
        // 确保剑恢复物理行为
        otherSword.rigidSword.isKinematic = false;
        otherSword.rigidSword.constraints = RigidbodyConstraints2D.None;

    }

    public void DetachSelfSword()
    {
        if (holder != null)
        {
            //otherSword.previousHolder = holder;
            // 获取持有者的 PlayerMovement 脚本
            PlayerMovement otherHolderScript = holder.GetComponent<PlayerMovement>();

            if (otherHolderScript != null)
            {
                // 清除持有者对剑的引用
                otherHolderScript.currentSword = null;
            }

            // 清除剑对持有者的引用
            holder = null;
        }

        // 更新剑的状态为 Dropped
        currentState = SwordState.Dropped;
        // 确保剑恢复物理行为
        rigidSword.isKinematic = false;
        rigidSword.constraints = RigidbodyConstraints2D.None;

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D otherRb = collision.rigidbody;


        if (currentState == SwordState.Thrown)
        {
            // 检查是否是当前投掷者
            if (previousHolder != null && collision.gameObject == previousHolder)
            {
                Debug.Log("Ignored collision with previous holder.");
                return; // 忽略与投掷者的碰撞
            }

            // 击中敌人
            if (collision.gameObject.CompareTag("Player 1") || collision.gameObject.CompareTag("Player 2"))
            {
                Debug.Log("Sword hit player: " + collision.gameObject.name);
                Destroy(collision.gameObject);
                camera.deathnum++;
                currentState = SwordState.Dropped;
            }

            // 击中地面
            if (collision.gameObject.CompareTag("Ground"))
            {
                Debug.Log("Sword hit the ground.");
                currentState = SwordState.Dropped;
            }

            if (collision.gameObject.CompareTag("sword"))
            {
                Debug.Log("Sword hit the sword.");
                currentState = SwordState.Dropped;
            }
        }


        if (currentState == SwordState.Held && collision.gameObject.CompareTag("sword") && !isDefending) 
        {
            Sword otherSword = collision.gameObject.GetComponent<Sword>();

            Debug.Log("Collided with other sword");
            float horizontalDirection = collision.transform.position.x - transform.position.x;
            
            // Normalize to determine the direction (-1 for left, 1 for right)
            float directionSign = Mathf.Sign(horizontalDirection);

            // Calculate target positions for both objects
            Vector2 thisTargetPosition = rigidHolder.position - new Vector2(directionSign * moveDistance, 0f);
            // Apply horizontal forces to both objects
            StartCoroutine(MoveToPosition(rigidHolder, thisTargetPosition, moveSpeed));
        }else if(currentState == SwordState.Held && collision.gameObject.CompareTag("sword") && isDefending) {
            Debug.Log("Enter defense judge");
            //DenfenseKnock();
            //这个逻辑写在其它部分了。。。
        } 
        if (holder != null)
        {
            if (!collision.gameObject.CompareTag(holder.tag) && collision.gameObject.layer == 6)
            {
                Debug.Log("Kill" + collision.gameObject.name);
                camera.deathnum++;
                Destroy(collision.gameObject);
            }

        }

    }

    private bool IsGrounded()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, swordCollider.bounds.size, 0);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Ground"))
            {
                return true;
            }
        }
        return false;
    }
    private void SetCollisionEnabled(bool isEnabled)
    {
        Collider2D[] colliders = GetComponents<Collider2D>(); // 获取该对象上的所有碰撞体
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = isEnabled; // 启用或禁用碰撞体
        }
    }


}
