using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Sword : MonoBehaviour
{
    public enum SwordState { Held, Dropped, Thrown } //Three different states of SWORD
    public SwordState currentState = SwordState.Held;
    public Vector3 padding;
    public Vector3 HolderPosition;
    [SerializeField] private float throwSpeed = 10f; 

    public GameObject holder; // who hold the SWORD now
    private Rigidbody2D rigidSword;
    private BoxCollider2D boxCollider2D;
    public float thrustSpeed = 10f;
    private bool isAttacking = false;
    public Vector3 highPositionOffset = new Vector3(0.5f, 1f, 0); // Offset for high position
    public Vector3 midPositionOffset = new Vector3(0.5f, 0.5f, 0); // Offset for mid position
    public Vector3 lowPositionOffset = new Vector3(0.5f, 0f, 0); // Offset for low position
    public Vector3[] PositionOffset = new [] {new Vector3(0.5f, 1f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(0.5f, 0f, 0)};
    private int position;
    private Vector3 currentOffset; // Current offset based on stance
    private Vector2 originalPosition;
    private Transform swordTransform;
    public Camera cam;
    public new DynamicCamera camera;


    private bool isAiming = false; // 是否处于瞄准投掷状态
    private Vector2 aimDirection = Vector2.right; // 初始投掷方向
    private Transform aimTarget; // 瞄准点的目标
    public Vector3 aimOffset = new Vector3(0.5f, 0.5f, 0); // 瞄准状态下的偏移量



    private void Awake()
    {
        camera = cam.GetComponent<DynamicCamera>();

        rigidSword = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        currentOffset = midPositionOffset; // Start with mid position
        // Enable interpolation for smoother movement
        //rigidSword.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        currentOffset = PositionOffset[position];
        if (holder != null && currentState == SwordState.Held)
        {
            if (Input.GetKeyDown(KeyCode.U)) // 进入瞄准投掷状态
            {
                isAiming = true;
                Debug.Log("Aiming...");
            }

            if (Input.GetKeyUp(KeyCode.U)) // 退出瞄准投掷状态
            {
                isAiming = false;
                Debug.Log("Stopped Aiming");

                // 恢复剑的默认旋转
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            if (isAiming)
            {
                HandleAimingRotation(); // 调用持续旋转逻辑

                // 允许按键 I 投掷
                if (Input.GetKeyDown(KeyCode.I))
                {
                    Throw(new Vector2(Mathf.Sign(holder.transform.localScale.x), 0)); // 使用玩家的朝向作为投掷方向
                }
            }
        }
        if (holder == null && currentState == SwordState.Dropped )
        {
            Physics2D.IgnoreLayerCollision(3, 6,true);

        }

        if (Input.GetKeyDown(KeyCode.F) && !isAttacking && currentState == SwordState.Held && holder.tag == "Player 1") // Attack
        {
            isAttacking = true;
            StartCoroutine(Thrust());
        }

        if (Input.GetKeyDown(KeyCode.M) && !isAttacking && currentState == SwordState.Held && holder.tag == "Player 2") // Attack
        {
            isAttacking = true;
            StartCoroutine(Thrust());
        }

        if (currentState == SwordState.Held && holder != null)
        {
            FollowHolder();
            Physics2D.IgnoreLayerCollision(3,6,false);
        }
        if (Input.GetKeyDown(KeyCode.W) && !isAttacking && currentState == SwordState.Held && holder.tag == "Player 1" && position>0)
        {
            position--;
        }
        else if (Input.GetKeyDown(KeyCode.S) && !isAttacking && currentState == SwordState.Held && holder.tag == "Player 1" && position<2)
        {
            position++;
        }
       
        if (Input.GetKeyDown(KeyCode.UpArrow) && !isAttacking && currentState == SwordState.Held && holder.tag == "Player 2" && position>0)
        {
            position--;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && !isAttacking && currentState == SwordState.Held && holder.tag == "Player 2"&& position<2)
        {
            position++;
        }

        if (holder != null) 
        {
            // Update sword position relative to player
            AdjustSwordPosition();
            Physics2D.IgnoreLayerCollision(3, 3, false);
        }
        if (holder == null)
        {
            SetCollisionEnabled(true);
            Physics2D.IgnoreLayerCollision(3, 3);
        }


    }

    void AdjustSwordPosition()
    {
        // Get the player's facing direction
        float facingDirection = holder.transform.localScale.x;
        Vector3 target = new Vector3(
            HolderPosition.x + currentOffset.x * facingDirection,
            HolderPosition.y + currentOffset.y,
            HolderPosition.z + currentOffset.z
        );

        // Adjust the sword's position based on the current offset and facing direction
        rigidSword.MovePosition(target);

        // Flip the sword's local rotation if needed (optional, for visual alignment)
        //swordTransform.localScale = new Vector3(facingDirection, 1, 1);
    }
    private IEnumerator Thrust()
    {
        float elapsedTime = 0f;
        float duration = 0.2f; // Attack duration

        Vector3 targetPosition = transform.localPosition + new Vector3(holder.transform.localScale.x * 1.5f,0,0);

        while (elapsedTime < duration)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        
        isAttacking = false;
    }
    // follow the holder
    private void FollowHolder()
    {

    
        if (holder != null)
        {
            // Determine the facing direction based on the holder's localScale.x
            float direction = Mathf.Sign(holder.transform.localScale.x); // +1 for right, -1 for left

            // Calculate the target position relative to the holder
            HolderPosition = holder.transform.position + new Vector3(padding.x * direction, padding.y, padding.z);


            if (holder.GetComponent<PlayerMovement>().isRunning) // 玩家快速移动中
            {
                // 禁用碰撞体
                SetCollisionEnabled(false);
                Physics2D.IgnoreCollision(holder.GetComponent<Collider2D>(), GetComponent<Collider2D>(), true);

                // 旋转剑


                if (!isAiming)
                {
                    float rotationAngle = 45f * direction; // 根据朝向设置旋转角度
                    transform.position = HolderPosition;
                    transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
                }

            }

            else
            {
                // enable collision
                SetCollisionEnabled(true);

                if (!isAiming)
                {
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                }
 

                // 暂时禁用玩家与剑之间的碰撞
                Physics2D.IgnoreCollision(holder.GetComponent<Collider2D>(), GetComponent<Collider2D>(), true);

                // Smoothly move the sword to the target position using MovePosition
                rigidSword.MovePosition(HolderPosition);
                // 重新启用玩家与剑之间的碰撞
                // 启动协程，在下一帧重新启用碰撞
                StartCoroutine(ReEnableCollisionWithHolder());
            }
            // Flip the sword if needed to align with the holder's facing direction
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);


        }
    }
    private IEnumerator ReEnableCollisionWithHolder()
    {
        // 等待物理更新完成
        yield return new WaitForFixedUpdate();

        if (holder != null)
        {
            // 重新启用玩家与剑之间的碰撞
            Physics2D.IgnoreCollision(holder.GetComponent<Collider2D>(), GetComponent<Collider2D>(), false);
        }
    }
    // Collision Enable & Disable
    private void SetCollisionEnabled(bool isEnabled)
    {
        Collider2D[] colliders = GetComponents<Collider2D>(); // 获取该对象上的所有碰撞体
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = isEnabled; // 启用或禁用碰撞体
        }
    }


    // pick up
    public void PickUp(GameObject newHolder)
    {
        holder = newHolder; 
        transform.position = holder.transform.position; 
        transform.rotation = holder.transform.rotation; 

        currentState = SwordState.Held;

        //rigidSword.velocity = Vector2.zero; // 停止物理运动
        //rigidSword.isKinematic = true; // 取消物理效果
    }
    private void HandleAimingRotation()
    {
        // 获取玩家的朝向
        float direction = Mathf.Sign(holder.transform.localScale.x);

        // 持续调整剑的旋转角度
        transform.rotation = Quaternion.Euler(0, 0, -105f * direction);
        // 更新剑的位置，在瞄准状态下更靠近玩家
        Vector3 adjustedAimOffset = new Vector3(aimOffset.x * direction, aimOffset.y, aimOffset.z);
        transform.position = holder.transform.position + adjustedAimOffset;

    }
    // Drop
    public void Drop()
    {
        currentState = SwordState.Dropped;
        holder = null;
        
        //rigidSword.isKinematic = false; // 开启物理效果
    }


    // Throw
    public void Throw(Vector2 direction)
    {
        
        if (currentState != SwordState.Held) return; // Ensure sword is currently held

        // Update state
        currentState = SwordState.Thrown;
        GameObject previousHolder = holder; // Save the current holder
        holder = null;

        // Enable physical motion
        rigidSword.isKinematic = false;
        rigidSword.velocity = direction.normalized * throwSpeed;

        if (previousHolder != null)
        {
            Physics2D.IgnoreCollision(previousHolder.GetComponent<Collider2D>(), boxCollider2D, true);
            StartCoroutine(ReenableCollisionWithPlayer(previousHolder));
        }
        // Start rotating the sword while it's in the air
        //StartCoroutine(SpinSwordInAir());

    }
    //临时方法，可在功能完善后删除
    private IEnumerator ReenableCollisionWithPlayer(GameObject previousHolder)
    {
        yield return new WaitForSeconds(0.2f); // Wait for a short moment
        if (previousHolder != null)
        {
            Physics2D.IgnoreCollision(previousHolder.GetComponent<Collider2D>(), boxCollider2D, false);
        }
    }




    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(holder != null){
            if (!collision.gameObject.CompareTag(holder.tag) && collision.gameObject.layer == 6)
            {
                Debug.Log("Kill"+ collision.gameObject.name);
                camera.deathnum++;
                Destroy(collision.gameObject);
            }

        }

        if(currentState == SwordState.Thrown)
        {
            if (collision.gameObject.layer == 6)
            {
                Debug.Log("Kill" + collision.gameObject.name);
                camera.deathnum++;
                Destroy(collision.gameObject);
            }else {
                currentState = SwordState.Dropped;
            }
        }

    }

}
