using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[SelectionBase]
public class NPCStateMachine : MonoBehaviour
{
    public enum States
    {
        Idle,
        Walk,
        Checkpoint,
        Interact
    }

    private GameObject _bodyObj;
    public Vector3 bodyPosition => _bodyObj.transform.position;
    [SerializeField] private AnimationClip _idleAnimation;
    [SerializeField] private AnimationClip _moveAnimation;

    [Header("Checkpoint Settings")]
    [SerializeField] protected SortMode _checkpointSortMode; // How it determines the next checkpoint to move to
    public UnityEvent OnCheckpointLeave = new();
    public UnityEvent OnCheckpointArrive = new();
    [SerializeField, ReadOnly, Label("Current State")] private string _currentStateName; // This is really just for debugging
    [HideInInspector] public ActionContainer currentCheckpoint;
    [HideInInspector] public int actionsLeft = 0;
    private List<ActionContainer> _checkpoints = new();
    [ShowIf("_checkpointSortMode", SortMode.Random), SerializeField] protected bool _alwaysUnique = true; // If it should be able to stay at the same checkpoint
    protected int _currentCheckpointIndex = -1;

    [Header("Interaction Settings")]
    public bool isInteractable = false;
    public Dialogue _firstDialogue;
    public Dialogue _incompleteDialogue;
    public Dialogue _completeDialogue;
    public Dialogue _failDialogue;
    public UnityEvent OnFirstInteraction = new();
    public UnityEvent OnCompleteInteraction = new();
    public UnityEvent OnIncompleteInteraction = new();
    public UnityEvent OnFailInteraction = new();
    public int interactionCount = 0;
    public DialogueSystem dialogueSystem;

    [Header("Quest Settings")]
    public bool _hasQuest = false;
    [ShowIf("_hasQuest")] public Quest Quest;

    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public Animator animator;
    private NPCBaseState _currentState;
    private ActionContainer _selfActionContainer; // This is used when there are no checkpoints

    private Dictionary<States, NPCBaseState> _states = new();
    private AudioSource _audioSource;

    private GameObject _playerObj;
    public Vector3 playerPosition => _playerObj.transform.position; 

    public virtual void Awake()
    {
        _bodyObj = transform.Find("Body").gameObject;
        _playerObj = GameObject.Find("Main Camera"); // This may need changed later
        animator = GetComponentInChildren<Animator>();
        agent = GetComponentInChildren<NavMeshAgent>();
        agent.updateRotation = false;
        _audioSource = GetComponentInChildren<AudioSource>();
        _selfActionContainer = _bodyObj.GetComponent<ActionContainer>(); // This acts as the universal ActionContainer, and is used when destinationDeterminedActions is false

        if(_firstDialogue != null)
        {
            if (_incompleteDialogue == null) _incompleteDialogue = _firstDialogue;
            if (_completeDialogue == null) _completeDialogue = _firstDialogue;
            if (_failDialogue == null) _failDialogue = _firstDialogue;
        }
        else if(isInteractable)
        {
            Debug.LogError("First dialogue is not set for " + gameObject.name + ". Please assign a dialogue.");
        }

        foreach (Transform child in transform)
        {
            if (child.name == "Body") continue; // Skip universal one

            if (child.TryGetComponent(out ActionContainer container))
            {
                _checkpoints.Add(container);
            }
        }
        StartAtFirstCheckpoint();

        // Initialize states
        _states.Add(States.Idle, new NPCIdleState(this));
        _states.Add(States.Walk, new NPCWalkState(this));
        _states.Add(States.Checkpoint, new NPCCheckpointState(this));
        _states.Add(States.Interact, new NPCInteractState(this));

        // Start in idle state
        SwitchState(States.Walk);
    }

    public virtual void Update()
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
        _currentState?.UpdateState();
    }

    public void SwitchState(States newState)
    {
        _currentState?.ExitState();
        _currentState = _states[newState];
        _currentState?.EnterState();
        _currentStateName = _states[newState].GetType().Name;
    }

    public void PlayAnimation(string animationName)
    {
        animator.CrossFadeInFixedTime(animationName, 0.4f);
    }
    public void PlayIdleAnimation() => PlayAnimation(_idleAnimation.name);
    public void PlayWalkAnimation() => PlayAnimation(_moveAnimation.name);
    public int PlayNextAction()
    {
        Action temp = currentCheckpoint.GetNextAction(out int lengthMS);
        if(temp != null)
        {
            PlayAnimation(temp.animToPlay.name);
        }
        return lengthMS;
    }

    public ActionContainer SetNextCheckpoint()
    {
        if (_checkpoints.Count == 0) return _selfActionContainer; // No checkpoints, use self

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

        currentCheckpoint = _checkpoints[_currentCheckpointIndex];
        return currentCheckpoint;
    }

    protected void StartAtFirstCheckpoint()
    {
        Vector3 checkPointPos = SetNextCheckpoint().transform.position;

        // raycast from pos downwards to find ground level
        if (Physics.Raycast(checkPointPos, Vector3.down, out RaycastHit hit, Mathf.Infinity))
        {
            checkPointPos = hit.point;
            checkPointPos.y += _bodyObj.GetComponent<CapsuleCollider>().height * 0.5f; // Adjust for the height of the NPC
        }

        _bodyObj.transform.position = checkPointPos;
        agent.SetDestination(checkPointPos);
    }

    [Button("Execute Interact", EButtonEnableMode.Playmode)]
    public void Interact()
    {
        if(_currentState == _states[States.Interact]) // if already in dialogue, exit it
        {
            SwitchState(States.Walk); // Walk will auto switch to checkpoint if already there, so this is kind of the same as resuming
        }
        else
        {
            SwitchState(States.Interact);
        }
    }
}
