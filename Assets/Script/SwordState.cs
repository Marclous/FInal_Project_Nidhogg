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
    }

    private void Update()
    {
        Physics2D.IgnoreLayerCollision(3,6);
        if (currentState == SwordState.Held && holder != null)
        {
            FollowHolder();
            
        }
    }

    // follow the holder
    private void FollowHolder()
    {

       if (holder != null)
    {
        // Determine the facing direction based on the holder's localScale.x
        float direction = Mathf.Sign(holder.transform.localScale.x); // +1 for right, -1 for left

        // Update the sword's position relative to the holder's facing direction
        transform.position = holder.transform.position + new Vector3(padding.x * direction, padding.y, padding.z);

        // Update the sword's rotation to match the holder's rotation
        transform.rotation = holder.transform.rotation;

        // Flip the sword if needed to align with the holder's facing direction
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);
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
        
        rigidSword.isKinematic = false; // 开启物理效果
    }

    // Throw
    public void Throw(Vector2 direction)
    {
        currentState = SwordState.Thrown;
        holder = null;

        rigidSword.isKinematic = false; // 开启物理效果
        rigidSword.velocity = direction.normalized * throwSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == SwordState.Thrown)
        {
            // If enemy touch the sword
            Debug.Log($"Sword hit {collision.gameObject.name}");
            rigidSword.velocity = Vector2.zero;
            currentState = SwordState.Dropped;
        }
    }
}
