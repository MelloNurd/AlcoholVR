using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class Sequence
{
    public bool nextSequenceOnEnd = true;

    [HideInInspector] public NPC npc;

    protected CancellationTokenSource ct = new();

    public async void Execute()
    {
        await ExecuteSequence();
        if (ct.IsCancellationRequested)
        {
            Debug.Log("Sequence execution cancelled.", npc.gameObject);
        }

        npc._currentState = NPC.NPCState.Idle;

        if (nextSequenceOnEnd)
        {
            npc.ExecuteNextSequence();
        }
    }

    protected abstract UniTask ExecuteSequence();
}

[Serializable] 
public class AnimateSequence : Sequence 
{
    // Fields
    public AnimationClip animation;
    
    // Constructors
    public AnimateSequence() { }
    public AnimateSequence(AnimationClip animation, bool nextSequenceOnEnd = true)
    {
        this.animation = animation;
        this.nextSequenceOnEnd = nextSequenceOnEnd;
    }

    // Methods
    protected override async UniTask ExecuteSequence()
    {
        if (animation == null)
        {
            Debug.LogWarning("AnimateSequence: No animation clip assigned.", npc.gameObject);
            return;
        }

        npc.PlayAnimation(animation);

        await UniTask.Delay(Mathf.RoundToInt(animation.length * 1000), cancellationToken: ct.Token).SuppressCancellationThrow();
    }
}

[Serializable]
public class DialogueSequence : Sequence
{
    // Fields
    public Dialogue dialogue;

    // Constructors
    public DialogueSequence() { }
    public DialogueSequence(Dialogue dialogue, bool nextSequenceOnEnd = true)
    {
        this.dialogue = dialogue;
        this.nextSequenceOnEnd = nextSequenceOnEnd;
    }

    // Methods
    protected override UniTask ExecuteSequence()
    {
        //if (npc.dialogueSystem == null || dialogue == null) return;
        
        //// Face player logic
        //if (npc.turnBodyToFacePlayer && Player.Instance != null)
        //{
        //    Vector3 dir = (Player.Instance.Camera.transform.position - npc.bodyObj.transform.position).WithY(0);
        //    Tween.Rotation(npc.bodyObj.transform, Quaternion.LookRotation(dir), 0.3f);
        //}
        //if (npc.turnHeadToFacePlayer) npc.lookAt.LookAtPlayer();

        //if(Player.Instance != null) await UniTask.WaitUntil(() => !Player.Instance.IsInDialogue, cancellationToken: ct);

        //bool dialogueFinished = false;
        //void OnEnd() => dialogueFinished = true;
        
        //npc.dialogueSystem.onEnd.AddListener(OnEnd);
        //npc.dialogueSystem.StartDialogue(dialogue);
        
        //await UniTask.WaitUntil(() => !npc.dialogueSystem.IsDialogueActive || dialogueFinished, cancellationToken: ct).SuppressCancellationThrow();
        
        //npc.dialogueSystem.onEnd.RemoveListener(OnEnd);
        //if (npc.turnHeadToFacePlayer) npc.lookAt.isLooking = false;
        //if(Player.Instance != null) Player.Instance.EnableMovement();

        return UniTask.CompletedTask;
    }
}

[Serializable]
public class WalkSequence : Sequence
{
    // Fields
    public Transform destination;

    // Constructors
    public WalkSequence() { }
    public WalkSequence(Transform destination, bool nextSequenceOnEnd = true)
    {
        this.destination = destination;
        this.nextSequenceOnEnd = nextSequenceOnEnd;
    }

    // Methods
    protected override UniTask ExecuteSequence()
    {
        //npc.agent.SetDestinationToClosestPoint(destination.position, 1.5f);
        //npc.agent.isStopped = false;
        //npc.PlayWalkAnimation();
        //await UniTask.WaitUntil(() => npc.agent.IsAtDestination(0.01f), cancellationToken: ct).SuppressCancellationThrow();
        //npc.PlayIdleAnimation();
        //npc.agent.isStopped = true;
        return UniTask.CompletedTask;
    }
}

[Serializable]
public class WaitSequence : Sequence
{
    // Fields
    public bool playIdleAnimation = true;
    public float secondsToWait;
    
    // Constructors
    public WaitSequence() { }
    public WaitSequence(float secondsToWait, bool playIdleAnimation = true, bool nextSequenceOnEnd = true)
    {
        this.secondsToWait = secondsToWait; 
        this.playIdleAnimation = playIdleAnimation;
        this.nextSequenceOnEnd = nextSequenceOnEnd;
    }

    // Methods
    protected override async UniTask ExecuteSequence()
    {
        if (playIdleAnimation)
            npc.PlayIdleAnimation();

        await UniTask.Delay(secondsToWait.ToMS(), cancellationToken: ct.Token).SuppressCancellationThrow();
    }
}

[Serializable]
public class TurnToFaceSequence : Sequence
{
    // Fields
    public GameObject calculateDirection;
    public Vector3 directionToFace;
    public float turnSpeed;

    // Constructors
    public TurnToFaceSequence() { }
    public TurnToFaceSequence(Vector3 directionToFace, float turnSpeed = 0.3f, bool nextSequenceOnEnd = true)
    {
        this.directionToFace = directionToFace;
        this.turnSpeed = turnSpeed;
        this.nextSequenceOnEnd = nextSequenceOnEnd;
    }

    // Methods
    protected override UniTask ExecuteSequence()
    {
        //Tween.StopAll(npc.bodyObj.transform);
        //await Tween.Rotation(npc.bodyObj.transform, Quaternion.LookRotation(directionToFace.normalized), turnSpeed).ToUniTask(cancellationToken: ct);
        return UniTask.CompletedTask;
    }
}

// WalkToPlayer is simpler as it usually doesn't need data passed in
[Serializable]
public class WalkToPlayerSequence : Sequence
{
    // Fields

    // Constructors
    public WalkToPlayerSequence() { }

    // Methods
    protected override UniTask ExecuteSequence()
    {
        if (Player.Instance == null || npc.Agent == null)
            return UniTask.CompletedTask;

        //npc.agent.isStopped = false;
        //npc.PlayWalkAnimation();

        //float lastUpdateTime = 0f;

        //// Loop until cancelled or broken out
        //while (!ct.IsCancellationRequested)
        //{
        //    lastUpdateTime += Time.deltaTime;

        //    // Update destination every 0.5s
        //    if (lastUpdateTime > 0.5f)
        //    {
        //        lastUpdateTime = 0f;
        //        Vector3 inFrontOfPlayer = Player.Instance.CamPosition + Player.Instance.Camera.transform.forward.WithY(0).normalized * 1.5f;
        //        npc.agent.SetDestinationToClosestPoint(inFrontOfPlayer);
        //    }

        //    // Check distance
        //    // If near player (and not a false positive far away check)
        //    if (npc.agent.IsAtDestination(0.01f) && Vector3.Distance(npc.transform.position, Player.Instance.Position) <= 2.5f)
        //    {
        //        break; // Reached
        //    }

        //    await UniTask.Yield(PlayerLoopTiming.Update, ct).SuppressCancellationThrow();
        //}

        //npc.PlayIdleAnimation();
        //npc.agent.isStopped = true;
        return UniTask.CompletedTask;
    }
}

// WalkToPlayer is simpler as it usually doesn't need data passed in
[Serializable]
public class UnityEventSequence : Sequence
{
    // Fields
    public UnityEvent unityEvent = new UnityEvent();

    // Constructors
    public UnityEventSequence() { }

    // Methods
    protected override UniTask ExecuteSequence()
    {
        unityEvent?.Invoke();

        if (nextSequenceOnEnd)
        {
            npc.ExecuteNextSequence();
        }

        return UniTask.CompletedTask;
    }
}