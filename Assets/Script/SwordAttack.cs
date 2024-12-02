using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : Sword
{
    public Transform highPosition;
    public Transform midPosition;
    public Transform lowPosition;

    private Transform currentSwordPosition;
    // Start is called before the first frame update

    public float thrustSpeed = 10f;
    private bool isAttacking = false;

    private Vector2 originalPosition;

    void Start()
    {
        currentSwordPosition = midPosition;
        
    }

    void Update()
    {
        //originalPosition = HolderPosition;
        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking) // Attack
        {
            isAttacking = true;
            StartCoroutine(Thrust());
        }
    }

    private IEnumerator Thrust()
    {
        float elapsedTime = 0f;
        float duration = 0.2f; // Attack duration

        Vector3 targetPosition = HolderPosition + transform.right * 1.5f;

        while (elapsedTime < duration)
        {
            transform.localPosition = Vector3.Lerp(HolderPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        isAttacking = false;
    }

}
