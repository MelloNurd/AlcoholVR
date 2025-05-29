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

public abstract class NPC : MonoBehaviour
{
    [Serializable]
    private enum NPCState
    {
        Idle,
        Moving,
        PerformingAction
    }

    [SerializeField] private bool _debugMode = false;
    [SerializeField, ReadOnly] private NPCState _currentState = NPCState.Idle;
    
    private Animator _animator;
    private GameObject _bodyObj;
    private NavMeshAgent _agent;

    [Header("Action Settings")]
    [SerializeField, Tooltip("If false, it will only pull Actions from the ActionController on the \"Body\" GameObject.")] private bool _checkpointDeterminedActions = true;
    private ActionContainer _actionContainer;


    [Header("Checkpoint Settings")]
    [SerializeField] private AnimationClip _idleAnimation;
    [SerializeField] private AnimationClip _moveAnimation;
    [SerializeField] private SortMode _checkpointSortMode; // How it determines the next checkpoint to move to
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
            await UniTask.Delay(5000);
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
        // invoke before moving to checkpoint event
        if (_checkPoints.Count > 0)
        {
            Vector3 nextCheckpoint = GetNextCheckpoint();
            _agent.SetDestination(nextCheckpoint);

            StartMoving();

            await UniTask.WaitUntil(() => _agent.remainingDistance <= _agent.stoppingDistance);
        }
        // invoke after moving to checkpoint event

        StartIdling();
    }
    private Vector3 GetNextCheckpoint()
    {
        if (_checkPoints.Count == 0) return _bodyObj.transform.position; // No checkpoints, stay in place

        _currentCheckpointIndex = _checkpointSortMode switch
        {
            SortMode.Random => Random.Range(0, _checkPoints.Count),
            SortMode.RoundRobin => (_currentCheckpointIndex + 1) % _checkPoints.Count,
            SortMode.RoundRobinReverse => (_currentCheckpointIndex - 1 + _checkPoints.Count) % _checkPoints.Count,
            _ => -1
        };

        return _checkPoints[_currentCheckpointIndex].transform.position;
    }

    

    private void StartMoving()
    {
        if (_debugMode)
        {
            Debug.Log("Starting to move...");
        }

        _currentState = NPCState.Moving;

        if (_moveAnimation != null)
        {
            _animator.Play(_moveAnimation.name); // Play the move animation
        }
    }
    private void StartIdling()
    {
        if (_debugMode)
        {
            Debug.Log("Starting to idle...");
        }

        _currentState = NPCState.Idle;

        if (_idleAnimation != null)
        {
            _animator.Play(_idleAnimation.name); // Play the idle animation
        }
    }

    // Consider coroutines for better cancellation
    private async void ProcessActions()
    {
        int numberOfActionsToPlay = Random.Range(_actionContainer.minActions, _actionContainer.maxActions + 1);

        // Wait some number of time
        // play some action
        // wiat more time
        // wait action
        // wait more time
        // finish

        // Play actions x times with y seconds in between, y being the one enum variable
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

    // Basically, should have abilities to walk between points, play animations based on min/max (random), and have a delay between actions
    // This should be super modular. Use as many events as possible. Also have buttons so we can like force skip an action or something.
    // Figure out how to use async or IEnumerators to wait for the things
}
