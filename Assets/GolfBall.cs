using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class GolfBall : MonoBehaviour
{
    private new Rigidbody rigidbody;

    [Header("Physics Settings")]
    public float magnusConstant = 0.0005f;
    public float dragCoefficient = 0.01f;

    [Header("Mobile/Debug Compatibility")]
    public float force = 20f;
    [Range(-1, 1)] public float backspin;
    [Range(-1, 1)] public float sideSpin;

    [Header("State")]
    public bool isHit = false; // Public field for compatibility
    public bool IsHit => isHit; // Property accessor for ClubHeadSensor

    [Header("Stats")]
    public BallStats stats;
    private Vector3 startPos;

    [Header("Values")]
    public float minHeight = -20.0f;

    // Optional: Events for other systems (Audio/VFX)
    public UnityEvent onShotStart = new UnityEvent();
    public UnityEvent onShotEnd = new UnityEvent();

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!isHit)
            return;

        // 0. Check for Fall
        if (transform.position.y < minHeight)
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.isKinematic = true; // Stop moving
            ResetHit();
            return;
        }

        // 1. Apply Magnus Effect (Lift/Curve)
        if (magnusConstant > 0)
        {
            Vector3 magnusForce = magnusConstant * Vector3.Cross(rigidbody.angularVelocity, rigidbody.linearVelocity);
            rigidbody.AddForce(magnusForce);
        }

        // 2. Track Stats
        stats.distance = Vector3.Distance(transform.position, startPos);
        if (transform.position.y > stats.height)
            stats.height = transform.position.y;

        // 3. Check for Stop
        if (rigidbody.linearVelocity.magnitude < 0.1f && isHit)
        {
             ResetHit();
        }
    }

    /// <summary>
    /// Legacy method for Mobile UI / Driving Range Controller
    /// </summary>
    public void HitBall()
    {
        if (isHit) return;

        // Convert the "Mobile" params to our new system
        // Mobile used AddRelativeForce(Vector3.forward * force)
        Vector3 worldForce = transform.TransformDirection(Vector3.forward * force);
        
        // Mobile used AddRelativeTorque(new Vector3(backspin, sideSpin, 0))
        // Note: Mobile spin range was -1 to 1. We might need to scale this if it feels weak in the new physics.
        // For now, we pass it raw to preserve logic.
        Vector3 worldTorque = transform.TransformDirection(new Vector3(backspin, sideSpin, 0));

        Launch(worldForce, worldTorque);
    }

    public void SetLaunchAngle(float launchAngle)
    {
        var rot = transform.eulerAngles;
        rot.x = -launchAngle;
        transform.eulerAngles = rot;
    }

    /// <summary>
    /// Applies an impulse force and torque to the ball to simulate a hit.
    /// </summary>
    public void Launch(Vector3 forceVector, Vector3 torqueVector)
    {
        if (isHit) return;

        isHit = true;
        
        // Reset Stats
        startPos = transform.position;
        stats = new BallStats();
        
        onShotStart?.Invoke();

        // Ensure we are in a dynamic state
        rigidbody.isKinematic = false;
        rigidbody.WakeUp();

        // Apply Impact
        rigidbody.AddForce(forceVector, ForceMode.Impulse);
        rigidbody.AddTorque(torqueVector, ForceMode.Impulse);

        Debug.Log($"Ball Launched! Force: {forceVector.magnitude} | Spin: {torqueVector}");
    }

    public void SetHitFlag()
    {
        isHit = true;
        Invoke(nameof(ResetHit), 1.0f);
    }

    private void ResetHit()
    {
        isHit = false;
        onShotEnd?.Invoke();
    }
}