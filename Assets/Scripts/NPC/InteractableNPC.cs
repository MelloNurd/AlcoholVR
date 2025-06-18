using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InteractableNPC : NPC
{
    [Header("Interaction Settings")]
    [Required] public Dialogue _firstDialogue;
    public Dialogue _incompleteDialogue;
    public Dialogue _completeDialogue;
    public Dialogue _failDialogue;
    public UnityEvent OnFirstInteraction = new();
    public UnityEvent OnCompleteInteraction = new();
    public UnityEvent OnIncompleteInteraction = new();
    public UnityEvent OnFailInteraction = new();
    private bool _canInteractWith = true;

    [Header("Quest Settings")]
    public bool _hasQuest = false;
    [ShowIf("_hasQuest")] public Quest Quest;

    private readonly string _playerObjName = "Main Camera";
    private GameObject _playerObj;

    private Vector3 _currentDestination;
    private int _interactionCount = 0;
    private DialogueSystem _dialogueSystem;
    private bool _isRotatingToPlayer = false;
    private bool _isInDialogue = false;

    private new void Awake()
    {
        base.Awake();
        if (_firstDialogue == null)
        {
            Debug.LogError("First dialogue is not set for " + gameObject.name + ". Please assign a dialogue.");
            return;
        }
        if (_incompleteDialogue == null) _incompleteDialogue = _firstDialogue;
        if (_completeDialogue == null) _completeDialogue = _firstDialogue;
        if (_failDialogue == null) _failDialogue = _firstDialogue;

        _dialogueSystem = GetComponent<DialogueSystem>();
    }

    private new void Start()
    {
        base.Start();
        _playerObj = GameObject.Find(_playerObjName);
    }

    private new void Update()
    {
        base.Update();
        
        // Some quest example stuff
        if (Keyboard.current.fKey.wasPressedThisFrame && _hasQuest)
        {
            Quest.Complete();
        }
        if (Keyboard.current.gKey.wasPressedThisFrame && _hasQuest)
        {
            Quest.Fail();
        }
    }

    [Button("Execute Interaction")]
    public void TryInteract()
    {
        // If we're already in dialogue, exit it
        if (_isInDialogue)
        {
            ResumeNPC();
            return;
        }

        if(!_canInteractWith) return;

        // If being interacted with but not in dialogue yet (e.g. rotating), do nothing
        if (_isBeingInteractedWith && !_isInDialogue)
        {
            return;
        }

        Interact();
    }

    public async void Interact()
    {
        if (_isBeingInteractedWith || _isRotatingToPlayer)
            return;

        _interactionCount++;
        InterruptNPC();

        _canInteractWith = false;
        await UniTask.Delay(750, ignoreTimeScale: true);
        _canInteractWith = true;
    }

    private void InterruptNPC()
    {
        // Store current destination and stop movement
        _currentDestination = _agent.destination;
        _agent.isStopped = true;

        // Track if we were moving when interrupted
        _wasMovingToCheckpoint = currentState == NPCState.Moving;
        
        // Cancel current tasks safely
        CancelCurrentOperations();

        _isBeingInteractedWith = true;
        _isRotatingToPlayer = true;

        // Set animation and rotate to player
        StartIdling();

        // Face player with clear transition to dialogue
        Vector3 directionToPlayer = _playerObj.transform.position - _bodyObj.transform.position;
        directionToPlayer.y = 0; // Make sure we only rotate around the Y axis

        if (directionToPlayer.sqrMagnitude < 0.001f)
        {
            // If player is too close, skip rotation
            _isRotatingToPlayer = false;
            StartDialogue();
        }
        else
        {
            Tween.LocalRotation(_bodyObj.transform, Quaternion.LookRotation(directionToPlayer), 0.3f)
                .OnComplete(() => {
                    _isRotatingToPlayer = false;
                    StartDialogue();
                });
        }
    }

    private void CancelCurrentOperations()
    {
        // Create a new token source before canceling the old one to avoid race conditions
        var oldTokenSource = _cancellationTokenSource;
        _cancellationTokenSource = new CancellationTokenSource();

        if (oldTokenSource != null && !oldTokenSource.IsCancellationRequested)
        {
            oldTokenSource.Cancel();
            oldTokenSource.Dispose();
        }
    }

    private void StartDialogue()
    {
        _isInDialogue = true;

        if (!_hasQuest)
        {
            _dialogueSystem.StartDialogue(_firstDialogue);
            _firstDialogue.onDialogueStart?.Invoke();
            return;
        }

        switch (Quest.State)
        {
            case QuestState.NotStarted:
                _dialogueSystem.StartDialogue(_firstDialogue);
                _firstDialogue.onDialogueStart?.Invoke();
                OnFirstInteraction?.Invoke();
                Quest.Start();
                break;
            case QuestState.Incomplete:
                _dialogueSystem.StartDialogue(_incompleteDialogue);
                _incompleteDialogue.onDialogueStart?.Invoke();
                OnIncompleteInteraction?.Invoke();
                break;
            case QuestState.Complete:
                _dialogueSystem.StartDialogue(_completeDialogue);
                _completeDialogue.onDialogueStart?.Invoke();
                OnCompleteInteraction?.Invoke();
                break;
            case QuestState.Failed:
                _dialogueSystem.StartDialogue(_failDialogue);
                _failDialogue.onDialogueStart?.Invoke();
                OnFailInteraction?.Invoke();
                break;
        }
    }

    private void ResumeNPC()
    {
        if (!_isBeingInteractedWith)
            return;

        _dialogueSystem.EndDialogue();
        _isInDialogue = false;

        // Resume NPC movement
        _agent.isStopped = false;

        // Only restore destination if it's valid
        if (_currentDestination != Vector3.zero)
            _agent.SetDestination(_currentDestination);

        _isBeingInteractedWith = false;

        // Let the NPC loop in base class naturally resume
    }
}