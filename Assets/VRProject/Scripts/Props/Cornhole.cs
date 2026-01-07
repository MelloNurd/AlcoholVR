using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Cornhole : MonoBehaviour
{
    [Serializable]
    public class CornholeScore
    {
        public Team team;
        public int score;
        public TMP_Text scoreText;

        public async void SetScore(int newScore)
        {
            score = newScore;

            await UniTask.Delay(100); // small delay to avoid flickering text if multiple collisions happen quickly

            if (scoreText != null) scoreText.text = score.ToString();
        }

        public void HideText()
        {
            if (scoreText != null) scoreText.text = "";
        }
    }

    private Dictionary<GameObject, Vector3> cornholeBags = new(); // Using this just to reset bag positions

    // Track bag states to prevent double scoring
    private Dictionary<GameObject, BagState> bagStates = new();

    private enum BagState
    {
        None,       // Not on board or in hole
        OnBoard,    // On the board (1 point)
        InHole      // In the hole (3 points)
    }

    private ColliderEventHandler colliders;
    private TriggerEventHandler triggers;

    [SerializeField] private CornholeScore yellow;
    [SerializeField] private CornholeScore green;

    private bool isGameActive = true;

    private void Awake()
    {
        yellow.team = Team.Yellow;
        green.team = Team.Green;

        colliders = transform.Find("Board Colliders").GetOrAddComponent<ColliderEventHandler>();
        colliders.EventsCooldown = 0.025f;
        triggers = transform.Find("Win Trigger").GetOrAddComponent<TriggerEventHandler>();
        triggers.EventsCooldown = 0.025f;

        colliders.OnCollisionEnterEvent.AddListener(OnCollision);
        colliders.OnCollisionExitEvent.AddListener(OnCollisionLeave);

        triggers.OnTriggerEnterEvent.AddListener(OnTrigger);

        // This is an ugly solution to track all bags in the scene and their starting positions
        foreach (GameObject bag in GameObject.FindGameObjectsWithTag("Yellow"))
        {
            if (!cornholeBags.ContainsKey(bag))
            {
                cornholeBags.Add(bag, bag.transform.position);
            }
        }
        foreach (GameObject bag in GameObject.FindGameObjectsWithTag("Green"))
        {
            if (!cornholeBags.ContainsKey(bag))
            {
                cornholeBags.Add(bag, bag.transform.position);
            }
        }
    }

    private void OnCollision(Collision collision)
    {
        if (!isGameActive)
            return;

        GameObject bag = collision.gameObject;

        if (!bag.CompareTag("Yellow") && !bag.CompareTag("Green"))
            return;

        Debug.Log("Collision with " + bag.name);

        // Only award points if the bag isn't already scored
        if (!bagStates.ContainsKey(bag) || bagStates[bag] == BagState.None)
        {
            bagStates[bag] = BagState.OnBoard;
            AddScoreForBag(bag, 1);
        }
    }

    private void OnCollisionLeave(Collision collision)
    {
        if (!isGameActive)
            return;

        GameObject bag = collision.gameObject;

        if (!bag.CompareTag("Yellow") && !bag.CompareTag("Green"))
            return;

        Debug.Log("Collision Leave with " + bag.name);

        // Only remove points if the bag was on the board (not in hole)
        if (bagStates.ContainsKey(bag) && bagStates[bag] == BagState.OnBoard)
        {
            bagStates[bag] = BagState.None;
            AddScoreForBag(bag, -1);
        }
    }

    private void OnTrigger(Collider other)
    {
        if (!isGameActive)
            return;

        GameObject bag = other.gameObject;

        if (!bag.CompareTag("Yellow") && !bag.CompareTag("Green"))
            return;

        Debug.Log("Trigger Enter with " + bag.name);

        // Handle scoring based on current bag state
        if (!bagStates.ContainsKey(bag) || bagStates[bag] == BagState.None)
        {
            // Bag went directly into hole without touching board
            bagStates[bag] = BagState.InHole;
            AddScoreForBag(bag, 3);
        }
        else if (bagStates[bag] == BagState.OnBoard)
        {
            // Bag was on board and slid into hole
            // Remove the 1 point from being on board, add 3 for hole (net +2)
            bagStates[bag] = BagState.InHole;
            AddScoreForBag(bag, 2);
        }
        // If already in hole, do nothing (prevents double scoring)
    }

    private void AddScoreForBag(GameObject bag, int scoreChange)
    {
        if (!isGameActive)
            return;

        if (bag.CompareTag("Yellow"))
        {
            yellow.SetScore(yellow.score + scoreChange);
        }
        else if (bag.CompareTag("Green"))
        {
            green.SetScore(green.score + scoreChange);
        }
    }

    // Optional: Clean up bag states when bags are removed/reset
    public void ResetBagState(GameObject bag)
    {
        if (bagStates.ContainsKey(bag))
        {
            bagStates.Remove(bag);
        }
    }

    // Optional: Clear all bag states (useful for game reset)
    public void ClearAllBagStates()
    {
        bagStates.Clear();
        yellow.SetScore(0);
        green.SetScore(0);
    }

    public async void ResetGame()
    {
        isGameActive = false;

        ClearAllBagStates();

        await UniTask.Delay(150); // Make sure score text is updated before hiding (awaits previous SetScore calls)

        yellow.HideText();
        green.HideText();

        // Reset positions
        foreach (var bag in cornholeBags.Keys)
        {
            bag.transform.position = cornholeBags[bag];
        }

        await UniTask.Delay(3000); // Wait a moment before re-enabling scoring

        isGameActive = true;
    }
}
