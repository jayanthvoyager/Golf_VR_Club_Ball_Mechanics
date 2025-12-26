using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GolfMovement : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public Transform xrRig; // Assign your XR Origin / Rig here
    public Transform mainCamera; // Determine player's head position

    [Header("Settings")]
    public float stanceDistance = 0.5f; // Distance behind the ball
    public InputActionProperty teleportInput; // New Input System Property

    // Fallback simple input for testing without configuring bindings
    public Key debugKey = Key.T;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (xrRig == null)
            xrRig = transform; // Assume script is on the rig if not assigned
            
        if (mainCamera == null)
            mainCamera = Camera.main.transform;
    }

    void Update()
    {
        // Check New Input System
        if (teleportInput.action != null && teleportInput.action.WasPressedThisFrame())
        {
            TeleportToBall();
        }

        // Check Debug Key
        if (Keyboard.current != null && Keyboard.current[debugKey].wasPressedThisFrame)
        {
            TeleportToBall();
        }
    }

    public void TeleportToBall()
    {
        if (gameManager == null || gameManager.ballRigidbody == null)
        {
            Debug.LogWarning("GolfMovement: missing Game Manager or Ball");
            return;
        }

        Vector3 ballPos = gameManager.ballRigidbody.transform.position;
        Vector3 targetPos = gameManager.CurrentHolePosition;

        // If no target (e.g. 0,0,0 fallback), just forward
        Vector3 toHole = Vector3.forward;
        
        if (targetPos != Vector3.zero)
        {
            toHole = targetPos - ballPos;
        }

        // Flatten logic for floor alignment
        toHole.y = 0;
        if (toHole.sqrMagnitude < 0.001f) toHole = xrRig.forward;
        toHole.Normalize();

        // Calculate Position behind ball
        // We want the player's FEET to be at (Ball - backward_offset)
        // But side-on? Golfers stand to the side. 
        // Let's assume standard "Behind the ball" viewing the line for now (Golf+ "Go to Ball" typically puts you behind it to aim).
        // Then user steps to the side to hit.
        Vector3 idealRigPos = ballPos - (toHole * stanceDistance);

        // Adjust for Head Position Offset (centering the player)
        // If the player is standing 1m 'right' of their play area center, we need to shift the rig 1m 'left' so their HEAD ends up at the ideal spot.
        // However, usually we just center the PLAY AREA (Rig).
        // Let's stick to moving the Rig Position to the ideal spot. 
        // Improve: Rotate Rig so 'Forward' matches 'toHole'.
        
        // 1. Rotate Rig
        Quaternion targetRotation = Quaternion.LookRotation(toHole, Vector3.up);
        xrRig.rotation = targetRotation;

        // 2. Move Rig
        // Maintain current Y (height) of Rig usually 0?
        idealRigPos.y = xrRig.position.y;
        xrRig.position = idealRigPos;

        Debug.Log("Teleported to Ball");
    }
}
