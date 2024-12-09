using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    public Camera mainCamera; // Reference to the main camera
    public string player1Tag = "Player 1"; // Tag for player 1
    public string player2Tag = "Player 2"; // Tag for player 2

    public GameObject playerPrefab; // Player prefab for respawning
    public Vector3 spawnOffset = new Vector3(0, 0, 0); // Offset for respawning relative to the camera
    public float respawnCooldown = 3f; // Cooldown duration before respawning

    public GameObject airWallPrefab; // Prefab for the air wall
    private GameObject leftWall; // Instance for the left wall
    private GameObject rightWall; // Instance for the right wall
    private GameObject moveLeftWall;
    private GameObject moveRightWall;

    private bool isRespawning = false;
    private GameObject player1; // Cached reference for Player 1
    private GameObject player2; // Cached reference for Player 2
    private Vector3 remainingPlayerOffset; // Offset of the remaining player relative to the camera

    private bool isFixedMode = true; // Starts in Fixed Camera mode
    public int deathnum = 2;
    private float hasWall = 0;

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

        // Generate air walls at the camera boundaries
        GenerateAirWalls();
    }

    void GenerateAirWalls()
    {
        if (airWallPrefab == null)
        {
            Debug.LogError("AirWall prefab not assigned!");
            return;
        }

        // Calculate camera boundaries
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        float leftEdge = mainCamera.transform.position.x - cameraWidth / 2;
        float rightEdge = mainCamera.transform.position.x + cameraWidth / 2;
        float wallHeight = cameraHeight;

        // Instantiate air walls at the edges
        leftWall = Instantiate(airWallPrefab, new Vector3(leftEdge, mainCamera.transform.position.y, 0), Quaternion.identity);
        rightWall = Instantiate(airWallPrefab, new Vector3(rightEdge, mainCamera.transform.position.y, 0), Quaternion.identity);

        // Scale the walls to match the camera height
        Vector3 wallScale = leftWall.transform.localScale;
        wallScale.y = wallHeight;
        leftWall.transform.localScale = wallScale;
        rightWall.transform.localScale = wallScale;

        Debug.Log("Air walls generated.");
    }

    void Update()
    {
        // Update player references
        player1 = GameObject.FindGameObjectWithTag(player1Tag);
        player2 = GameObject.FindGameObjectWithTag(player2Tag);

        if (isFixedMode)
        {
            hasWall = 0;
            FixedCamera(); // Fixed camera logic
            return;
        }

        if (player1 != null && player2 == null )
        {
            hasWall = 1;
            FollowRemainingPlayer(player1.transform);
            if (moveRightWall != null)
            {
                Destroy(moveRightWall);
            }
            
        }
        else if (player2 != null && player1 == null  )
        {
            hasWall= 2;
            FollowRemainingPlayer(player2.transform);
            if (moveLeftWall != null)
            {
                Destroy(moveLeftWall);
            }
        }
        if (player1 != null && player2 != null)
        {
            CenterCameraOnBothPlayers(player1.transform, player2.transform);
        }
        if (hasWall == 1 && moveLeftWall == null)
        {
            float cameraHeight = 2f * mainCamera.orthographicSize;
            float cameraWidth = cameraHeight * mainCamera.aspect;
            float leftEdge = mainCamera.transform.position.x - cameraWidth /2 ;
            moveLeftWall = Instantiate(airWallPrefab, new Vector3(leftEdge, mainCamera.transform.position.y, 0), Quaternion.identity);
        }
        if (hasWall == 2 && moveRightWall == null)
        {
            float cameraHeight = 2f * mainCamera.orthographicSize;
            float cameraWidth = cameraHeight * mainCamera.aspect;
            float rightEdge = mainCamera.transform.position.x + cameraWidth /2;
            moveRightWall = Instantiate(airWallPrefab, new Vector3(rightEdge, mainCamera.transform.position.y, 0), Quaternion.identity);
        }
        if (hasWall == 0)
        {
            if (moveLeftWall != null)
            {
                Destroy(moveLeftWall);
            }
            if (moveRightWall != null)
            {
                Destroy(moveRightWall);
            }
        }
        // Both players dead
        if (player1 == null && player2 == null)
        {
            EnterFixedMode();
        }
    }

    void FollowRemainingPlayer(Transform playerTransform)
    {
        mainCamera.transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, -10);
        Destroy(leftWall);
        Destroy(rightWall);
    }

    void CenterCameraOnBothPlayers(Transform player1Transform, Transform player2Transform)
    {
        Vector3 midpoint = (player1Transform.position + player2Transform.position) / 2;
        mainCamera.transform.position = new Vector3(midpoint.x, midpoint.y+2, -10);
        remainingPlayerOffset = mainCamera.transform.position - midpoint;

        if (deathnum == 2)
        {
            isFixedMode = true;
        }
    }

    void EnterFixedMode()
    {
        Debug.Log("Both players dead, entering Fixed Camera mode");
        isFixedMode = true;
    }

    private void FixedCamera()
    {
        if (deathnum == 1)
        {
            isFixedMode = false;
        }
    }
}
