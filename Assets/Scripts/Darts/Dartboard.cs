using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EditorAttributes;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static Dartboard;

public class Dartboard : MonoBehaviour
{
    public enum Team
    {
        Red,
        Blue
    }

    [Serializable]
    public struct DartsScore
    {
        public const int StartScore = 301; // Maximum number of scores to keep track of
        public List<int> score; // We subtract, so we to keep each score earned
        public int hitCount;
        public TMP_Text scoreText;
        public TMP_Text finalScoreText;
        public Team team;
    }

    [Header("Score Data")]
    public DartsScore red;
    public DartsScore blue;

    private Team? wonTeam = null;

    [Header("Dartboard Settings")]
    public float doubleBullseyeRadius = 0.1f;
    public float bullseyeRadius = 0.2f; 
    public float firstSingleRingRadius = 0.3f; 
    public float tripleRingRadius = 0.4f; 
    public float secondSingleRingRadius = 0.5f; 
    public float doubleRingRadius = 0.6f;

    public float sphereOffset = 0.1f; // Offset to position the sphere correctly
    public float degreeOffset = 0f;

    void OnTriggerEnter(Collider other)
    {
        // Compare layer to see if it's Dart
        if (other.gameObject.layer == LayerMask.NameToLayer("Dart"))
        {
            Vector3 hitPoint = other.transform.position;
            CalculateScore(hitPoint, other.CompareTag("Red") ? Team.Red : Team.Blue);
        }
    }

    private void Start()
    {
        //todo initialize text here
    }

    [Button]
    public void ResetGame()
    {
        wonTeam = null; // Reset the winning team
        // Reset scores
        red.score.Clear();
        red.hitCount = 0;
        red.finalScoreText.text = DartsScore.StartScore.ToString();
        red.scoreText.text = $"{DartsScore.StartScore}\n";
        blue.score.Clear();
        blue.hitCount = 0;
        blue.finalScoreText.text = DartsScore.StartScore.ToString();
        blue.scoreText.text = $"{DartsScore.StartScore}\n";
        // Reset final score text visibility
        red.finalScoreText.transform.GetChild(0).GetComponent<Image>().enabled = false;
        blue.finalScoreText.transform.GetChild(0).GetComponent<Image>().enabled = false;
    }

    void CalculateScore(Vector3 worldHitPoint, Team team)
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
        //Debug.Log($"Radius: {radius:F4}, Angle: {angle:F1}, FlatHit: {flatHit}");

        int score = GetScore(radius, angle);
        if(score > 0)
        {
            DartsScore teamScore = (team == Team.Red) ? red : blue;
            IncrementScore(score, teamScore);
        }
    }

    private void IncrementScore(int newScore, DartsScore scoreData)
    {
        if (wonTeam.HasValue && scoreData.team == wonTeam.Value) return; // if you already won, cannot score more

        int totalScore = scoreData.score.Sum() + newScore;
        int newCurrentScore = DartsScore.StartScore - totalScore;

        if (newCurrentScore < 0) return; // Must get exactly zero to win

        scoreData.hitCount++;
        scoreData.score.Add(newScore);

        StringBuilder scoreTextBuilder = new();
        scoreTextBuilder.AppendLine($"{DartsScore.StartScore.ToString()}");

        foreach (int score in scoreData.score)
        {
            scoreTextBuilder.AppendLine("-" + score.ToString());
        }

        scoreData.finalScoreText.text = newCurrentScore.ToString();
        scoreData.scoreText.text = scoreTextBuilder.ToString();

        if (newCurrentScore == 0 && !wonTeam.HasValue)
        {
            wonTeam = scoreData.team; // Set the winning team
            scoreData.finalScoreText.transform.GetChild(0).GetComponent<Image>().enabled = true;
        }
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
