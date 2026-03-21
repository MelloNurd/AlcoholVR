using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using EditorAttributes.Editor;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class Sequence
{
    public UnityEvent OnSequenceStart = new();
    public UnityEvent OnSequenceEnd = new();

    public void StartSequence(NPC npc) 
    {
        Debug.Log($"Started sequence: {GetType().Name}");
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
        Debug.Log($"Ended sequence: {GetType().Name}");
        OnSequenceEnd.Invoke();
        OnEnd(npc);
    }
    protected virtual void OnEnd(NPC npc) { }
}

[Serializable]
public class AnimateSequence : Sequence
{
    public AnimationClip Animation;

    [Tooltip("If true, the NPC will switch back to the Idle animation after the given animation plays. If false, it will stay on the given animation.")]
    public bool ResetOnEnd = true;

    protected override async void OnStart(NPC npc)
    {
        if (Animation == null)
        {
            Debug.LogWarning("No animation assigned to AnimateSequence, continuing to next sequence.");
            npc.StartNextSequence();
            return;
        }

        await npc.PlayAnimationAsync(Animation);

        if (ResetOnEnd)
        {
            npc.PlayIdleAnimation();
        }

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

[Serializable]
public class StartDialogueSequence : Sequence
{
    public DialogueGraph dialogue;
    protected override async void OnStart(NPC npc)
    {
        await npc.StartDialogueAsync(dialogue);
        npc.StartNextSequence();
    }
}

[Serializable]
public class WaitSecondsSequence : Sequence
{
    public float Seconds = 0f;

    protected override async void OnStart(NPC npc)
    {
        await UniTask.Delay(Seconds.ToMS(), cancellationToken: npc.CancelTokenSource.Token).SuppressCancellationThrow();
        if (!npc.CancelTokenSource.IsCancellationRequested)
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
public class WaitForInteractionSequence : Sequence
{
    protected override void OnStart(NPC npc)
    {
        npc.ClearDialogueQueue(); // Ensure no dialogue is queued that would interfere with interaction
        npc.canInteract = true;
        npc.OnNPCInteract.AddListener(npc.StartNextSequence);
    }

    protected override void OnEnd(NPC npc)
    {
        npc.canInteract = false;
        npc.OnNPCInteract.RemoveListener(npc.StartNextSequence);
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
            Debug.LogWarning("No target assigned for MoveToSequence, continuing to next sequence.");
            npc.StartNextSequence();
            return;
        }

        await npc.MoveToAsync(Target); // This is cancelled internally

        if (!npc.CancelTokenSource.IsCancellationRequested)
            npc.StartNextSequence();
    }
}

[Serializable]
public class MoveToPlayerSequence : Sequence
{
    float _lastDestinationUpdateTime;
    Vector3 _lastDestinationPosition;

    protected override async void OnStart(NPC npc)
    {
        _lastDestinationUpdateTime = 0f;
        _lastDestinationPosition = Vector3.zero;
    }

    protected override void OnUpdate(NPC npc)
    {
        _lastDestinationUpdateTime += Time.deltaTime;

        // Update destination to be in front of player every half second
        Vector3 inFrontOfPlayer = Player.Instance.CamPosition + Player.Instance.Camera.transform.forward.WithY(0).normalized * 1.5f;
        if (_lastDestinationPosition != inFrontOfPlayer && _lastDestinationUpdateTime > 0.5f)
        {
            _lastDestinationUpdateTime = 0f;
            _ = npc.MoveToAsync(inFrontOfPlayer);
            _lastDestinationPosition = inFrontOfPlayer;
        }
    }

    //if (!_isAtDestination && (currentSequence.type == OldSequence.Type.Walk || currentSequence.type == OldSequence.Type.WalkToPlayer))
    //   {
    //       bool isWalkToPlayer = currentSequence.type == OldSequence.Type.WalkToPlayer;

    //       if (isWalkToPlayer)
    //       {
    //           _lastDestinationUpdateTime += Time.deltaTime;

    //           // Update destination to be in front of player every half second
    //           Vector3 inFrontOfPlayer = Player.Instance.CamPosition + Player.Instance.Camera.transform.forward.WithY(0).normalized*1.5f;
    //           if (_lastDestinationPosition != inFrontOfPlayer && _lastDestinationUpdateTime > 0.5f)
    //           {
    //               _lastDestinationUpdateTime = 0f;
    //               agent.SetDestinationToClosestPoint(inFrontOfPlayer);
    //               _lastDestinationPosition = inFrontOfPlayer;
    //           }
    //       }

    //       if (agent.IsAtDestination(0.01f))
    //       {
    //           if(isWalkToPlayer && Vector3.Distance(agent.transform.position, Player.Instance.Position) > 2f)
    //           {
    //               // If this happens, it's basically a false positive, and we want to keep the NPC walking to the player
    //               return;
    //           }

    //           Debug.Log($"{gameObject.name} reached destination!!!");
    //           _isAtDestination = true;
    //           agent.isStopped = true;
    //       }
    //   }
}

[Serializable]
public class TurnToFaceSequence : Sequence
{
    public Transform Target;
    public float TurnDuration = 1f;

    protected override async void OnStart(NPC npc)
    {
        if (Target == null) {
            Debug.LogWarning("No target assigned for TurnToFaceSequence, continuing to next sequence.");
            npc.StartNextSequence();
            return;
        }

        await npc.TurnToFaceAsyc(Target, TurnDuration); // This is cancelled internally

        if (!npc.CancelTokenSource.IsCancellationRequested)
            npc.StartNextSequence();
    }
}