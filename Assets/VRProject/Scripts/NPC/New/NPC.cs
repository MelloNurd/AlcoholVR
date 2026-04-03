using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using static UnityEngine.GraphicsBuffer;

public enum DialoguePose
{
    IdleStand,
    IdleSit,
    IdleSitCrossLegged,
}

[SelectionBase]
public class NPC : MonoBehaviour
{
    [HideInInspector] public int CurrentSequenceIndex = 0;
    [SerializeReference] public List<Sequence> Sequences = new List<Sequence>();

    public CancellationTokenSource CancelTokenSource { get; private set; } = new CancellationTokenSource();

    public bool isDrunk = false;
    public DialoguePose dialoguePose = DialoguePose.IdleStand;
    public bool canInteract = true;

    // Component References
    private GameObject _bodyObject;
    private DialogueRunner _dialogueRunner;
    private Animator _animator;
    private XRSimpleInteractable _interactable;
    private AudioSource _audioSource;
    private NavMeshAgent _agent;

    [HideInInspector] public UnityEvent OnNPCInteract = new();

    public void Awake()
    {
        _agent = GetComponentInChildren<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _interactable = GetComponentInChildren<XRSimpleInteractable>();
        _audioSource = GetComponentInChildren<AudioSource>();
        _dialogueRunner = GetComponentInChildren<DialogueRunner>();
        _bodyObject = transform.Find("Body").gameObject;

        if (_interactable == null)
        {
            Debug.LogError($"XRSimpleInteractable component not found on {gameObject.name} or its children. Interactions will not work.", gameObject);
            return;
        }
        _interactable.selectEntered.AddListener((args) => InteractWith());

        CurrentSequenceIndex = 0;
    }

    private async void Start()
    {
        await UniTask.Yield(); // Wait a frame to ensure everything is initialized

        if (CurrentSequenceIndex >= 0 && CurrentSequenceIndex < Sequences.Count)
        {
            Sequences[CurrentSequenceIndex].StartSequence(this);
        }
    }

    private void Update()
    {
        if (CurrentSequenceIndex < 0 || CurrentSequenceIndex >= Sequences.Count)
            return;

        Sequences[CurrentSequenceIndex].UpdateSequence(this);
    }

    // Sequence Management
    public void StartNextSequence()
    {
        if (CurrentSequenceIndex < 0 || CurrentSequenceIndex >= Sequences.Count)
        {
            Debug.LogWarning($"Current sequence {CurrentSequenceIndex} is out of bounds.", gameObject);
            return;
        }

        int nextIndex = CurrentSequenceIndex + 1;
        if (nextIndex < Sequences.Count)
        {
            StartSequence(nextIndex);
        }
        else
        {
            Sequences[CurrentSequenceIndex].EndSequence(this);
            Debug.Log($"NPC ({gameObject.name}) reached the end of their sequences.", gameObject);
        }
    }

    public void StartSequence(int index)
    {
        if (index < 0 || index >= Sequences.Count)
        {
            Debug.LogWarning($"Sequence index {index} is out of bounds.");
            return;
        }
        if (CurrentSequenceIndex >= 0 && CurrentSequenceIndex < Sequences.Count && index != CurrentSequenceIndex)
        {
            Sequences[CurrentSequenceIndex].EndSequence(this);
        }
        CurrentSequenceIndex = index;
        Sequences[CurrentSequenceIndex].StartSequence(this);
    }

    // Dialogue
    public async void InteractWith()
    {
        Debug.Log($"CurrentSequenceIndex start of ineract: {CurrentSequenceIndex}");
        if (!canInteract)
            return;

        // Prioritize dialogue interactions over other interactions
        if (_dialogueRunner.IsQueued)
        {
            CancelTokenSource?.Cancel();

            // Create new dialogue sequence and start (ideally not in the list)
            Debug.Log($"CurrentSequenceIndex middle of ineract: {CurrentSequenceIndex}");

            await StartDialogueAsync(_dialogueRunner.QueuedDialogue);

            // Return to previous current sequence
            CancelTokenSource = new CancellationTokenSource();
            StartSequence(CurrentSequenceIndex);
        }
        else
        {
            OnNPCInteract.Invoke();
        }
        Debug.Log($"CurrentSequenceIndex end of ineract: {CurrentSequenceIndex}");
    }

    public async UniTask StartDialogueAsync(DialogueGraph dialogue)
    {
        if(_dialogueRunner == null)
        {
            Debug.LogError("DialogueRunner component not found. Cannot start dialogue.", gameObject);
            return;
        }
        canInteract = false;

        // Save the current animation to return to after dialogue
        AnimationClip currentAnimation = _animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        switch (dialoguePose)
        {
            case DialoguePose.IdleStand:
                PlayIdleAnimation();
                break;
            case DialoguePose.IdleSit:
                _animator.CrossFade(SettingsManager.Instance.MaleSittingIdleAnim.name, 0.2f);
                break;
            case DialoguePose.IdleSitCrossLegged:
                _animator.CrossFade(SettingsManager.Instance.FemaleSittingIdleAnim.name, 0.2f);
                break;
        }

        await TurnToFaceAsyc(Player.Instance.Camera.transform, 0.5f); // Turn to face the player before starting dialogue

        await _dialogueRunner.StartDialogueAsync(dialogue);

        _animator.CrossFade(currentAnimation.name, 0.2f);
        Debug.Log("Dialogue finished (from NPC script).");
    }

    public void QueueDialogue(DialogueGraph dialogue)
    {
        if(_dialogueRunner == null)
        {
            Debug.LogError("DialogueRunner component not found. Cannot queue dialogue.", gameObject);
            return;
        }

        Debug.Log($"Queueing dialogue: {dialogue.name} on NPC: {gameObject.name}");
        _dialogueRunner.QueueDialogue(dialogue);
        canInteract = true;
    }

    public void ClearDialogueQueue()
    {
        if(_dialogueRunner == null)
        {
            Debug.LogError("DialogueRunner component not found. Cannot clear dialogue queue.", gameObject);
            return;
        }
        _dialogueRunner.QueueDialogue(null);
        canInteract = false;
    }

    // Animations
    public void PlayAnimation(AnimationClip clip)
    {
        PlayAnimationAsync(clip).Forget();
    }

    public void PlayIdleAnimation()
    {
        Debug.Log("Playing idle animation", gameObject);
        _animator.SetBool("isDrunk", isDrunk);

        _animator.SetTrigger("Start Idle");
        _animator.SetBool("isWalk", false);
    }

    public void PlayWalkAnimation()
    {
        _animator.SetBool("isDrunk", isDrunk);

        _animator.SetTrigger("Start Idle");
        _animator.SetBool("isWalk", true);
    }

    public async UniTask PlayAnimationAsync(AnimationClip clip)
    {
        _animator.CrossFade(clip.name, 0.15f);
        await UniTask.Delay(clip.length.ToMS(), cancellationToken: CancelTokenSource.Token).SuppressCancellationThrow();
    }

    // NavMesh Agent
    public bool IsAtDestination(float threshold = 0.1f) => _agent.IsAtDestination();
    public void SetDestinationToClosestPoint(Vector3 destination) => _agent.SetDestinationToClosestPoint(destination);

    public async UniTask MoveToAsync(Transform transform) => await MoveToAsync(transform.position);
    public async UniTask MoveToAsync(Vector3 destination)
    {
        if (_agent == null || !_agent.isActiveAndEnabled)
        {
            Debug.LogWarning("NavMeshAgent is not active or enabled. Cannot move.", gameObject);
            return;
        }
        if (!_agent.SetDestinationToClosestPoint(destination, 1.5f))
        {
            Debug.LogWarning($"Failed to set destination for {_agent.gameObject.name}. Check if the destination is valid.", gameObject);
            return;
        }

        PlayWalkAnimation();
        while (!_agent.IsAtDestination())
        {
            await UniTask.Yield(PlayerLoopTiming.Update, CancelTokenSource.Token).SuppressCancellationThrow(); // Wait for the next frame
            if (CancelTokenSource.IsCancellationRequested)
            {
                _agent.ResetPath();
                break;
            }
        }
        PlayIdleAnimation();
        await UniTask.Yield(); // Ensure the idle animation has a frame to start before any next actions are taken
    }

    public async UniTask TurnToFaceAsyc(Transform target, float duration) => await TurnToFaceAsyc(target.position, duration);
    public async UniTask TurnToFaceAsyc(Vector3 targetPosition, float duration)
    {
        Tween.StopAll(_bodyObject.transform);

        Vector3 directionToTarget = (targetPosition - _bodyObject.transform.position).WithY(0);
        _ = Tween.Rotation(_bodyObject.transform, Quaternion.LookRotation(directionToTarget), duration);

        await UniTask.Delay(duration.ToMS(), cancellationToken: CancelTokenSource.Token).SuppressCancellationThrow(); // Use UniTask to allow cancellation

        if (CancelTokenSource.IsCancellationRequested)
            Tween.StopAll(_bodyObject.transform); // Stop the tween immediately if cancelled
    }
}