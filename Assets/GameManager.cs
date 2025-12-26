using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private int currentHoleNumber = 0;

    public List<Transform> startingPositions;
    public List<Transform> holePositions;

    public Vector3 CurrentHolePosition
    {
        get
        {
            if (holePositions != null && currentHoleNumber < holePositions.Count && holePositions[currentHoleNumber] != null)
            {
                return holePositions[currentHoleNumber].position;
            }
            return Vector3.zero; // Fallback
        }
    }

    public Rigidbody ballRigidbody;

    public TMPro.TextMeshPro textmesh;

    public int currentHitNumber = 0;
    private List<int> previousHitNumbers = new List<int>();

    public float respawnDelay = 3.0f;
    private GolfBall golfBall;

    // Start is called before the first frame update
    void Start()
    {
        ballRigidbody.transform.position = startingPositions[currentHoleNumber].position;
        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;

        textmesh.text = "";

        golfBall = ballRigidbody.GetComponent<GolfBall>();
        if (golfBall != null)
        {
            golfBall.onShotEnd.AddListener(OnShotEnded);
        }
    }

    private void OnShotEnded()
    {
        StartCoroutine(AutoRespawnRoutine());
    }

    private IEnumerator AutoRespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        // Reset to first hole (element 0)
        currentHoleNumber = 0;
        
        if (startingPositions != null && startingPositions.Count > 0 && startingPositions[0] != null)
        {
            ballRigidbody.transform.position = startingPositions[0].position;
            ballRigidbody.linearVelocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
            ballRigidbody.isKinematic = false; // Ensure physics is enabled
            
            // Also reset current hit count if desired, though user didn't explicitly ask, it makes sense for a "respawn"
            currentHitNumber = 0;
            DisplayScore();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            GoToNextHole();
        }
    }

    public void GoToNextHole()
    {
        currentHoleNumber += 1;

        if(currentHoleNumber >= startingPositions.Count)
        {
            Debug.Log("We reached the end");
        }
        else
        {
            ballRigidbody.transform.position = startingPositions[currentHoleNumber].position;

            ballRigidbody.linearVelocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
        }

        previousHitNumbers.Add(currentHitNumber);
        currentHitNumber = 0;
        DisplayScore();
    }

    public void DisplayScore()
    {
        string scoreText = "";

        for (int i = 0; i < previousHitNumbers.Count; i++)
        {
            scoreText += "HOLE " + (i + 1) + " - " + previousHitNumbers[i] + "<br>";
        }

        textmesh.text = scoreText;
    }
}
