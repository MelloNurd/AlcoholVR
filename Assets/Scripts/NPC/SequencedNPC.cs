using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using PrimeTween;
using TMPro;
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
        TurnToFace,
    }

    [Header("Sequence Settings")]
    public SequenceType type;

    [ShowField(nameof(type), SequenceType.Animate)] public AnimationClip animation;

    [ShowField(nameof(type), SequenceType.Dialogue)] public Dialogue dialogue;

    [ShowField(nameof(type), SequenceType.Walk)] public Transform destination;
    [ConditionalEnumField(ConditionType.OR, nameof(type), SequenceType.Walk, nameof(type), SequenceType.WalkToPlayer)]
    public bool useDefaultWalkAnimation = true;
    [ConditionalEnumField(ConditionType.OR, nameof(type), SequenceType.Walk, nameof(type), SequenceType.WalkToPlayer), HideField(nameof(useDefaultWalkAnimation))]
    public AnimationClip walkAnimation;

    [ShowField(nameof(type), SequenceType.Wait)] public float secondsToWait;

    [ShowField(nameof(type), SequenceType.TurnToFace)] public float turnSpeed = 0.3f;
    [ShowField(nameof(type), SequenceType.TurnToFace)] public Vector3 directionToFace;
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
    public Sequence currentSequence;

    [SerializeField, Required] private AnimationClip defaultAnimation;
    [SerializeField, Required] private AnimationClip walkAnimation;

    public bool wrapAroundSequences = false; // If it should loop through the sequences or stop at the end

    [ButtonField(nameof(StartNextSequence)), DisableInEditMode, SerializeField] private Void startNextSequenceButton;

    public bool turnBodyToFacePlayer = true;
    public bool turnHeadToFacePlayer = true;

    [HideInInspector] public GameObject bodyObj;
    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public Animator animator;
    private AudioSource _audioSource;
    private GameObject _playerObj;
    public DialogueSystem dialogueSystem;
    public LookAt lookAt;

    private CancellationTokenSource _cancelToken;
    private bool _isAtDestination = true;

    private Vector3 _lastDestinationPosition;

    private float _lastDestinationUpdateTime = 0f;

    [HideInInspector] public UnityEvent onFinishSequences = new();

    private void Awake()
    {
        bodyObj = transform.Find("Body").gameObject;
        _playerObj = GameObject.Find("Main Camera"); // This may need changed later
        animator = GetComponentInChildren<Animator>();
        agent = GetComponentInChildren<NavMeshAgent>();
        _audioSource = GetComponentInChildren<AudioSource>();
        dialogueSystem = GetComponent<DialogueSystem>();
        lookAt = GetComponentInChildren<LookAt>();
    }

    private void Start()
    {
        if (sequences.Count == 0)
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
                _lastDestinationUpdateTime += Time.deltaTime;

                Vector3 inFrontOfPlayer = Player.Instance.Position + Player.Instance.playerCamera.transform.forward.WithY(0).normalized;
                if (_lastDestinationPosition != inFrontOfPlayer && _lastDestinationUpdateTime > 0.5f)
                {
                    _lastDestinationUpdateTime = 0f;
                    agent.SetDestinationToClosestPoint(inFrontOfPlayer);
                    _lastDestinationPosition = inFrontOfPlayer;
                }
            }
            if (agent.IsAtDestination())
            {
                Debug.Log($"{gameObject.name} has reached {(isWalkToPlayer ? "the player" : "its destination")}.");
                _isAtDestination = true;
                animator.CrossFadeInFixedTime(defaultAnimation.name, 0.4f);
                if (currentSequence.nextSequenceOnEnd)
                {
                    StartNextSequence();
                }
            }
        }
    }

    private async void HandleSequence(Sequence sequence)
    {
        if (_cancelToken != null)
        {
            _cancelToken.Dispose();
        }
        _cancelToken = new CancellationTokenSource();

        switch (sequence.type)
        {
            case Sequence.SequenceType.Animate:
                animator.CrossFadeInFixedTime(sequence.animation.name, 0.4f);
                if (sequence.nextSequenceOnEnd)
                {
                    await UniTask.Delay(Mathf.RoundToInt(sequence.animation.length * 1000), cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
                    if (_cancelToken.IsCancellationRequested) break;
                    animator.CrossFadeInFixedTime(defaultAnimation.name, 0.4f);
                    StartNextSequence();
                }

                break;
            case Sequence.SequenceType.Dialogue:
                dialogueSystem.onEnd?.AddListener(DialogueEndHandler);

                if (turnBodyToFacePlayer)
                {
                    Vector3 directionToPlayer = (_playerObj.transform.position - bodyObj.transform.position).WithY(0);
                    await Tween.Rotation(bodyObj.transform, Quaternion.LookRotation(directionToPlayer), 0.3f);
                }
                if (turnHeadToFacePlayer)
                {
                    lookAt.LookAtPlayer();
                }

                // Wait until player is free (not interacting with an NPC) before starting dialogue
                await UniTask.WaitUntil(() => !Player.Instance.IsInteractingWithNPC, cancellationToken: _cancelToken.Token);
                if (_cancelToken.IsCancellationRequested) break;

                dialogueSystem.StartDialogue(sequence.dialogue);

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
            case Sequence.SequenceType.TurnToFace:
                Tween.StopAll(bodyObj.transform);
                await Tween.Rotation(bodyObj.transform, Quaternion.LookRotation(sequence.directionToFace), sequence.turnSpeed);
                if (sequence.nextSequenceOnEnd)
                {
                    StartNextSequence();
                }
                break;
        }
    }

    public void SitDown() // I don't relaly have a much better way of doing this at the moment, unfortunately
    {
        Vector3 pos = bodyObj.transform.position + (bodyObj.transform.forward * -0.25f) + new Vector3(0, 0.125f, 0);
        Tween.StopAll(bodyObj.transform);
        Tween.Position(bodyObj.transform, pos, 0.3f);
    }

    private async void DialogueEndHandler()
    {
        if (_cancelToken != null)
        {
            _cancelToken.Dispose();
        }
        _cancelToken = new CancellationTokenSource();

        if (turnHeadToFacePlayer)
        {
            lookAt.isLooking = false;
        }
        Player.Instance.EnableMovement();
        await UniTask.Delay(1000, cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
        if (_cancelToken.IsCancellationRequested) return;
        StartNextSequence();
    }

    public void StartNextSequence() => StartNextSequence(1);
    public void StartNextSequence(int indexIncrease)
    {
        if (currentSequence == null || sequences.Count == 0) return;

        int currentIndex = sequences.IndexOf(currentSequence);
        if (currentIndex < 0) return;

        int nextIndex = currentIndex + indexIncrease;

        if (nextIndex >= sequences.Count)
        {
            Debug.Log($"Reached end of sequences for {gameObject.name}.");
            currentSequence?.onSequenceEnd?.Invoke();
            onFinishSequences?.Invoke();
            if (!wrapAroundSequences) return;
            nextIndex = 0;
        }

        dialogueSystem.onEnd?.RemoveListener(DialogueEndHandler); // only need this for dialogue sequences

        Debug.Log($"Starting next sequence for {gameObject.name}: {sequences[nextIndex].type}");
        StartSequence(sequences[nextIndex]);
    }

    public void StartSequence(int index) => StartSequence(sequences[index]);
    public void StartSequence(Sequence sequence)
    {
        currentSequence?.onSequenceEnd?.Invoke();
        if (dialogueSystem.IsDialogueActive) dialogueSystem.EndCurrentDialogue();
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
