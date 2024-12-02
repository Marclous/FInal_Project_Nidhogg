using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public enum SwordState { Held, Dropped, Thrown } //Three different states of SWORD
    public SwordState currentState = SwordState.Held;
    public Vector3 padding;

    [SerializeField] private float throwSpeed = 10f; 

    public GameObject holder; // who hold the SWORD now
    private Rigidbody2D rigidSword;
    private BoxCollider2D boxCollider2D;


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
            Physics2D.IgnoreLayerCollision(3, 6);

        }



        if (currentState == SwordState.Held && holder != null)
        {
            FollowHolder();
            
        }
    }

    // follow the holder
    private void FollowHolder()
    {

    //   if (holder != null)
    //{
    //    // Determine the facing direction based on the holder's localScale.x
    //    float direction = Mathf.Sign(holder.transform.localScale.x); // +1 for right, -1 for left

    //    // Update the sword's position relative to the holder's facing direction
    //    transform.position = holder.transform.position + new Vector3(padding.x * direction, padding.y, padding.z);

    //    // Update the sword's rotation to match the holder's rotation
    //    transform.rotation = holder.transform.rotation;

    //    // Flip the sword if needed to align with the holder's facing direction
    //    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);
    //}
        if (holder != null)
        {
            // Determine the facing direction based on the holder's localScale.x
            float direction = Mathf.Sign(holder.transform.localScale.x); // +1 for right, -1 for left

            // Calculate the target position relative to the holder
            Vector3 targetPosition = holder.transform.position + new Vector3(padding.x * direction, padding.y, padding.z);

            // 暂时禁用玩家与剑之间的碰撞
            Physics2D.IgnoreCollision(holder.GetComponent<Collider2D>(), GetComponent<Collider2D>(), true);

            // Smoothly move the sword to the target position using MovePosition
            //rigidSword.MovePosition(Vector3.Lerp(transform.position, targetPosition, 0.5f)); // Adjust 0.8 for smoothness
            // Update the sword's rotation to match the holder's rotation
            //transform.rotation = Quaternion.Lerp(transform.rotation, holder.transform.rotation, 0.5f);
            rigidSword.MovePosition(targetPosition);

            // Flip the sword if needed to align with the holder's facing direction
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);
            // 重新启用玩家与剑之间的碰撞
            // 启动协程，在下一帧重新启用碰撞
            StartCoroutine(ReEnableCollisionWithHolder());

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
        if (collision.gameObject.CompareTag("sword"))
        {
            Debug.Log($"Sword collided with {collision.gameObject.name}");
        }

    }

}
