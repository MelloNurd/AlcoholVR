using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Dartboard : MonoBehaviour
{
    public float doubleBullseyeRadius = 0.1f;
    public float bullseyeRadius = 0.2f; 
    public float firstSingleRingRadius = 0.3f; 
    public float tripleRingRadius = 0.4f; 
    public float secondSingleRingRadius = 0.5f; 
    public float doubleRingRadius = 0.6f;

    public float sphereOffset = 0.1f; // Offset to position the sphere correctly
    public float degreeOffset = 0f;
    int hitCount = 0;
    int score = 0;

    [SerializeField] TextMeshPro currentScore;

    void OnTriggerEnter(Collider other)
    {
        // Compare layer to see if it's Dart
        if (other.gameObject.layer == LayerMask.NameToLayer("Dart"))
        {
            Vector3 hitPoint = other.transform.position;
            CalculateScore(hitPoint);
        }
    }

    void CalculateScore(Vector3 worldHitPoint)
    {
        Vector3 boardCenter = transform.position;
        Vector3 toHit = worldHitPoint - boardCenter;

        // Flatten the vector to the board's face plane
        Vector3 flatHit = Vector3.ProjectOnPlane(toHit, transform.forward);
        float radius = flatHit.magnitude;

        // Get angle relative to the board’s right/up
        float angle = Mathf.Atan2(
            Vector3.Dot(flatHit, transform.up),
            Vector3.Dot(flatHit, transform.right)
        ) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        angle += degreeOffset; // Apply any additional offset
        Debug.Log($"Radius: {radius:F4}, Angle: {angle:F1}, FlatHit: {flatHit}");

        if (hitCount == 3)
        {
            hitCount = 0; // Reset hit count after 3 hits
            score = 0;
        }
        
        hitCount++;
        score += GetScore(radius, angle);
        currentScore.text = score.ToString();
    }

    int GetScore(float radius, float angle)
    {
        // Create the corrected sector scores array
        int[] sectorScores = { 12, 5, 20, 1, 18, 4, 13, 6, 10, 15, 2, 17, 3, 19, 7, 16, 8, 11, 14, 9 };

        // Calculate the sector
        int sector = Mathf.FloorToInt(angle / 18f); // 360°/20 sectors = 18° each

        // Double bullseye
        if (radius < doubleBullseyeRadius)
        {
            return 50;
        }
        // Single bullseye
        else if (radius < bullseyeRadius)
        {
            return 25;
        }
        // First single ring
        else if (radius < firstSingleRingRadius)
        {
            return sectorScores[sector];
        }
        // Triple ring
        else if (radius < tripleRingRadius)
        {
            return sectorScores[sector] * 3; // Triple score
        }
        // Second single ring
        else if (radius < secondSingleRingRadius)
        {
            return sectorScores[sector];
        }
        // Double Ring
        else if (radius < doubleRingRadius)
        {
            return sectorScores[sector] * 2; // Double score
        }
        // Miss
        else
        {
            return 0;
        }
    }

    // Visualize the radius zones in the editor
    void OnDrawGizmos()
    {
        Vector3 boardCenter = transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(boardCenter, doubleBullseyeRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(boardCenter, bullseyeRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(boardCenter, firstSingleRingRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(boardCenter, tripleRingRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(boardCenter, secondSingleRingRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(boardCenter, doubleRingRadius);
    }
}
