using UnityEngine;
using System.Collections;

public class DynamicCamera : MonoBehaviour
{
    public Camera mainCamera; // Reference to the main camera
    public string player1Tag = "Player1"; // Tag for player 1
    public string player2Tag = "Player2"; // Tag for player 2

    public GameObject playerPrefab; // Player prefab for respawning
    public Vector3 spawnOffset = new Vector3(0, 0, 0); // Offset for respawning relative to the camera
    public float respawnCooldown = 3f; // Cooldown duration before respawning

    private bool isRespawning = false;

    private GameObject player1; // Cached reference for Player 1
    private GameObject player2; // Cached reference for Player 2
    private Vector3 remainingPlayerOffset; // Offset of the remaining player relative to the camera

    void Start()
    {
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

    void LateUpdate()
    {
        // Update player references
        player1 = GameObject.FindGameObjectWithTag(player1Tag);
        player2 = GameObject.FindGameObjectWithTag(player2Tag);

        if (player1 != null && player2 == null && !isRespawning)
        {
            FollowRemainingPlayer(player1.transform);
            StartCoroutine(RespawnPlayer(player2Tag));
        }
        else if (player2 != null && player1 == null && !isRespawning)
        {
            FollowRemainingPlayer(player2.transform);
            StartCoroutine(RespawnPlayer(player1Tag));
        }
        else if (player1 != null && player2 != null)
        {
            CenterCameraOnBothPlayers(player1.transform, player2.transform);
        }
    }

    void FollowRemainingPlayer(Transform playerTransform)
    {
        // Keep the camera at the same relative offset to the remaining player
        mainCamera.transform.position = playerTransform.position + remainingPlayerOffset;
    }

    void CenterCameraOnBothPlayers(Transform player1Transform, Transform player2Transform)
    {
        // Calculate the midpoint between both players
        Vector3 midpoint = (player1Transform.position + player2Transform.position) / 2;

        // Center the camera on the midpoint
        mainCamera.transform.position = new Vector3(
            midpoint.x,
            midpoint.y,
            mainCamera.transform.position.z
        );

        // Update the offset for a new fixed state
        remainingPlayerOffset = mainCamera.transform.position - midpoint;
    }

    IEnumerator RespawnPlayer(string playerTag)
    {
        isRespawning = true;

        // Wait for the cooldown duration
        yield return new WaitForSeconds(respawnCooldown);

        // Calculate the spawn point relative to the camera
        Vector3 spawnPoint = mainCamera.transform.position + mainCamera.transform.forward * spawnOffset.z
                             + mainCamera.transform.right * spawnOffset.x
                             + mainCamera.transform.up * spawnOffset.y;

        // Spawn the player prefab at the calculated spawn point
        if (playerPrefab != null)
        {
            GameObject newPlayer = Instantiate(playerPrefab, spawnPoint, Quaternion.identity);
            newPlayer.tag = playerTag; // Assign the correct tag to the respawned player
        }

        isRespawning = false; // Reset respawning flag
    }
}
