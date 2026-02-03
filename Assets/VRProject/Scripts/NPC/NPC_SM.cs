using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UIElements;

[SelectionBase]
public class NPC_SM : MonoBehaviour // SM = State Machine
{
    public enum States
    {
        Idle,
        Walk,
        Checkpoint,
        Interact
    }

    [HideInInspector] public GameObject bodyObj;

    public bool isDrunk = false;
    public bool alwaysUseStartAnim = false;

    [HelpBox("If this is assigned, this animation will play on start instead of the default idle animation.")]
    [SerializeField] public AnimationClip startAnim;

    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public Animator animator;
    public NPC_BaseState currentState;
    protected ActionContainer _selfActionContainer; // This is used when there are no checkpoints

    public Dictionary<States, NPC_BaseState> states = new();
    protected AudioSource _audioSource;

    [Header("Checkpoint Settings")]
    [SerializeField] public bool usesCheckpoints = true;
    [SerializeField] protected SortMode _checkpointSortMode; // How it determines the next checkpoint to move to
    public UnityEvent OnCheckpointLeave = new();
    public UnityEvent OnCheckpointArrive = new();
    [HideInInspector] public (ActionContainer container, Transform transform) currentCheckpoint;
    [HideInInspector] public int actionsLeft = 0;
    protected List<(ActionContainer container, Transform transform)> _checkpoints = new();
    [ShowField(nameof(_checkpointSortMode), SortMode.Random), SerializeField] protected bool _alwaysUnique = true; // If it should be able to stay at the same checkpoint
    protected int _currentCheckpointIndex = -1;

    [SerializeField, ReadOnly, Rename("Current State (Debug)")] protected string _currentStateName;

    [HideInInspector] public LookAt lookAt;
    [HideInInspector] public GameObject lookAtPoint;

    protected async void Awake()
    {
        bodyObj = transform.Find("Body").gameObject;
        animator = GetComponentInChildren<Animator>();
        agent = GetComponentInChildren<NavMeshAgent>();
        lookAt = GetComponentInChildren<LookAt>();

        agent.updateRotation = false;
        _audioSource = GetComponentInChildren<AudioSource>();
        _selfActionContainer = bodyObj.GetComponent<ActionContainer>(); // This acts as the universal ActionContainer, and is used when destinationDeterminedActions is false

        lookAtPoint = new GameObject("LookAtPoint");
        lookAtPoint.transform.SetParent(transform);
        lookAt.objectToLookAt = lookAtPoint.transform;

        foreach (Transform child in transform)
        {
            if (child.name == "Body") continue; // Skip universal one

            if (child.TryGetComponent(out ActionContainer container))
            {
                _checkpoints.Add((container, container.transform));
            }
        }
        if(_checkpoints.Count == 0)
        {
            _checkpoints.Add((_selfActionContainer, transform));
        }

        // Initialize states dictionary
        states.Add(States.Idle, new NPC_IdleState(this));
        states.Add(States.Walk, new NPC_WalkState(this));
        states.Add(States.Checkpoint, new NPC_CheckpointState(this));
        states.Add(States.Interact, new NPC_InteractState(this));

        if (agent == null || agent.enabled == false)
        {
            SwitchState(States.Idle);
            if (startAnim != null)
            {
                await UniTask.Delay(200);
                PlayAnimation(startAnim.name);
                Debug.Log("playing start anim");
            }
            else
            {
                PlayIdleAnimation();
            }
            return;
        }
        else
        {
            StartAtFirstCheckpoint();
            SwitchState(States.Walk);
        }
    }

    protected virtual void Update()
    {
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(agent.velocity.normalized);
            agent.transform.rotation = Quaternion.RotateTowards(
                agent.transform.rotation,
                targetRot,
                Time.deltaTime * agent.angularSpeed // turn speed
            );
        }
        currentState?.UpdateState();
    }

    public bool IsInState(States state)
    {
        return currentState == states[state];
    }

    public void SwitchState(States newState)
    {
        currentState?.ExitState();
        currentState = states[newState];
        currentState?.EnterState();
        _currentStateName = states[newState].GetType().Name;
    }

    public void PlayAnimation(string animationName)
    {
        animator.SetBool("isDrunk", isDrunk);

        animator.CrossFade(animationName, 0.1f);
    }

    public void PlayIdleAnimation()
    {
        animator.SetBool("isDrunk", isDrunk);

        animator.SetTrigger("Start Idle");
        animator.SetBool("isWalk", false);
    }

    public void PlayWalkAnimation()
    {
        animator.SetBool("isDrunk", isDrunk);

        animator.SetTrigger("Start Idle");
        animator.SetBool("isWalk", true);
    }

    public int PlayNextAction()
    {
        Action temp = currentCheckpoint.container.GetNextAction(out int lengthMS);
        if(temp != null)
        {
            PlayAnimation(temp.animToPlay.name);
        }
        return lengthMS;
    }

    public (ActionContainer container, Transform transform) SetNextCheckpoint()
    {
        if (_checkpoints.Count == 0) return (_selfActionContainer, transform); // No checkpoints, use self

        int index = -1;
        switch (_checkpointSortMode)
        {
            case SortMode.Random:
                do index = Random.Range(0, _checkpoints.Count);
                while (_alwaysUnique && _currentCheckpointIndex == index);
                break;
            case SortMode.RoundRobin:
                index = (_currentCheckpointIndex + 1) % _checkpoints.Count;
                break;
            case SortMode.RoundRobinReverse:
                index = (_currentCheckpointIndex - 1 + _checkpoints.Count) % _checkpoints.Count;
                break;
        }
        _currentCheckpointIndex = index;

        currentCheckpoint = (_checkpoints[_currentCheckpointIndex].container, _checkpoints[_currentCheckpointIndex].container.transform);
        return currentCheckpoint;
    }

    protected void StartAtFirstCheckpoint()
    {
        Vector3 checkPointPos = SetNextCheckpoint().transform.position;

        // raycast from pos downwards to find ground level
        if (Physics.Raycast(checkPointPos, Vector3.down, out RaycastHit hit, Mathf.Infinity))
        {
            checkPointPos = hit.point;
            checkPointPos.y += bodyObj.GetComponent<NavMeshAgent>().height * 0.5f; // Adjust for the height of the NPC
        }

        bodyObj.transform.position = checkPointPos;
        agent.SetDestination(checkPointPos);
    }

    public void ManualCheckpointSet(Transform transform)
    {
        currentCheckpoint = (null, transform);
        SwitchState(States.Walk);
        OnCheckpointLeave?.Invoke();
    }
    public void ManualCheckpointSet(ActionContainer container)
    {
        currentCheckpoint = (container, container.transform);
        SwitchState(States.Walk);
        OnCheckpointLeave?.Invoke();
    }
}
