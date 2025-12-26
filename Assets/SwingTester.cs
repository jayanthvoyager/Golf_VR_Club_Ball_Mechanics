using UnityEngine;
using System.Collections;

public class SwingTester : MonoBehaviour
{
    [Header("References")]
    public Transform pivotPoint; // The "Shoulder"
    public Transform clubContainer; // The object holding your Club
    public Rigidbody ballRb;
    public Transform ballStartPosition;

    [Header("Test Configuration")]
    public float swingSpeed = 300f; // Degrees per second
    public float resetDelay = 3f; // Time between swings
    public bool randomizeAngle = true;
    public float angleVariance = 5f; // Random +/- degrees

    private Quaternion initialRotation;
    private bool isSwinging = false;

    void Start()
    {
        initialRotation = pivotPoint.localRotation;
        StartCoroutine(TestLoop());
    }

    IEnumerator TestLoop()
    {
        while (true)
        {
            // 1. Reset Setup
            ResetBall();
            ResetClub();

            // 2. Randomize (The "Slightly Different Angle" request)
            if (randomizeAngle)
            {
                float randomOffset = Random.Range(-angleVariance, angleVariance);
                // Rotate the entire pivot slightly on the Y axis to hit toe/heel
                pivotPoint.parent.localRotation = Quaternion.Euler(0, randomOffset, 0);
            }

            yield return new WaitForSeconds(1f); // Wait a moment before swinging

            // 3. Swing!
            isSwinging = true;
            float duration = 0.5f; // Half second swing
            float timer = 0f;

            while (timer < duration)
            {
                // Rotate the shoulder (Pivot) on the X axis
                pivotPoint.Rotate(Vector3.right * swingSpeed * Time.fixedDeltaTime);
                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            isSwinging = false;

            // 4. Wait to see result
            yield return new WaitForSeconds(resetDelay);
        }
    }

    void ResetBall()
    {
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.position = ballStartPosition.position;
        ballRb.rotation = Quaternion.identity;
    }

    void ResetClub()
    {
        pivotPoint.localRotation = initialRotation;
    }
}