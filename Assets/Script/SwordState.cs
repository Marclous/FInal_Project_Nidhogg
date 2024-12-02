using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private Vector2 originalPosition;


    private void Awake()
    {
        rigidSword = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        // Enable interpolation for smoother movement
        //rigidSword.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        if (currentState == SwordState.Dropped && holder == null)
        {
            Physics2D.IgnoreLayerCollision(3, 6,true);

        }

        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking && currentState == SwordState.Held && holder.tag == "Player 1") // Attack
        {
            isAttacking = true;
            StartCoroutine(Thrust());
        }

        if (Input.GetKeyDown(KeyCode.RightShift) && !isAttacking && currentState == SwordState.Held && holder.tag == "Player 2") // Attack
        {
            isAttacking = true;
            StartCoroutine(Thrust());
        }

        if (currentState == SwordState.Held && holder != null)
        {
            FollowHolder();
            Physics2D.IgnoreLayerCollision(3,6,false);
        }
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
                float rotationAngle = 45f * direction; // 根据朝向设置旋转角度
                transform.position = HolderPosition;
                transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
            }



            else
            {
                // enable collision
                SetCollisionEnabled(true);
                transform.rotation = Quaternion.Euler(0, 0, 0);

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
    private IEnumerator Thrust()
    {
        float elapsedTime = 0f;
        float duration = 0.2f; // Attack duration

        Vector3 targetPosition = HolderPosition + new Vector3(holder.transform.localScale.x * 1.5f,0,0);

        while (elapsedTime < duration)
        {
            transform.localPosition = Vector3.Lerp(HolderPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        
        isAttacking = false;
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
        currentState = SwordState.Thrown;
        holder = null;

        //rigidSword.isKinematic = false; // 开启物理效果
        //rigidSword.velocity = direction.normalized * throwSpeed;
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (currentState == SwordState.Thrown)
    //    {
    //        // If enemy touch the sword
    //        Debug.Log($"Sword hit {collision.gameObject.name}");
    //        rigidSword.velocity = Vector2.zero;
    //        currentState = SwordState.Dropped;
    //    }
    //}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(holder != null){
            if (!collision.gameObject.CompareTag(holder.tag) && collision.gameObject.layer == 6)
            {
                Debug.Log("Kill"+ collision.gameObject.name);
                Destroy(collision.gameObject);
            }

        }

    }

}
