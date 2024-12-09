using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private float timeBtwAttack;
    public float startTimeBtwAttack;

    public Transform attackPos;
    public LayerMask whatIsEnemies;
    public float attackRange;
    public Animator anim;

    private PlayerMovement playerMovement;

    // Update is called once per frame
    void Start()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (timeBtwAttack <= 0)
        {
            if (((Input.GetKey(KeyCode.F) && playerMovement.playerTag == "Player 1") || (Input.GetKey(KeyCode.M) && playerMovement.playerTag == "Player 1")) && playerMovement.currentSword == null)
            {
                Debug.Log(playerMovement.playerTag+" is punching");
                anim.SetBool("isPunching", true);
                StartCoroutine(PunchCD());
                Collider2D[] enemyPlayer = Physics2D.OverlapCircleAll(attackPos.position, attackRange, whatIsEnemies);
                for (int i = 0; i < enemyPlayer.Length; i++)
                {
                    PlayerMovement enemy = enemyPlayer[i].GetComponent<PlayerMovement>();
                    if (enemy.allowMove == false)
                    {
                        Destroy(enemyPlayer[i].gameObject);
                    }
                    else
                    {
                        enemyPlayer[i].GetComponent<PlayerMovement>().StartParalyze();
                    }

                }
                
            }
            timeBtwAttack = startTimeBtwAttack;
        }
        else
        {
            timeBtwAttack -= Time.deltaTime;
        }

    }
    



    private IEnumerator PunchCD()
    {
        yield return new WaitForSeconds(startTimeBtwAttack); // 等待指定时间
        anim.SetBool("isPunching", false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position, attackRange);
    }
}
