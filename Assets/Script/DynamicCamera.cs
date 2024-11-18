using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    public Transform player1; // Reference to Player 1
    public Transform player2; // Reference to Player 2

    public float smoothSpeed = 0.125f; // Smooth transition speed
    public Vector2 minCameraBounds; // Minimum camera boundaries (x, y)
    public Vector2 maxCameraBounds; // Maximum camera boundaries (x, y)
    public float minZoom = 5f; // Minimum zoom level
    public float maxZoom = 15f; // Maximum zoom level
    public float zoomLimiter = 10f; // Controls zoom sensitivity based on distance

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void FixedUpdate()
    {
        if (player1 == null || player2 == null)
        {
            Debug.LogWarning("Player references are missing!");
            return;
        }

        // Update camera position
        Vector3 desiredPosition = GetMidpoint();
        desiredPosition.z = transform.position.z; // Maintain original Z position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Update camera zoom
        float distance = Vector2.Distance(player1.position, player2.position);
        float desiredZoom = Mathf.Lerp(maxZoom, minZoom, distance / zoomLimiter);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, desiredZoom, smoothSpeed);

        // Clamp camera to boundaries
        ClampCamera();
    }

    private Vector3 GetMidpoint()
    {
        Vector3 midpoint = (player1.position + player2.position) / 2f;
        return midpoint;
    }

    private void ClampCamera()
    {
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minCameraBounds.x, maxCameraBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minCameraBounds.y, maxCameraBounds.y);
        transform.position = clampedPosition;
    }
}

