using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ClubController : MonoBehaviour
{
    [Header("Inventory")]
    public List<Club> clubInventory;
    private int currentClubIndex = 0;

    [Header("References")]
    public ClubHeadSensor clubSensor;
    public TextMeshPro uiText; // Optional: World space text on controller or HUD

    [Header("Input")]
    public InputActionProperty swapInput; // e.g. Thumbstick Y
    public float inputDeadzone = 0.5f;
    private bool _inputReset = true; // To prevent rapid scrolling

    // Debug Fallback
    public Key nextClubKey = Key.E;
    public Key prevClubKey = Key.Q;

    void Start()
    {
        if (clubInventory == null || clubInventory.Count == 0)
        {
            Debug.LogWarning("ClubController: No clubs in inventory!");
            return;
        }

        // Initialize
        SelectClub(0);
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // 1. Keyboard Debug
        if (Keyboard.current != null)
        {
            if (Keyboard.current[nextClubKey].wasPressedThisFrame)
                CycleClub(1);
            if (Keyboard.current[prevClubKey].wasPressedThisFrame)
                CycleClub(-1);
        }

        // 2. VR Input (Thumbstick)
        if (swapInput.action != null)
        {
            float value = swapInput.action.ReadValue<Vector2>().y;

            if (Mathf.Abs(value) > inputDeadzone)
            {
                if (_inputReset)
                {
                    if (value > 0) CycleClub(1);
                    else CycleClub(-1);
                    _inputReset = false;
                }
            }
            else
            {
                _inputReset = true;
            }
        }
    }

    void CycleClub(int direction)
    {
        if (clubInventory.Count == 0) return;

        currentClubIndex += direction;

        // Wrap around
        if (currentClubIndex >= clubInventory.Count) currentClubIndex = 0;
        if (currentClubIndex < 0) currentClubIndex = clubInventory.Count - 1;

        SelectClub(currentClubIndex);
    }

    void SelectClub(int index)
    {
        if (index < 0 || index >= clubInventory.Count) return;

        Club selected = clubInventory[index];
        
        // Apply to Sensor
        if (clubSensor != null)
        {
            clubSensor.currentClub = selected;
        }

        // Update UI
        if (uiText != null)
        {
            uiText.text = selected.name; // Assuming Asset Name is "Driver", "Putter", etc.
        }

        Debug.Log($"Switched to Club: {selected.name}");
    }
}
