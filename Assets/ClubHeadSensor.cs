using UnityEngine;

public class ClubHeadSensor : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask ballLayer; // Set this to "GolfBall" in Inspector
    public Transform sweetSpot; // Create a tiny empty GameObject at the center of your club face and assign it here

    [Header("Audio & Haptics")]
    public AudioSource hitAudio;
    public AudioClip hitImpactSound;
    public UnityEngine.XR.XRNode controllerNode = UnityEngine.XR.XRNode.RightHand;

    [Header("Club Data")]
    public Club currentClub; // Assigned by ClubController

    [Header("Debug")]
    public Vector3 currentVelocity;

    private Vector3 _previousPosition;
    private float _lastHitTime;
    private float _hitCooldown = 0.2f; // Seconds to wait before hitting again

    // We use FixedUpdate for consistent physics calculation
    void Start()
    {
        if (sweetSpot == null) sweetSpot = transform;
        _previousPosition = sweetSpot.position;
    }

    void FixedUpdate()
    {
        // 1. Calculate Velocity
        // Velocity = (Distance / Time)
        Vector3 currentPosition = sweetSpot.position;
        Vector3 direction = currentPosition - _previousPosition;
        float dist = direction.magnitude;

        // Calculate velocity in meters per second
        if (Time.fixedDeltaTime > 0)
            currentVelocity = direction / Time.fixedDeltaTime;

        // 2. The Sweep Test (The "Ghost" collider)
        // We cast a box from where we WERE to where we ARE.
        // If the ball is in between, we hit it.
        RaycastHit hit;

        // We use the club's rotation for the box orientation
        // boxHalfExtents should be roughly half the size of your club head
        Vector3 boxHalfExtents = transform.localScale / 2f;

        if (Physics.BoxCast(_previousPosition, boxHalfExtents, direction.normalized,
            out hit, transform.rotation, dist, ballLayer))
        {
            // We hit the ball!
            GolfBall ball = hit.collider.GetComponent<GolfBall>();
            if (ball != null && !ball.IsHit)
            {
                // Cooldown Check
                if (Time.time < _lastHitTime + _hitCooldown)
                    return;

                ApplyPhysicsForce(ball, direction.normalized, currentVelocity.magnitude);
            }
        }

        _previousPosition = currentPosition;
    }

    void ApplyPhysicsForce(GolfBall ball, Vector3 hitDirection, float speed)
    {
        if (currentClub == null)
        {
            Debug.LogWarning("ClubHeadSensor: No Club assigned!");
            return;
        }

        // 1. Calculate Launch Vector
        // We can add "Loft" by rotating the hit direction upwards
        // Axis of rotation is the Cross product of Up and HitDirection (the "Right" vector relative to the shot)
        Vector3 shotRight = Vector3.Cross(Vector3.up, hitDirection).normalized;
        Quaternion loftRotation = Quaternion.AngleAxis(-currentClub.loft, shotRight); // Negative because we rotate 'up' around the right axis
        
        Vector3 finalLaunchVector = (loftRotation * hitDirection).normalized;

        // Update Cooldown
        _lastHitTime = Time.time;

        // 2. Calculate Force
        Vector3 forceVector = finalLaunchVector * speed * currentClub.forceMultiplier;

        // 3. Calculate Spin
        // For this merge, we apply a base backspin plus some side spin if the swing is curved?
        // Let's stick to the mobile project's "Preset" style logic: add consistent backspin.
        // In real life, spin is determined by the friction and relative velocity of the surface.
        // Mobile Sim: "Backspin = 1.0 (Topspin) to -1.0 (Backspin)".
        // We'll apply a Torque vector. Backspin rotates around the "Right" axis.
        Vector3 spinTorque = shotRight * -currentClub.spinRate; 

        // Launch!
        ball.Launch(forceVector, spinTorque);
        
        // Audio
        if (hitAudio != null && hitImpactSound != null)
        {
            // Simple volume curve based on speed (capped at 1.0)
            float vol = Mathf.Clamp01(speed / 10f); // Adjust denominator for sensitivity
            hitAudio.PlayOneShot(hitImpactSound, vol);
        }

        // Haptics
        var device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(controllerNode);
        if (device.isValid)
        {
            float intensity = Mathf.Clamp01(speed / 10f);
            device.SendHapticImpulse(0, intensity, 0.1f); // Channel 0, Intensity, Duration
        }

        Debug.Log($"Impact! Speed: {speed} m/s -> Launch Force: {forceVector.magnitude}, Torque: {spinTorque}");
    }
}