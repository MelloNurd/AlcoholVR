using System;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using EditorAttributes.Editor;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class Sequence
{
    public UnityEvent OnSequenceStart = new();
    public UnityEvent OnSequenceEnd = new();

    public void StartSequence(NPC npc) 
    {
        OnSequenceStart.Invoke();
        OnStart(npc);
    }
    protected virtual void OnStart(NPC npc) { }

    public void UpdateSequence(NPC npc) 
    {
        OnUpdate(npc);
    }
    protected virtual void OnUpdate(NPC npc) { }

    public void EndSequence(NPC npc)
    {
        OnSequenceEnd.Invoke();
        OnEnd(npc);
    }
    protected virtual void OnEnd(NPC npc) { }
}

[Serializable]
public class WaitSequence : Sequence
{
    public float Time = 0f;

    protected override async void OnStart(NPC npc)
    {
        await UniTask.Delay(Time.ToMS());

        npc.StartNextSequence();
    }
}

[Serializable]
public class AnimateSequence : Sequence
{
    public AnimationClip Animation;
    public bool Loop = false;

    protected override async void OnStart(NPC npc)
    {
        if (Animation == null)
        {
            Debug.LogWarning("No animation assigned to AnimateSequence, continuing to next sequence.");
            npc.StartNextSequence();
            return;
        }

        await npc.PlayAnimationAsync(Animation);

        if (!Loop)
        {
            npc.StartNextSequence();
        }
    }
}

[Serializable]
public class StartDialogueSequence : Sequence
{
    public DialogueGraph dialogue;
    protected override async void OnStart(NPC npc)
    {
        await npc.StartDialogueAsync(dialogue);
        Debug.Log("Dialogue finished (from Sequence script).");
        npc.StartNextSequence();
    }
}

[Serializable]
public class WaitForItemSequence : Sequence
{
    public GameObject Item;

    public UnityEvent OnCorrectItemGiven = new();
    public UnityEvent OnIncorrectItemGiven = new();

    private UnityAction<NPC> _checkForCorrectItemAction;

    protected override void OnStart(NPC npc)
    {
        _checkForCorrectItemAction = CheckForCorrectItem;
        npc.OnNPCInteract.AddListener(() => _checkForCorrectItemAction(npc));
    }

    protected override void OnEnd(NPC npc)
    {
        npc.OnNPCInteract.RemoveListener(() => _checkForCorrectItemAction(npc));
    }

    private void CheckForCorrectItem(NPC npc)
    {
        if (HeldItems.IsHoldingItem(Item))
        {
            OnCorrectItemGiven.Invoke();
            npc.StartNextSequence();
        }
        else
        {
            OnIncorrectItemGiven.Invoke();
        }
    }
}

[Serializable]
public class MoveToSequence : Sequence
{
    public Transform Target;
    protected override async void OnStart(NPC npc)
    {
        if (Target == null)
        {
            Debug.LogWarning("No target assigned to MoveToSequence, continuing to next sequence.");
            npc.StartNextSequence();
            return;
        }
        await npc.MoveToAsync(Target);
        npc.StartNextSequence();
    }
}

[Serializable]
public class QueueDialogueSequence : Sequence
{
    [Tooltip("This will queue the dialogue for the NPC, causing the NPC to become interactable. Interacting with the npc while queued will initiate the dialogue.")]
    public DialogueGraph queuedDialogue;
    protected override void OnStart(NPC npc)
    {
        npc.QueueDialogue(queuedDialogue);
        npc.StartNextSequence();
    }
}