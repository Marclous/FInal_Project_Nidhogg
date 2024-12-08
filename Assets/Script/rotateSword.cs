using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateSword : MonoBehaviour
{
    public GameObject sword;
    // Start is called before the first frame update
    void Start()
    {
        Collider2D swordCollider = sword.GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
