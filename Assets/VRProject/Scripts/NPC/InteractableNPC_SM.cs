using EditorAttributes;
using UnityEngine;
using UnityEngine.Events;

[SelectionBase]
[RequireComponent(typeof(DialogueSystem), typeof(ObjectiveSystem))]
public class InteractableNPC_SM : NPC_SM // SM = State Machine
{
    public GameObject exclamationObj;

    [Header("Interaction Settings")]
    [field: SerializeField] public bool IsInteractable { get; set; } = true;

    public Dialogue firstDialogue;
    public Dialogue incompleteDialogue;
    public Dialogue completeDialogue;
    public Dialogue failDialogue;

    public bool turnBackAfterDialogue = true;

    public bool turnBodyToFacePlayer = true;
    public bool turnHeadToFacePlayer = true;

    [Header("Interaction Events")]
    public UnityEvent onFirstInteraction = new();
    public UnityEvent onCompleteInteraction = new();
    public UnityEvent onIncompleteInteraction = new();
    public UnityEvent onFailInteraction = new();

    [HideInInspector] public int interactionCount = 0;
    [HideInInspector] public DialogueSystem dialogueSystem;

    //[Header("Objective Settings")]
    //[SerializeField] private bool hasObjective = false;
    public ObjectiveSystem objective;
    public bool autoStartObjective = true;

    private new void Awake()
    {
        base.Awake();

        dialogueSystem = GetComponent<DialogueSystem>();
        if (objective == null) objective = GetComponent<ObjectiveSystem>();

        if (firstDialogue != null)
        {
            if (incompleteDialogue == null) incompleteDialogue = firstDialogue;
            if (completeDialogue == null) completeDialogue = firstDialogue;
            if (failDialogue == null) failDialogue = firstDialogue;
        }
        else if (IsInteractable)
        {
            Debug.LogError("First dialogue is not set for " + gameObject.name + ". Please assign a dialogue.");
        }

        exclamationObj.SetActive(IsInteractable);
    }

    [Button]
    public void Interact()
    {
        if(!IsInteractable || Player.Instance.IsInteractingWithNPC) return;

        if (exclamationObj.activeSelf)
        {
            exclamationObj.SetActive(false);
        }

        interactionCount++;

        if (currentState == states[States.Interact]) // if already in dialogue, exit it
        {
            Player.Instance.IsInteractingWithNPC = false;
            SwitchState(States.Walk); // Walk will auto switch to checkpoint if already there, so this is kind of the same as resuming
        }
        else
        {
            Player.Instance.IsInteractingWithNPC = true;
            SwitchState(States.Interact);
        }
    }

    public void StartDialogue()
    {
        if (firstDialogue == null)
        {
            Debug.LogError($"NPC {gameObject.name} does not have a first dialogue assigned.");
            return;
        }

        // First, we run events
        // By doing these first, we can have dialogue/objective changes that affect the next dialogue shown
        switch (objective.currentStatus)
        {
            case ObjectiveSystem.Statuses.ToDo:
                onFirstInteraction?.Invoke();
                break;
            case ObjectiveSystem.Statuses.InProgress:
                onIncompleteInteraction?.Invoke();
                break;
            case ObjectiveSystem.Statuses.Completed:
                onCompleteInteraction?.Invoke();
                break;
            case ObjectiveSystem.Statuses.Failed:
                onFailInteraction?.Invoke();
                break;
        }

        // Then, we run dialogue
        switch (objective.currentStatus)
        {
            case ObjectiveSystem.Statuses.ToDo:
                dialogueSystem.StartDialogue(firstDialogue);

                if(autoStartObjective) objective.Begin();
                break;
            case ObjectiveSystem.Statuses.InProgress:
                dialogueSystem.StartDialogue(incompleteDialogue);
                break;
            case ObjectiveSystem.Statuses.Completed:
                dialogueSystem.StartDialogue(completeDialogue);
                break;
            case ObjectiveSystem.Statuses.Failed:
                dialogueSystem.StartDialogue(failDialogue);
                break;
        }
    }

    private void OnValidate()
    {
        if(exclamationObj == null)
        {
            exclamationObj = GetComponentInChildren<ExclamationPoint>(true)?.gameObject;
        }
    }
}
