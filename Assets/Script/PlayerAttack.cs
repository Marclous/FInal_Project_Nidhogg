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

    // Update is called once per frame
    void Update()
    {
        if(timeBtwAttack <= 0) {
            if(Input.GetKey(KeyCode.F)) {
                Collider2D[] enemyPlayer = Physics2D.OverlapCircleAll(attackPos.position, attackRange, whatIsEnemies);
                for(int i = 0 ; i < enemyPlayer.Length ; i++) {
                    PlayerMovement enemy = enemyPlayer[i].GetComponent<PlayerMovement>();
                    if(enemy.allowMove == false) {
                        Destroy(enemyPlayer[i].gameObject);
                    }else{
                        enemyPlayer[i].GetComponent<PlayerMovement>().StartParalyze();
                    }
                    
                }
            }
            timeBtwAttack = startTimeBtwAttack;
        } else {
            timeBtwAttack -= Time.deltaTime;
        }

    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position,attackRange);
    }
}
