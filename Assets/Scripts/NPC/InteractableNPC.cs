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

    [Header("Quest Settings")]
    public bool _hasQuest = false;
    [ShowIf("_hasQuest")] public Quest Quest;

    private readonly string _playerObjName = "Main Camera";
    private GameObject _playerObj;

    private Vector3 _currentDestination;
    private int _interactionCount = 0;
    private DialogueSystem _dialogueSystem;
    private bool _isRotatingToPlayer = false;

    private new void Awake()
    {
        base.Awake();
        if (_firstDialogue == null)
        {
            Debug.LogError("First dialogue is not set for " + gameObject.name + ". Please assign a dialogue.");
            return;
        }
        if(_incompleteDialogue == null) _incompleteDialogue = _firstDialogue;
        if(_completeDialogue == null) _completeDialogue = _firstDialogue;
        if(_failDialogue == null) _failDialogue = _firstDialogue;

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
        
        #if UNITY_EDITOR
        // Debug controls only for testing
        if(Keyboard.current.fKey.wasPressedThisFrame && _hasQuest)
        {
            Quest.Complete();
        }
        if (Keyboard.current.gKey.wasPressedThisFrame && _hasQuest)
        {
            Quest.Fail();
        }
        #endif
    }

    [Button("Execute Interaction")]
    public void TryInteract()
    {
        if (_isBeingInteractedWith)
        {
            ResumeNPC();
        }
        else
        {
            Interact();
        }
    }

    public void Interact()
    {
        if (_isBeingInteractedWith || _isRotatingToPlayer)
            return;
            
        _interactionCount++;
        InterruptNPC();
    }

    private void InterruptNPC()
    {
        // Store current destination and stop movement
        _currentDestination = _agent.destination;
        _agent.SetDestination(_bodyObj.transform.position);
        
        // Cancel current tasks safely
        if (!_cancellationTokenSource.IsCancellationRequested)
            _cancellationTokenSource.Cancel();
            
        _isBeingInteractedWith = true;
        _isRotatingToPlayer = true;
        _cancellationTokenSource = new CancellationTokenSource();
        
        // Set animation and rotate to player
        StartIdling();

        // Face player with clear transition to dialogue
        Vector3 directionToPlayer = _playerObj.transform.position - _bodyObj.transform.position;
        directionToPlayer.y = 0;
        
        Tween.LocalRotation(_bodyObj.transform, Quaternion.LookRotation(directionToPlayer), 0.3f)
            .OnComplete(() => {
                _isRotatingToPlayer = false;
                StartDialogue();
            });
    }

    private void StartDialogue()
    {
        if (!_hasQuest)
        {
            _dialogueSystem.StartDialogue(_firstDialogue);
            _firstDialogue.onDialogueStart?.Invoke();
            return;
        }

        switch(Quest.State)
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
        
        // Only restore destination if it's valid
        if (_currentDestination != Vector3.zero) 
            _agent.SetDestination(_currentDestination);
            
        _isBeingInteractedWith = false;
        
        // Let the NPC loop in base class naturally resume
    }
}
