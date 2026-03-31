using System;
using System.Collections;
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
using UnityEngine.PlayerLoop;
using static NPC;
using static UnityEngine.Rendering.DebugUI;
using Void = EditorAttributes.Void;

[Serializable]
public class OldSequence
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
    public OldSequence(Type type, AnimationClip animation, bool nextSequenceOnEnd = true)
    {
        this.type = type;
        this.animation = animation;
        this.nextSequenceOnEnd = nextSequenceOnEnd;
    }
    public OldSequence(Type type, Dialogue dialogue, bool nextSequenceOnEnd = true)
    {
        this.type = type;
        this.dialogue = dialogue;
        this.nextSequenceOnEnd = nextSequenceOnEnd;
    }
    public OldSequence(Type type, Transform destination, bool nextSequenceOnEnd = true)
    {
        this.type = type;
        this.destination = destination;
        this.nextSequenceOnEnd = nextSequenceOnEnd;
    }
    public OldSequence(Type type, bool nextSequenceOnEnd = true)
    {
        this.type = type;
        this.nextSequenceOnEnd = nextSequenceOnEnd;
    }
    public OldSequence(Type type, float secondsToWait, bool nextSequenceOnEnd = true)
    {
        this.type = type;
        this.secondsToWait = secondsToWait;
        this.nextSequenceOnEnd = nextSequenceOnEnd;
    }
    public OldSequence(Type type, Vector3 directionToFace, float turnSpeed = 0.3f, bool nextSequenceOnEnd = true)
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
    public bool isDrunk = false;

    public List<OldSequence> sequences = new List<OldSequence>();
    public OldSequence currentSequence;
    public int currentSequenceIndex => sequences.IndexOf(currentSequence);

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
        if (Player.Instance == null)
        {
            Debug.Log("Player camera object is is null");
            //return;
        }
        else
        {
            _playerObj = Player.Instance.Camera.gameObject;
        }

        if (sequences.Count == 0)
        {
            Debug.Log("No sequences assigned to " + gameObject.name + ". Please assign at least one sequence.");
            return;
        }

        StartCoroutine(DelayedStartSequence());
    }

    private void Update()
    {
        ProcessWalking();
    }

    // Coroutine DelayedStartSequence to wait until end of frame to ensure all Start() methods have run, allowing outfit system to initialize characters before sequences start (in case sequences rely on that)
    private IEnumerator DelayedStartSequence()
    {
        yield return new WaitForEndOfFrame();
        StartSequence(0);
    }

    private void ProcessWalking()
    {
        ApplyWalkRotations();

        if (!_isAtDestination && (currentSequence.type == OldSequence.Type.Walk || currentSequence.type == OldSequence.Type.WalkToPlayer))
        {
            bool isWalkToPlayer = currentSequence.type == OldSequence.Type.WalkToPlayer;

            if (isWalkToPlayer)
            {
                _lastDestinationUpdateTime += Time.deltaTime;

                // Update destination to be in front of player every half second
                Vector3 inFrontOfPlayer = Player.Instance.CamPosition + Player.Instance.Camera.transform.forward.WithY(0).normalized * 1.5f;
                if (_lastDestinationPosition != inFrontOfPlayer && _lastDestinationUpdateTime > 0.5f)
                {
                    _lastDestinationUpdateTime = 0f;
                    agent.SetDestinationToClosestPoint(inFrontOfPlayer);
                    _lastDestinationPosition = inFrontOfPlayer;
                }
            }

            if (agent.IsAtDestination(0.01f))
            {
                if (isWalkToPlayer && Vector3.Distance(agent.transform.position, Player.Instance.Position) > 2f)
                {
                    // If this happens, it's basically a false positive, and we want to keep the NPC walking to the player
                    return;
                }

                _isAtDestination = true;
                agent.isStopped = true;
            }
        }
    }

    private async UniTask HandleSequence(OldSequence sequence)
    {
        if (_cancelToken != null)
        {
            _cancelToken.Dispose();
        }
        _cancelToken = new CancellationTokenSource();

        switch (sequence.type)
        {
            case OldSequence.Type.Animate:
                await ExecuteAnimateSequence(sequence);
                break;
            case OldSequence.Type.Dialogue:
                await ExecuteDialogueSequence(sequence);
                break;
            case OldSequence.Type.Walk:
                // ensure destination is world space position 
                await ExecuteWalkSequence(sequence);
                break;
            case OldSequence.Type.WalkToPlayer:
                await ExecuteWalkToPlayerSequence(sequence);
                break;
            case OldSequence.Type.Wait:
                await ExecuteWaitSequence(sequence);
                break;
            case OldSequence.Type.TurnToFace:
                await ExecuteTurnToFaceSequence(sequence);
                break;
        }
    }
    private async UniTask ExecuteTurnToFaceSequence(OldSequence sequence)
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
    private async UniTask ExecuteWaitSequence(OldSequence sequence)
    {
        PlayIdleAnimation();

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
    private async UniTask ExecuteWalkToPlayerSequence(OldSequence sequence)
    {
        _isAtDestination = false;
        agent.SetDestinationToClosestPoint(Player.Instance.Position + Player.Instance.Camera.transform.forward.WithY(0).normalized, 1f);
        agent.isStopped = false;

        PlayWalkAnimation();

        await UniTask.WaitUntil(() => _isAtDestination, cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
        if (_cancelToken.IsCancellationRequested) return;

        if (currentSequence == sequence)
        {
            PlayIdleAnimation();
            if (sequence.nextSequenceOnEnd)
            {
                StartNextSequence();
            }
        }
    }
    private async UniTask ExecuteWalkSequence(OldSequence sequence)
    {
        _isAtDestination = false;
        agent.SetDestinationToClosestPoint(sequence.destination.position, 1.5f);
        agent.isStopped = false;

        PlayWalkAnimation();

        await UniTask.WaitUntil(() => _isAtDestination, cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
        if (_cancelToken.IsCancellationRequested) return;

        if (currentSequence == sequence)
        {
            PlayIdleAnimation();
            if (sequence.nextSequenceOnEnd)
            {
                StartNextSequence();
            }
        }
    }
    private async UniTask ExecuteDialogueSequence(OldSequence sequence)
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

        Debug.Log("Started dialogue sequence on " + gameObject.name + ". dialogueSystem.IsDialogueActive: " + dialogueSystem.IsDialogueActive, gameObject);

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
    private async UniTask ExecuteAnimateSequence(OldSequence sequence)
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
        Vector3 pos = bodyObj.transform.position + (bodyObj.transform.forward * -0.45f);
        Tween.StopAll(bodyObj.transform);
        Tween.Position(bodyObj.transform, pos.AddY(0.15f), 0.3f, Ease.InOutSine);
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
    public void StartSequence(OldSequence sequence) => StartSequenceAsync(sequence).Forget();
    public async UniTask StartSequenceAsync(int index) => await StartSequenceAsync(sequences[index]);
    public async UniTask StartSequenceAsync(OldSequence sequence)
    {
        _cancelToken?.Cancel();
        _isAtDestination = true;
        if (agent != null && agent.enabled) agent.isStopped = true;
        currentSequence?.onSequenceEnd?.Invoke();
        if (dialogueSystem.IsDialogueActive) dialogueSystem.EndCurrentDialogue();
        currentSequence = sequence;
        sequence.onSequenceStart?.Invoke();
        await HandleSequence(sequence);
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

        if (animator == null)
        {
            Debug.LogWarning("No animator found on SequencedNPC.");
            return;
        }

        animator.SetBool("isDrunk", isDrunk);

        animator.CrossFade(clip.name, 0.2f);
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