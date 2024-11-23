using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab; // Reference to the player prefab
    public Vector3 spawnOffset = new Vector3(0, 0, 5); // Offset relative to the camera's position and forward direction

    public float respawnCooldown = 3f; // Cooldown in seconds

    private bool isRespawning = false;

    void Update()
    {
        // Check if the player is null and start respawn process if not already respawning
        if (GameObject.FindGameObjectWithTag("Player 1") == null && !isRespawning)
        {
            StartCoroutine(RespawnPlayer());
        }
    }

    private IEnumerator RespawnPlayer()
    {
        isRespawning = true;

        // Wait for the cooldown duration
        yield return new WaitForSeconds(respawnCooldown);

        // Calculate the spawn point relative to the camera
        Vector3 spawnPoint = Camera.main.transform.position + Camera.main.transform.forward * spawnOffset.z
                             + Camera.main.transform.right * spawnOffset.x
                             + Camera.main.transform.up * spawnOffset.y;

        // Spawn the player prefab at the calculated spawn point
        if (playerPrefab != null)
        {
            Instantiate(playerPrefab, spawnPoint, Quaternion.identity);
        }

        isRespawning = false; // Reset respawning flag
    }
}
