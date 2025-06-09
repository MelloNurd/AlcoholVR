using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using static UnityEngine.ParticleSystem;
using Random = UnityEngine.Random;

[Serializable]
public enum NPCState
{
    Idle,
    Moving,
    PerformingAction
}

public class NPC : MonoBehaviour
{
    [SerializeField] protected bool _debugMode = false;
    [ReadOnly] public NPCState currentState = NPCState.Idle;

    protected Animator _animator;
    protected GameObject _bodyObj;
    protected NavMeshAgent _agent;

    [Header("Action Settings")]
    [SerializeField, Tooltip("If false, it will only pull Actions from the ActionController on the \"Body\" GameObject.")] protected bool _checkpointDeterminedActions = true;
    protected ActionContainer _actionContainer;

    [Header("Checkpoint Settings")]
    public UnityEvent OnCheckpointLeave = new();
    public UnityEvent OnCheckpointArrive = new();
    [SerializeField] protected AnimationClip _idleAnimation;
    [SerializeField] protected AnimationClip _moveAnimation;
    [SerializeField] protected SortMode _checkpointSortMode; // How it determines the next checkpoint to move to
    [ShowIf("_checkpointSortMode", SortMode.Random), SerializeField] protected bool _excludeCurrentCheckpoint = true; // If it should be able to stay at the same checkpoint
    protected List<ActionContainer> _checkPoints = new();
    protected int _currentCheckpointIndex = -1;

    // INTERACTION STUFF
    protected CancellationTokenSource _cancellationTokenSource;
    protected bool _isBeingInteractedWith = false;

    protected int _currentActionIndex = -1;
    protected bool _wasInterrupted = false;
    protected float _remainingDelay = 0;

    protected void Awake()
    {
        _bodyObj = transform.Find("Body").gameObject; // Assuming the NPC has a "Body" GameObject that contains the ActionContainer
        _actionContainer = _bodyObj.GetComponent<ActionContainer>(); // This acts as the universal ActionContainer, and is used when destinationDeterminedActions is false
        _animator = GetComponentInChildren<Animator>();
        _agent = GetComponentInChildren<NavMeshAgent>();
        _cancellationTokenSource = new CancellationTokenSource();

        // Initialize Checkpoints list
        foreach (Transform child in transform)
        {
            if(child.name == "Body") continue; // Skip universal one

            if (child.TryGetComponent(out ActionContainer container))
            {
                _checkPoints.Add(container);
            }
        }
        if(_checkPoints.Count <= 0 && _checkpointDeterminedActions)
        {
            Debug.Log("No checkpoints found for \"" + gameObject.name + "\", setting _checkpointDeterminedActions to false.");
            _checkpointDeterminedActions = false;
        }
    }

    protected void Start()
    {
        StartAtFirstCheckpoint();
        RunNPCLoop();
    }

    protected async void RunNPCLoop()
    {
        while(true)
        {
            await UniTask.WaitUntil(() => !_isBeingInteractedWith);

            if (_wasInterrupted && _currentActionIndex >= 0)
            {
                await ProcessActions(); // Resume actions
            }
            else
            {
                await ProcessActions();
                await UniTask.WaitUntil(() => !_isBeingInteractedWith);
                await GoToNextCheckpoint();
                await UniTask.WaitUntil(() => !_isBeingInteractedWith);
            }
        }
    }
    protected Vector3 GetNextCheckpoint()
    {
        if (_checkPoints.Count == 0) return _bodyObj.transform.position; // No checkpoints, stay in place

        int index = -1;
        switch(_checkpointSortMode)
        {
            case SortMode.Random:
                do index = Random.Range(0, _checkPoints.Count);
                while(_excludeCurrentCheckpoint && _currentCheckpointIndex == index);
                break;
            case SortMode.RoundRobin:
                index = (_currentCheckpointIndex + 1) % _checkPoints.Count;
                break;
            case SortMode.RoundRobinReverse:
                index = (_currentCheckpointIndex - 1 + _checkPoints.Count) % _checkPoints.Count;
                break;
        }
        _currentCheckpointIndex = index;
        return _checkPoints[_currentCheckpointIndex].transform.position;
    }
    protected void StartAtFirstCheckpoint()
    {
        // Default to current position, then try to set to a checkpoint position
        Vector3 checkPointPos = _bodyObj.transform.position;
        if(_checkPoints.Count > 0)
        {
            _currentCheckpointIndex = _checkpointSortMode switch
            {
                SortMode.Random => Random.Range(0, _checkPoints.Count),
                SortMode.RoundRobin => 0,
                SortMode.RoundRobinReverse => _checkPoints.Count - 1,
                _ => -1
            };

            checkPointPos = _checkPoints[_currentCheckpointIndex].transform.position;
        }

        // raycast from pos downwards to find groundlevel
        if (Physics.Raycast(checkPointPos, Vector3.down, out RaycastHit hit, Mathf.Infinity)) // Assuming "Ground" layer is used for ground
        {
            checkPointPos = hit.point;
            checkPointPos.y += _bodyObj.GetComponent<CapsuleCollider>().height * 0.5f; // Adjust for the height of the NPC
        }

        _bodyObj.transform.position = checkPointPos;
        _agent.SetDestination(checkPointPos);
    }
    protected async UniTask GoToNextCheckpoint()
    {
        await UniTask.WaitUntil(() => !_isBeingInteractedWith);

        OnCheckpointLeave?.Invoke();
        if (_checkPoints.Count > 0)
        {
            Vector3 nextCheckpoint = GetNextCheckpoint();
            _agent.SetDestination(nextCheckpoint);

            StartMoving();

            await UniTask.WaitUntil(() => _agent.remainingDistance <= _agent.stoppingDistance || _cancellationTokenSource.IsCancellationRequested);
            await UniTask.WaitUntil(() => !_isBeingInteractedWith);
        }
        OnCheckpointArrive?.Invoke();

        StartIdling();
    }


    protected async UniTask ProcessActions()
    {
        ActionContainer container = !_checkpointDeterminedActions ? _actionContainer : _checkPoints[_currentCheckpointIndex];
        int numberOfActionsToPlay = Random.Range(container.minActions, container.maxActions + 1);
        int seconds = Mathf.RoundToInt(container.AnimationDelay * 1000); // Convert to milliseconds for async delay

        // Resume from where we left off if interrupted
        int startIndex = _wasInterrupted ? _currentActionIndex : 0;
        _wasInterrupted = false;
        if (_remainingDelay > 0 && startIndex > 0) // Handle initial delay if resuming
        {
            await UniTask.Delay(Mathf.RoundToInt(_remainingDelay), cancellationToken: _cancellationTokenSource.Token).SuppressCancellationThrow();
            _remainingDelay = 0;
        }

        for (int i = 0; i < numberOfActionsToPlay; i++)
        {
            _currentActionIndex = i; // Track current action index
            float startTime = Time.time;

            await UniTask.Delay(seconds, cancellationToken: _cancellationTokenSource.Token).SuppressCancellationThrow();

            if (_isBeingInteractedWith)
            {
                _remainingDelay = Mathf.Max(0, seconds - ((Time.time - startTime) * 1000));
                _wasInterrupted = true;
                return;
            }

            await UniTask.WaitUntil(() => !_isBeingInteractedWith);

            Action temp = container.GetAction(out int durationMS);
            if (temp.animToPlay != null)
            {
                PlayAnimation(temp.animToPlay.name); // Get the next action and its duration
                currentState = NPCState.PerformingAction;
            }

            temp.OnActionStart?.Invoke();

            startTime = Time.time;
            await UniTask.Delay(durationMS, cancellationToken: _cancellationTokenSource.Token).SuppressCancellationThrow();

            if (_isBeingInteractedWith)
            {
                _remainingDelay = Mathf.Max(0, durationMS - ((Time.time - startTime) * 1000));
                _wasInterrupted = true;
                return;
            }

            await UniTask.WaitUntil(() => !_isBeingInteractedWith);

            temp.OnActionEnd?.Invoke();

            StartIdling();
        }

        _currentActionIndex = -1;
        await UniTask.Delay(seconds, cancellationToken: _cancellationTokenSource.Token).SuppressCancellationThrow();
        await UniTask.WaitUntil(() => !_isBeingInteractedWith);
    }

    private void PlayAnimation(string name)
    {
        _animator.CrossFadeInFixedTime(name, 0.2f); // Play the animation with a crossfade
    }

    protected void StartMoving()
    {
        currentState = NPCState.Moving;
        PlayAnimation(_moveAnimation.name);
    }
    protected void StartIdling()
    {
        currentState = NPCState.Idle;
        PlayAnimation(_idleAnimation.name);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && _debugMode)
        {
            Gizmos.color = Color.green;

            if (_agent.path.corners.Length > 0) // This is drawing the actual path the agent is taking
            {
                for (int i = 0; i < _agent.path.corners.Length - 1; i++)
                {
                    Gizmos.DrawLine(_agent.path.corners[i], _agent.path.corners[i + 1]);
                }
            }

            Gizmos.DrawSphere(_agent.destination, 0.2f);
        }
    }
}
