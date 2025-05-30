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
    public UnityEvent OnIncompleteInteraction= new();
    public UnityEvent OnFailInteraction= new();

    [Header("Quest Settings")]
    public bool _hasQuest = false;
    [ShowIf("_hasQuest")] public Quest Quest;

    private readonly string _playerObjName = "Main Camera";
    private GameObject _playerObj;

    private Vector3 _currentDestination;

    private int _interactionCount = 0;

    private DialogueSystem _dialogueSystem;

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

        _dialogueSystem = GetComponent<DialogueSystem>();
    }

    private new void Start()
    {
        base.Start();
        _playerObj = GameObject.Find(_playerObjName);
    }

    private void Update()
    {
        if(Keyboard.current.spaceKey.wasPressedThisFrame)
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

        if(Keyboard.current.fKey.wasPressedThisFrame)
        {
            Quest.Complete();
        }
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            Quest.Fail();
        }
    }

    public void Interact()
    {
        _interactionCount++;
        InterruptNPC();
    }

    private void InterruptNPC()
    {
        _currentDestination = _agent.destination;
        _agent.SetDestination(_bodyObj.transform.position);

        // Face player
        // Get the direction to the player
        Vector3 directionToPlayer = _playerObj.transform.position - _bodyObj.transform.position;
        // Zero out the Y component to only rotate horizontally
        directionToPlayer.y = 0;
        // Create rotation and apply it
        Tween.LocalRotation(_bodyObj.transform, Quaternion.LookRotation(directionToPlayer), 0.3f).OnComplete(StartDialogue);

        StartIdling();

        _cancellationTokenSource.Cancel();
        _isBeingInteractedWith = true;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private void StartDialogue()
    {
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
        _dialogueSystem.EndDialogue();
        _agent.SetDestination(_currentDestination);
        _isBeingInteractedWith = false;
    }
}
