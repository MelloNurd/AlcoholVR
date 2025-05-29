using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[Serializable]
public enum NPCState
{
    Idle,
    Moving,
    PerformingAction
}

public abstract class NPC : MonoBehaviour
{
    [SerializeField] private bool _debugMode = false;
    [ReadOnly] public NPCState currentState = NPCState.Idle;
    
    private Animator _animator;
    private GameObject _bodyObj;
    private NavMeshAgent _agent;

    [Header("Action Settings")]
    [SerializeField, Tooltip("If false, it will only pull Actions from the ActionController on the \"Body\" GameObject.")] private bool _checkpointDeterminedActions = true;
    private ActionContainer _actionContainer;

    [Header("Checkpoint Settings")]
    public UnityEvent OnCheckpointLeave = new();
    public UnityEvent OnCheckpointArrive = new();
    [SerializeField] private AnimationClip _idleAnimation;
    [SerializeField] private AnimationClip _moveAnimation;
    [SerializeField] private SortMode _checkpointSortMode; // How it determines the next checkpoint to move to
    [ShowIf("_checkpointSortMode", SortMode.Random), SerializeField] private bool _excludeCurrentCheckpoint = true; // If it should be able to stay at the same checkpoint
    private List<ActionContainer> _checkPoints = new();
    private int _currentCheckpointIndex = -1;

    private void Awake()
    {
        _bodyObj = transform.Find("Body").gameObject; // Assuming the NPC has a "Body" GameObject that contains the ActionContainer
        _actionContainer = _bodyObj.GetComponent<ActionContainer>(); // This acts as the universal ActionContainer, and is used when destinationDeterminedActions is false
        _animator = GetComponentInChildren<Animator>();
        _agent = GetComponentInChildren<NavMeshAgent>();

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

    private void Start()
    {
        StartAtFirstCheckpoint();
        RunNPCLoop();
    }

    private async void RunNPCLoop()
    {
        while(true)
        {
            await ProcessActions();
            await GoToNextCheckpoint();
        }
    }

    private void StartAtFirstCheckpoint()
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
    private async UniTask GoToNextCheckpoint()
    {
        OnCheckpointLeave?.Invoke();
        if (_checkPoints.Count > 0)
        {
            Vector3 nextCheckpoint = GetNextCheckpoint();
            _agent.SetDestination(nextCheckpoint);

            StartMoving();

            await UniTask.WaitUntil(() => _agent.remainingDistance <= _agent.stoppingDistance);
        }
        OnCheckpointArrive?.Invoke();

        StartIdling();
    }
    private Vector3 GetNextCheckpoint()
    {
        if (_checkPoints.Count == 0) return _bodyObj.transform.position; // No checkpoints, stay in place

        int index = -1;
        switch(_checkpointSortMode)
        {
            case SortMode.Random:
                Debug.Log("Random CHECKPOINT");
                do index = Random.Range(0, _checkPoints.Count);
                while(_excludeCurrentCheckpoint && _currentCheckpointIndex == index);
                break;
            case SortMode.RoundRobin:
                Debug.Log("RR CHECKPOINT");
                index = (_currentCheckpointIndex + 1) % _checkPoints.Count;
                break;
            case SortMode.RoundRobinReverse:
                Debug.Log("RRR CHECKPOINT");
                index = (_currentCheckpointIndex - 1 + _checkPoints.Count) % _checkPoints.Count;
                break;
        }
        _currentCheckpointIndex = index;
        return _checkPoints[_currentCheckpointIndex].transform.position;
    }

    private async UniTask ProcessActions()
    {
        ActionContainer container = !_checkpointDeterminedActions ? _actionContainer : _checkPoints[_currentCheckpointIndex];

        int numberOfActionsToPlay = Random.Range(container.minActions, container.maxActions + 1);

        int seconds = Mathf.RoundToInt(container.AnimationDelay * 1000); // Convert to milliseconds for async delay

        for (int i = 0; i < numberOfActionsToPlay; i++)
        {
            await UniTask.Delay(seconds);

            Action temp = container.GetAction(out int durationMS);
            if (temp.animToPlay != null)
            {
                _animator.Play(temp.animToPlay.name); // Get the next action and its duration
                currentState = NPCState.PerformingAction;
            }

            temp.OnActionStart?.Invoke();
            await UniTask.Delay(durationMS); // Wait for the action to complete
            temp.OnActionEnd?.Invoke();

            StartIdling();
        }

        await UniTask.Delay(seconds);
    }

    private void StartMoving()
    {
        currentState = NPCState.Moving;

        if (_moveAnimation != null)
        {
            _animator.Play(_moveAnimation.name); // Play the move animation
        }
    }
    private void StartIdling()
    {
        currentState = NPCState.Idle;

        if (_idleAnimation != null)
        {
            _animator.Play(_idleAnimation.name); // Play the idle animation
        }
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
