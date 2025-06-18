using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[SelectionBase]
public class InteractableNPC_SM : NPC_SM // SM = State Machine
{
    [Header("Interaction Settings")]
    public bool isInteractable = true;

    public Dialogue firstDialogue;
    public Dialogue incompleteDialogue;
    public Dialogue completeDialogue;
    public Dialogue failDialogue;

    public UnityEvent onFirstInteraction = new();
    public UnityEvent onCompleteInteraction = new();
    public UnityEvent onIncompleteInteraction = new();
    public UnityEvent onFailInteraction = new();

    [HideInInspector] public int interactionCount = 0;
    [HideInInspector] public DialogueSystem dialogueSystem;

    [Header("Quest Settings")]
    public bool _hasQuest = false;
    [ShowIf("_hasQuest")] public Quest Quest;

    private new void Awake()
    {
        base.Awake();

        dialogueSystem = GetComponent<DialogueSystem>();

        if (firstDialogue != null)
        {
            if (incompleteDialogue == null) incompleteDialogue = firstDialogue;
            if (completeDialogue == null) completeDialogue = firstDialogue;
            if (failDialogue == null) failDialogue = firstDialogue;
        }
        else if (isInteractable)
        {
            Debug.LogError("First dialogue is not set for " + gameObject.name + ". Please assign a dialogue.");
        }
    }

    [Button("Execute Interact", EButtonEnableMode.Playmode)]
    public void Interact()
    {
        if(!isInteractable) return;

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

        if (!_hasQuest)
        {
            dialogueSystem.StartDialogue(firstDialogue);
            firstDialogue.onDialogueStart?.Invoke();
            return;
        }

        switch (Quest.State)
        {
            case QuestState.NotStarted:
                dialogueSystem.StartDialogue(firstDialogue);
                firstDialogue.onDialogueStart?.Invoke();
                onFirstInteraction?.Invoke();
                Quest.Start();
                break;
            case QuestState.Incomplete:
                dialogueSystem.StartDialogue(incompleteDialogue);
                incompleteDialogue.onDialogueStart?.Invoke();
                onIncompleteInteraction?.Invoke();
                break;
            case QuestState.Complete:
                dialogueSystem.StartDialogue(completeDialogue);
                completeDialogue.onDialogueStart?.Invoke();
                onCompleteInteraction?.Invoke();
                break;
            case QuestState.Failed:
                dialogueSystem.StartDialogue(failDialogue);
                failDialogue.onDialogueStart?.Invoke();
                onFailInteraction?.Invoke();
                break;
        }
    }
}
