using TMPro;
using UnityEngine;

public class HoopScore : MonoBehaviour
{
    float CurrentDelay = 0f; // Current delay timer
    float ScoreDelay = 0.5f; // Delay before scoring again
    int currentScore = 0; // Current score
    TextMeshPro ScoreText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //set score text to the format "00" at start
        ScoreText = gameObject.GetComponentInChildren<TextMeshPro>();
        ScoreText.text = currentScore.ToString("00");
    }

    // Update is called once per frame
    void Update()
    {
        if(CurrentDelay > 0f)
        {
            CurrentDelay -= Time.deltaTime; // Decrease the delay timer
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CurrentDelay <= 0f)
        {
            currentScore++; // Increment the score
            if (currentScore > 99) // Cap the score at 99
            {
                currentScore = 0;
            }
            ScoreText.text = currentScore.ToString("00"); // Update the score text
            CurrentDelay = ScoreDelay; // Reset the delay timer
        }
    }
}
