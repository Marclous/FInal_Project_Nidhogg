using UnityEngine;
using System.Collections;
using System.Net;

public class DynamicCamera : MonoBehaviour
{
    public Camera mainCamera; // Reference to the main camera
    public string player1Tag = "Player 1"; // Tag for player 1
    public string player2Tag = "Player 2"; // Tag for player 2

    public GameObject playerPrefab; // Player prefab for respawning
    public Vector3 spawnOffset = new Vector3(0, 0, 0); // Offset for respawning relative to the camera
    public float respawnCooldown = 3f; // Cooldown duration before respawning

    private bool isRespawning = false;

    private GameObject player1; // Cached reference for Player 1
    private GameObject player2; // Cached reference for Player 2
    private Vector3 remainingPlayerOffset; // Offset of the remaining player relative to the camera

    private bool isFixedMode = true; // Starts in Fixed Camera mode
    //public Sword swordstate;
    public int deathnum;

    void Start()
    {
       // GameObject swordObject = GameObject.FindWithTag("Sword"); // 假设 Sword 有 "Sword" 标签
        //if (swordObject != null)
        //{
        //    swordstate = swordObject.GetComponent<Sword>();
        //}
        // Ensure the camera is assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }


        // Cache initial player references
        player1 = GameObject.FindGameObjectWithTag(player1Tag);
        player2 = GameObject.FindGameObjectWithTag(player2Tag);

        // Initialize the relative offset for camera centering
        if (player1 != null)
        {
            remainingPlayerOffset = mainCamera.transform.position - player1.transform.position;
        }
    }

    void Update()
    {
        // Update player references
        player1 = GameObject.FindGameObjectWithTag(player1Tag);
        player2 = GameObject.FindGameObjectWithTag(player2Tag);

        if (isFixedMode)
        {
            FixedCamera(); // Fixed camera logic
            return;
        }


        if (player1 != null && player2 == null)
        {
            FollowRemainingPlayer(player1.transform);
          
        }
        else if (player2 != null && player1 == null)
        {
            FollowRemainingPlayer(player2.transform);
            
        }
        if (player1 != null && player2 != null)
        {
            CenterCameraOnBothPlayers(player1.transform, player2.transform);
        }

        // Both players dead
        if (player1 == null && player2 == null)
        {
            EnterFixedMode();
        }
    }

    void FollowRemainingPlayer(Transform playerTransform)
    {
        // Keep the camera at the same relative offset to the remaining player
        mainCamera.transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, -10);
        
    }

    void CenterCameraOnBothPlayers(Transform player1Transform, Transform player2Transform)
    {
        Debug.Log("Following midpoint");
        // Calculate the midpoint between both players
        Vector3 midpoint = (player1Transform.position + player2Transform.position) / 2;

        // Center the camera on the midpoint
        mainCamera.transform.position = new Vector3(
            midpoint.x,
            midpoint.y,
            -10
        );

        // Update the offset for a new fixed state
        remainingPlayerOffset = mainCamera.transform.position - midpoint;
    }
    void EnterFixedMode()
    {
        Debug.Log("Both players dead, entering Fixed Camera mode");
        isFixedMode = true;
        // Optionally reset the camera to a fixed position here
    }
    private void FixedCamera()
    {
        //Debug.Log("Fixed Camera mode");
        // Keep the camera fixed at the current position

        //mainCamera.transform.position = new Vector3(
        //    Mathf.Clamp(mainCamera.transform.position.x, fixedCameraBoundsMin.x, fixedCameraBoundsMax.x),
        //    Mathf.Clamp(mainCamera.transform.position.y, fixedCameraBoundsMin.y, fixedCameraBoundsMax.y),
        //    -10
        //);

        // Prevent players from crossing the boundaries (air walls)
         if(deathnum == 1)
         {
             isFixedMode = false;
         }



    }


}
