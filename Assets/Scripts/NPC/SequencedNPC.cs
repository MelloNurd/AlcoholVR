using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using PrimeTween;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI;
using Void = EditorAttributes.Void;

[Serializable]
public class Sequence
{
    public enum SequenceType
    {
        Animate,
        Dialogue,
        Walk,
        WalkToPlayer,
        Wait,
    }

    [Header("Sequence Settings")]
    public SequenceType type;

    [ShowField(nameof(type), SequenceType.Animate)] public AnimationClip animation;

    [ShowField(nameof(type), SequenceType.Dialogue)] public DialogueTree dialogue;

    [ShowField(nameof(type), SequenceType.Walk)] public Transform destination;
    [ConditionalEnumField(ConditionType.OR, nameof(type), SequenceType.Walk, nameof(type), SequenceType.WalkToPlayer)] 
    public bool useDefaultWalkAnimation = true;
    [ConditionalEnumField(ConditionType.OR, nameof(type), SequenceType.Walk, nameof(type), SequenceType.WalkToPlayer), HideField(nameof(useDefaultWalkAnimation))] 
    public AnimationClip walkAnimation;

    [ShowField(nameof(type), SequenceType.Wait)] public float secondsToWait;
    [Space]
    public bool nextSequenceOnEnd;

    [FoldoutGroup("Events", nameof(onSequenceStart), nameof(onSequenceEnd))]
    [SerializeField] private Void groupHolder;

    [HideInInspector] public UnityEvent onSequenceStart = new();
    [HideInInspector] public UnityEvent onSequenceEnd = new();
}

[SelectionBase]
public class SequencedNPC : MonoBehaviour
{
    public List<Sequence> sequences = new List<Sequence>();
    private Sequence currentSequence;

    [SerializeField, Required] private AnimationClip defaultAnimation;
    [SerializeField, Required] private AnimationClip walkAnimation;

    public bool wrapAroundSequences = false; // If it should loop through the sequences or stop at the end

    [ButtonField(nameof(StartNextSequence)), DisableInEditMode, SerializeField] private Void startNextSequenceButton;

    [HideInInspector] public GameObject bodyObj;
    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public Animator animator;
    private AudioSource _audioSource;
    private GameObject _playerObj;
    private DialogueSystem dialogueSystem;

    private CancellationTokenSource _cancelToken;
    private bool _isAtDestination = true;

    private Vector3 _lastDestinationPosition;

    private void Awake()
    {
        bodyObj = transform.Find("Body").gameObject;
        _playerObj = GameObject.Find("Main Camera"); // This may need changed later
        animator = GetComponentInChildren<Animator>();
        agent = GetComponentInChildren<NavMeshAgent>();
        _audioSource = GetComponentInChildren<AudioSource>();
        dialogueSystem = GetComponent<DialogueSystem>();
    }

    private void Start()
    {
        if(sequences.Count == 0)
        {
            Debug.LogWarning("No sequences assigned to " + gameObject.name + ". Please assign at least one sequence.");
            return;
        }

        StartSequence(0);
    }

    private void Update()
    {
        ApplyRotations();
        RunWalkSequences();
    }

    private void RunWalkSequences()
    {
        if (!_isAtDestination && (currentSequence.type == Sequence.SequenceType.Walk || currentSequence.type == Sequence.SequenceType.WalkToPlayer))
        {
            bool isWalkToPlayer = currentSequence.type == Sequence.SequenceType.WalkToPlayer;

            if (isWalkToPlayer)
            {

                Vector3 inFrontOfPlayer = Player.Instance.Position + Player.Instance.playerCamera.transform.forward.WithY(0).normalized;
                if (_lastDestinationPosition != inFrontOfPlayer)
                {
                    agent.SetDestinationToClosestPoint(inFrontOfPlayer);
                    _lastDestinationPosition = inFrontOfPlayer;
                }
            }

            if (agent.IsAtDestination())
            {
                Debug.Log($"{gameObject.name} has reached {(isWalkToPlayer ? "the player" : "its destination")}.");
                _isAtDestination = true;
                animator.CrossFadeInFixedTime(defaultAnimation.name, 0.4f);
                if (currentSequence.nextSequenceOnEnd) StartNextSequence();
            }
        }
    }

    private async void HandleSequence(Sequence sequence)
    {
        if(_cancelToken != null)
        {
            _cancelToken.Dispose();
        }
        _cancelToken = new CancellationTokenSource();

        switch (sequence.type)
        {
            case Sequence.SequenceType.Animate:
                animator.CrossFadeInFixedTime(sequence.animation.name, 0.4f);
                if(sequence.nextSequenceOnEnd)
                {
                    Debug.Log($"Waiting {sequence.animation.length} seconds");
                    await UniTask.Delay(Mathf.RoundToInt(sequence.animation.length * 1000), cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
                    if (_cancelToken.IsCancellationRequested) break;
                    animator.CrossFadeInFixedTime(defaultAnimation.name, 0.4f);
                    StartNextSequence();
                }
                
                break;
            case Sequence.SequenceType.Dialogue:
                if(dialogueSystem.currentTree != null)
                {
                    dialogueSystem.currentTree.onDialogueEnd.RemoveListener(async () => {
                        await UniTask.Delay(250);
                        StartNextSequence();
                    });
                    dialogueSystem.EndCurrentDialogue();
                }

                Vector3 directionToPlayer = (_playerObj.transform.position - bodyObj.transform.position).WithY(0);
                await Tween.LocalRotation(bodyObj.transform, Quaternion.LookRotation(directionToPlayer), 0.3f);
                dialogueSystem.BeginDialogueTree(sequence.dialogue);
                if(sequence.nextSequenceOnEnd)
                {
                    dialogueSystem.currentTree.onDialogueEnd.AddListener(async () => {
                        await UniTask.Delay(250);
                        StartNextSequence();
                    });
                }

                break;
            case Sequence.SequenceType.Walk:
                _isAtDestination = false;
                agent.SetDestinationToClosestPoint(sequence.destination.transform.position);

                string walkAnimName = (sequence.useDefaultWalkAnimation && sequence.walkAnimation != null) 
                    ? sequence.walkAnimation.name 
                    : walkAnimation.name;

                animator.CrossFadeInFixedTime(walkAnimName, 0.4f);

                break;
            case Sequence.SequenceType.WalkToPlayer:
                _isAtDestination = false;
                agent.SetDestinationToClosestPoint(Player.Instance.Position + Player.Instance.playerCamera.transform.forward.WithY(0).normalized);

                walkAnimName = (sequence.useDefaultWalkAnimation && sequence.walkAnimation != null)
                    ? sequence.walkAnimation.name
                    : walkAnimation.name;

                animator.CrossFadeInFixedTime(walkAnimName, 0.4f);

                break;
            case Sequence.SequenceType.Wait:
                animator.CrossFadeInFixedTime(defaultAnimation.name, 0.4f);
                await UniTask.Delay(Mathf.RoundToInt(sequence.secondsToWait * 1000), cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
                if (!_cancelToken.IsCancellationRequested && sequence.nextSequenceOnEnd)
                {
                    StartNextSequence();
                }

                break;
        }
    }

    public void StartNextSequence()
    {
        if (currentSequence == null || sequences.Count == 0) return;

        int currentIndex = sequences.IndexOf(currentSequence);
        if (currentIndex < 0) return;

        int nextIndex = currentIndex + 1;

        if (nextIndex >= sequences.Count)
        {
            Debug.Log($"Reached end of sequences for {gameObject.name}.");
            if (!wrapAroundSequences) return;
            nextIndex = 0;
        }

        StartSequence(sequences[nextIndex]);
    }

    public void StartSequence(int index) => StartSequence(sequences[index]);
    public void StartSequence(Sequence sequence)
    {
        currentSequence?.onSequenceEnd?.Invoke();
        currentSequence = sequence;
        _cancelToken?.Cancel();
        HandleSequence(sequence);
        sequence.onSequenceStart?.Invoke();
    }

    public void PlaySound(AudioClip sound)
    {
        if (_audioSource == null) return;
        _audioSource.PlayOneShot(sound);
    }

    private void ApplyRotations()
    {
        if (agent.velocity.sqrMagnitude > 0.01f) // Makes rotation look a lot snappier, manually doing it
        {
            Quaternion targetRot = Quaternion.LookRotation(agent.velocity.normalized);
            agent.transform.rotation = Quaternion.RotateTowards(
                agent.transform.rotation,
                targetRot,
                Time.deltaTime * agent.angularSpeed // turn speed
            );
        }
    }
}
