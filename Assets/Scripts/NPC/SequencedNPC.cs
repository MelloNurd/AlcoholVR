using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    public enum Type
    {
        Animate,
        Dialogue,
        Walk,
        WalkToPlayer,
        Wait,
        TurnToFace,
    }

    [Header("Sequence Settings")]
    public Type type;

    [ShowField(nameof(type), Type.Animate)] public AnimationClip animation;

    [ShowField(nameof(type), Type.Dialogue)] public Dialogue dialogue;

    [ShowField(nameof(type), Type.Walk)] public Transform destination;
    [ConditionalEnumField(ConditionType.OR, nameof(type), Type.Walk, nameof(type), Type.WalkToPlayer)]
    public bool useDefaultWalkAnimation = true;
    [ConditionalEnumField(ConditionType.OR, nameof(type), Type.Walk, nameof(type), Type.WalkToPlayer), HideField(nameof(useDefaultWalkAnimation))]
    public AnimationClip walkAnimation;

    [ShowField(nameof(type), Type.Wait)] public float secondsToWait;

    [ShowField(nameof(type), Type.TurnToFace)] public float turnSpeed = 0.3f;
    [ShowField(nameof(type), Type.TurnToFace)] public Vector3 directionToFace;
    [Space]
    public bool nextSequenceOnEnd;

    [FoldoutGroup("Events", nameof(onSequenceStart), nameof(onSequenceEnd))]
    [SerializeField] private Void groupHolder;

    [HideInInspector] public UnityEvent onSequenceStart = new();
    [HideInInspector] public UnityEvent onSequenceEnd = new();

    #region Constructors
    public Sequence(Type type, AnimationClip animation, bool nextSequenceOnEnd = true)
    {
        this.type = type;
        this.animation = animation;
        this.nextSequenceOnEnd = nextSequenceOnEnd;
    }
    public Sequence(Type type, Dialogue dialogue, bool nextSequenceOnEnd = true)
    {
        this.type = type;
        this.dialogue = dialogue;
        this.nextSequenceOnEnd = nextSequenceOnEnd;
    }
    public Sequence(Type type, Transform destination, bool nextSequenceOnEnd = true)
    {
        this.type = type;
        this.destination = destination;
        this.nextSequenceOnEnd = nextSequenceOnEnd;
    }
    public Sequence(Type type, bool nextSequenceOnEnd = true)
    {
        this.type = type;
        this.nextSequenceOnEnd = nextSequenceOnEnd;
    }
    public Sequence(Type type, float secondsToWait, bool nextSequenceOnEnd = true)
    {
        this.type = type;
        this.secondsToWait = secondsToWait;
        this.nextSequenceOnEnd = nextSequenceOnEnd;
    }
    public Sequence(Type type, Vector3 directionToFace, float turnSpeed = 0.3f, bool nextSequenceOnEnd = true)
    {
        this.type = type;
        this.directionToFace = directionToFace;
        this.turnSpeed = turnSpeed;
        this.nextSequenceOnEnd = nextSequenceOnEnd;
    }
    #endregion
}

[SelectionBase]
public class SequencedNPC : MonoBehaviour
{
    public List<Sequence> sequences = new List<Sequence>();
    public Sequence currentSequence;
    public int currentSequenceIndex => sequences.IndexOf(currentSequence);

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
        animator = GetComponentInChildren<Animator>();
        agent = GetComponentInChildren<NavMeshAgent>();
        _audioSource = GetComponentInChildren<AudioSource>();
        dialogueSystem = GetComponent<DialogueSystem>();
        lookAt = GetComponentInChildren<LookAt>();
    }

    private void Start()
    {
        _playerObj = Player.Instance.Camera.gameObject;

        if (sequences.Count == 0)
        {
            Debug.Log("No sequences assigned to " + gameObject.name + ". Please assign at least one sequence.");
            return;
        }

        StartSequence(0);
    }

    private void Update()
    {
        ProcessWalking();
    }

    private void ProcessWalking()
    {
        ApplyWalkRotations();

        if (!_isAtDestination && (currentSequence.type == Sequence.Type.Walk || currentSequence.type == Sequence.Type.WalkToPlayer))
        {
            bool isWalkToPlayer = currentSequence.type == Sequence.Type.WalkToPlayer;

            if (isWalkToPlayer)
            {
                _lastDestinationUpdateTime += Time.deltaTime;

                // Update destination to be in front of player every half second
                Vector3 inFrontOfPlayer = Player.Instance.CamPosition + Player.Instance.Camera.transform.forward.WithY(0).normalized;
                if (_lastDestinationPosition != inFrontOfPlayer && _lastDestinationUpdateTime > 0.5f)
                {
                    _lastDestinationUpdateTime = 0f;
                    agent.SetDestinationToClosestPoint(inFrontOfPlayer);
                    _lastDestinationPosition = inFrontOfPlayer;
                }
            }

            if (agent.IsAtDestination())
            {
                _isAtDestination = true;
                agent.isStopped = true;
            }
        }
    }

    private async UniTask HandleSequence(Sequence sequence)
    {
        if (_cancelToken != null)
        {
            _cancelToken.Dispose();
        }
        _cancelToken = new CancellationTokenSource();

        switch (sequence.type)
        {
            case Sequence.Type.Animate:
                await ExecuteAnimateSequence(sequence);
                break;
            case Sequence.Type.Dialogue:
                await ExecuteDialogueSequence(sequence);
                break;
            case Sequence.Type.Walk:
                await ExecuteWalkSequence(sequence);
                break;
            case Sequence.Type.WalkToPlayer:
                await ExecuteWalkToPlayerSequence(sequence);
                break;
            case Sequence.Type.Wait:
                await ExecuteWaitSequence(sequence);
                break;
            case Sequence.Type.TurnToFace:
                await ExecuteTurnToFaceSequence(sequence);
                break;
        }
    }
    private async UniTask ExecuteTurnToFaceSequence(Sequence sequence)
    {
        Tween.StopAll(bodyObj.transform);

        await Tween.Rotation(bodyObj.transform, Quaternion.LookRotation(sequence.directionToFace.normalized), sequence.turnSpeed);

        if (currentSequence == sequence)
        {
            if (sequence.nextSequenceOnEnd && currentSequence == sequence)
            {
                StartNextSequence();
            }
        }
    }
    private async UniTask ExecuteWaitSequence(Sequence sequence)
    {
        PlayAnimation(defaultAnimation);

        await UniTask.Delay(Mathf.RoundToInt(sequence.secondsToWait * 1000), cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
        if (_cancelToken.IsCancellationRequested) return;

        if (currentSequence == sequence)
        {
            if (sequence.nextSequenceOnEnd && currentSequence == sequence)
            {
                StartNextSequence();
            }
        }
    }
    private async UniTask ExecuteWalkToPlayerSequence(Sequence sequence)
    {
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        temp.transform.position = Player.Instance.Position + Player.Instance.Camera.transform.forward.WithY(0).normalized;
        temp.GetComponent<Renderer>().material.color = Color.yellow;
        temp.transform.localScale *= 0.5f;
        Debug.Log($"player position: ({Player.Instance.Position}), cam forward: ({Player.Instance.Camera.transform.forward.WithY(0).normalized})");

        Destroy(temp, 6f);

        _isAtDestination = false; 
        agent.SetDestinationToClosestPoint(Player.Instance.Position + Player.Instance.Camera.transform.forward.WithY(0).normalized, 1f);
        agent.isStopped = false;

        AnimationClip walkAnim = (sequence.useDefaultWalkAnimation && sequence.walkAnimation != null)
            ? sequence.walkAnimation
            : walkAnimation;

        PlayAnimation(walkAnim);

        await UniTask.WaitUntil(() => agent.IsAtDestination(), cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
        if (_cancelToken.IsCancellationRequested) return;

        if (currentSequence == sequence)
        {
            PlayAnimation(defaultAnimation);
            if (sequence.nextSequenceOnEnd)
            {
                StartNextSequence();
            }
        }
    }
    private async UniTask ExecuteWalkSequence(Sequence sequence)
    {
        _isAtDestination = false;
        agent.SetDestinationToClosestPoint(sequence.destination.transform.position);
        agent.isStopped = false;

        AnimationClip walkAnim = (sequence.useDefaultWalkAnimation && sequence.walkAnimation != null)
            ? sequence.walkAnimation
            : walkAnimation;

        PlayAnimation(walkAnim);

        await UniTask.WaitUntil(() => _isAtDestination, cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
        if (_cancelToken.IsCancellationRequested) return;

        if(currentSequence == sequence)
        {
            PlayAnimation(defaultAnimation);
            if (sequence.nextSequenceOnEnd)
            {
                StartNextSequence();
            }
        }
    }
    private async UniTask ExecuteDialogueSequence(Sequence sequence)
    {
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
        await UniTask.WaitUntil(() => !Player.Instance.IsInDialogue, cancellationToken: _cancelToken.Token);
        if (_cancelToken.IsCancellationRequested) return;

        dialogueSystem.StartDialogue(sequence.dialogue);

        await UniTask.WaitUntil(() => !dialogueSystem.IsDialogueActive, cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
        if (_cancelToken.IsCancellationRequested) return;

        if (currentSequence == sequence)
        {
            if (sequence.nextSequenceOnEnd)
            {
                StartNextSequence();
            }
        }
    }
    private async UniTask ExecuteAnimateSequence(Sequence sequence)
    {
        if (sequence.animation == null)
        {
            Debug.LogWarning($"No animation assigned for Animate sequence on {gameObject.name}. Skipping.");
            if (sequence.nextSequenceOnEnd && currentSequence == sequence)
            {
                StartNextSequence();
            }
            return;
        }

        PlayAnimation(sequence.animation);

        await UniTask.Delay(Mathf.RoundToInt(sequence.animation.length * 1000), cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
        if (_cancelToken.IsCancellationRequested) return;

        if (currentSequence == sequence)
        {
            if (sequence.nextSequenceOnEnd)
            {
                StartNextSequence();
            }
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

        await UniTask.Delay(500, cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
        if (_cancelToken.IsCancellationRequested) return;
    }

    public void StartNextSequence() => StartNextSequenceAsync(1).Forget();
    public void StartNextSequence(int indexIncrease) => StartNextSequenceAsync(indexIncrease).Forget();
    public async UniTask StartNextSequenceAsync() => await StartNextSequenceAsync(1);
    public async UniTask StartNextSequenceAsync(int indexIncrease)
    {
        if (currentSequence == null || sequences.Count == 0) return;

        int currentIndex = Mathf.Max(0, sequences.IndexOf(currentSequence)); // in case currentSequence is not in sequences, start from 0

        int nextIndex = currentIndex + indexIncrease;
        if (nextIndex >= sequences.Count)
        {
            currentSequence?.onSequenceEnd?.Invoke();
            onFinishSequences?.Invoke();

            if (!wrapAroundSequences)
            {
                return;
            }

            nextIndex = 0;
        }

        dialogueSystem.onEnd?.RemoveListener(DialogueEndHandler); // only need this for dialogue sequences

        //Debug.Log($"Starting next sequence for {gameObject.name}: {sequences[nextIndex].type}");
        await StartSequenceAsync(sequences[nextIndex]);
    }

    public void StartSequence(int index) => StartSequenceAsync(index).Forget();
    public void StartSequence(Sequence sequence) => StartSequenceAsync(sequence).Forget();
    public async UniTask StartSequenceAsync(int index) => await StartSequenceAsync(sequences[index]);
    public async UniTask StartSequenceAsync(Sequence sequence)
    {
        _cancelToken?.Cancel();
        _isAtDestination = true;
        if(agent != null && agent.enabled) agent.isStopped = true;
        currentSequence?.onSequenceEnd?.Invoke();
        if (dialogueSystem.IsDialogueActive) dialogueSystem.EndCurrentDialogue();
        currentSequence = sequence;
        await HandleSequence(sequence);
        sequence.onSequenceStart?.Invoke();
    }

    public void PlaySound(AudioClip sound)
    {
        if (_audioSource == null) return;
        _audioSource.PlayOneShot(sound);
    }
    public void PlayAnimation(AnimationClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("No animation clip provided to PlayAnimation.");
            return;
        }

        if(animator == null)
        {
            Debug.LogWarning("No animator found on SequencedNPC.");
            return;
        }

        animator.CrossFade(clip.name, 0.2f);
    }

    private void ApplyWalkRotations()
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