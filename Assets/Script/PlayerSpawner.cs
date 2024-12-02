using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab; // Reference to the player prefab
    public Vector3 spawnOffset = new Vector3(0, 0, 5); // Offset relative to the camera's position and forward direction

    public float respawnCooldown = 3f; // Cooldown in seconds

    public bool isRespawning = false;
    public Sword swordstate;
    void Update()
    {
        swordstate = GetComponent<Sword>();
        // Check if the player is null and start respawn process if not already respawning
        if (GameObject.FindGameObjectWithTag(playerPrefab.tag) == null && !isRespawning)
        {
            StartCoroutine(RespawnPlayer());
        }
        
    }

    public IEnumerator RespawnPlayer()
    {
        isRespawning = true;

        // Wait for the cooldown duration
        yield return new WaitForSeconds(respawnCooldown);

        // Calculate the spawn point relative to the camera
        Vector3 spawnPoint = Camera.main.transform.position 
                             + Camera.main.transform.right * spawnOffset.x
                             + Camera.main.transform.up * spawnOffset.y;
        spawnPoint.z += spawnOffset.z;
        // Spawn the player prefab at the calculated spawn point
        if (playerPrefab != null)
        {
            Instantiate(playerPrefab, spawnPoint, Quaternion.identity);
            //swordstate.death_num--;
        }

        isRespawning = false; // Reset respawning flag
    }
}
