using UnityEngine;
/*

	got loft and distance references from https://www.vaughns-1-pagers.com/sports/golf-club-data.htm

 */

[System.Serializable]
[CreateAssetMenu]
public class Club : ScriptableObject
{
	[Range(0f, 60f), Tooltip("Angle of club in degrees")]
	public float loft;
	[Tooltip("Avg min distance in metres")]
	public float avgDistMin;
	[Tooltip("Avg max distance in metres")]
	public float avgDistMax;

    [Header("Physics Settings")]
    public float forceMultiplier = 1.3f; // Default "Smash Factor"
    public float spinRate = 10f; // Base backspin amount

	public override string ToString()
	{
		return JsonUtility.ToJson(this, true);
	}
}