using EditorAttributes;
using EditorAttributes.Editor.Utility;
using UnityEngine;
using UnityEngine.Events;

[SelectionBase]
[RequireComponent(typeof(DialogueSystem), typeof(ObjectiveSystem))]
public class InteractableNPC_SM : NPC_SM // SM = State Machine
{
    [Header("Interaction Settings")]
    [field: SerializeField] public bool IsInteractable { get; set; } = true;

    public Dialogue firstDialogue;
    public Dialogue incompleteDialogue;
    public Dialogue completeDialogue;
    public Dialogue failDialogue;

    [Header("Interaction Events")]
    public UnityEvent onFirstInteraction = new();
    public UnityEvent onCompleteInteraction = new();
    public UnityEvent onIncompleteInteraction = new();
    public UnityEvent onFailInteraction = new();

    [HideInInspector] public int interactionCount = 0;
    [HideInInspector] public DialogueSystem dialogueSystem;

    //[Header("Objective Settings")]
    //[SerializeField] private bool hasObjective = false;
    private ObjectiveSystem objective;

    private new void Awake()
    {
        base.Awake();

        dialogueSystem = GetComponent<DialogueSystem>();
        objective = GetComponent<ObjectiveSystem>();

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

    }

    [Button]
    public void Interact()
    {
        if(!IsInteractable) return;

        if (currentState == states[States.Interact]) // if already in dialogue, exit it
        {
            SwitchState(States.Walk); // Walk will auto switch to checkpoint if already there, so this is kind of the same as resuming
        }
        else
        {
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

        switch (objective.currentStatus)
        {
            case ObjectiveSystem.Statuses.ToDo:
                onFirstInteraction?.Invoke();

                dialogueSystem.StartDialogue(firstDialogue);

                objective.Begin();
                break;
            case ObjectiveSystem.Statuses.InProgress:
                onIncompleteInteraction?.Invoke();

                dialogueSystem.StartDialogue(incompleteDialogue);
                break;
            case ObjectiveSystem.Statuses.Completed:
                onCompleteInteraction?.Invoke();

                dialogueSystem.StartDialogue(completeDialogue);
                break;
            case ObjectiveSystem.Statuses.Failed:
                onFailInteraction?.Invoke();

                dialogueSystem.StartDialogue(failDialogue);
                break;
        }
    }
}
