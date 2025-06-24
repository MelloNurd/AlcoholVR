using System;
using System.Linq;
using EditorAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct ScoringCollider
{
    public Collider Collider;

    [Space(10)]
    public float OnEnterScoreValue;
    public float OnLeaveScoreValue;

    [Space(10)]
    public bool OverrideScoreCooldown;
    [ShowField(nameof(OverrideScoreCooldown)), Min(0)] public float ScoringCooldown;

    [Space(10)]
    public UnityEvent OnEnterEvent;
    public UnityEvent OnLeaveEvent;
}

[SelectionBase]
public class Minigame : MonoBehaviour
{
    [Serializable]
    private struct ScoreEvent
    {
        public UnityEvent Event;
        public float ScoreValue;
    }

    [SerializeField] private ScoringCollider[] _triggeredColliders;
    [Tooltip("The layers that can interact with the colliders")] public LayerMask LayerMask = -1;

    public bool CanScore = true;

    [SerializeField] private float _startingScore;
    [ReadOnly] public float currentScore;

    [SerializeField] private float _scoreCooldown = 0f;
    private float _cooldownTimer;

    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private bool _showTextOnlyAfterFirstScore = true;

    [SerializeField] private ScoreEvent[] _scoreEvents;
    public UnityEvent OnScoreIncrement;
    public UnityEvent OnScoreDecrement;

    void Awake()
    {
        if(_scoreText != null) _scoreText.text = _showTextOnlyAfterFirstScore ? "" : currentScore.ToString();

        foreach(var scoringCollider in _triggeredColliders)
        {
             MinigameCollider temp = scoringCollider.Collider.gameObject.AddComponent<MinigameCollider>();
            temp.Initialize(this, scoringCollider);
        }
    }

    private void Update()
    {
        _cooldownTimer -= Time.deltaTime;
        if (_cooldownTimer < 0f && !CanScore)
        {
            CanScore = true;
        }
    }

    public void StartCooldown() => StartCooldown(_scoreCooldown);
    public void StartCooldown(float length)
    {
        CanScore = false;
        _cooldownTimer = length;
    }

    public void AddScore(float increment)
    {
        if(!CanScore) return;

        Debug.Log($"Adding {increment} to score. Current score: {currentScore}, new score: {currentScore+increment}");

        currentScore += increment;
        if(_scoreText)
        {
            _scoreText.text = currentScore.ToString();
        }

        if (increment < 0f)
        {
            OnScoreDecrement?.Invoke();
        }
        else
        {
            OnScoreIncrement?.Invoke();
        }
        foreach (var scoreEvent in _scoreEvents.Where(x => x.ScoreValue == currentScore))
        {
            scoreEvent.Event?.Invoke();
        }
    }
}
