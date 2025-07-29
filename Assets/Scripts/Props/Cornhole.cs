using System;
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

        public async void AddScore(int scoreToAdd = 1)
        {
            score += scoreToAdd;

            await UniTask.Delay(250); // small delay to avoid flickering text if multiple collisions happen quickly

            if (scoreText != null) scoreText.text = score.ToString();
        }
    }

    private ColliderEventHandler colliders;
    private TriggerEventHandler triggers;

    [SerializeField] private CornholeScore yellow;
    [SerializeField] private CornholeScore green;

    private void Awake()
    {
        yellow.team = Team.Yellow;
        green.team = Team.Green;

        colliders = transform.Find("Board Colliders").GetOrAddComponent<ColliderEventHandler>();
        colliders.CollisionCooldown = 0.025f;
        triggers = transform.Find("Win Trigger").GetOrAddComponent<TriggerEventHandler>();
        triggers.TriggerCooldown = 0.025f;

        colliders.OnCollisionEnterEvent.AddListener(OnCollision);
        colliders.OnCollisionExitEvent.AddListener(OnCollisionLeave);

        triggers.OnTriggerEnterEvent.AddListener(OnTrigger);
    }

    private void OnCollision(Collision collision)
    {
        Debug.Log("Collision with " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Yellow"))
        {
            yellow.AddScore(1);
        }
        else if (collision.gameObject.CompareTag("Green"))
        {
            green.AddScore(1);
        }
    }

    private void OnCollisionLeave(Collision collision)
    {
        Debug.Log("Collision Leave with " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Yellow"))
        {
            yellow.AddScore(-1);
        }
        else if (collision.gameObject.CompareTag("Green"))
        {
            green.AddScore(-1);
        }
    }

    private void OnTrigger(Collider other)
    {
        Debug.Log("Trigger Enter with " + other.gameObject.name);
        if (other.gameObject.CompareTag("Yellow"))
        {
            yellow.AddScore(3);
        }
        else if (other.gameObject.CompareTag("Green"))
        {
            green.AddScore(3);
        }
    }
}
