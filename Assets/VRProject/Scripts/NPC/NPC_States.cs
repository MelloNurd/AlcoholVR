using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public abstract class NPC_BaseState
{
    protected NPC_SM npc;

    public NPC_BaseState(NPC_SM npc)
    {
        this.npc = npc;
    }

    // NOTE: These can be overridden as async
    public virtual void EnterState() { }
    public virtual void UpdateState() { }
    public virtual void ExitState() { }
}

public class NPC_IdleState : NPC_BaseState
{
    public NPC_IdleState(NPC_SM npc) : base(npc) { } // constructor
    public override void EnterState()
    {
        base.EnterState();
        npc.PlayIdleAnimation();
    }
}

public class NPC_WalkState : NPC_BaseState
{
    public NPC_WalkState(NPC_SM npc) : base(npc) { } // constructor
    
    public override void EnterState()
    {
        base.EnterState();

        if(npc.agent == null || !npc.agent.enabled)
        {
            npc.SwitchState(NPC_SM.States.Idle);
            return;
        }

        npc.PlayWalkAnimation();

        npc.agent.destination = npc.currentCheckpoint.transform.position;

        CheckIfAtDestination(notFirstArrival: false); // Do an inital check in case we're already there. If we are, don't reset actions left (assume some may have played already)
    }

    public override void UpdateState()
    {
        base.UpdateState();
        CheckIfAtDestination();
    }

    public override void ExitState()
    {
        base.ExitState();
        npc.lookAt.isLooking = false;
    }

    private void CheckIfAtDestination(bool notFirstArrival = true)
    {
        if (npc.agent.IsAtDestination())
        if (!npc.agent.pathPending && npc.agent.remainingDistance <= npc.agent.stoppingDistance)
        {
            if (!npc.agent.hasPath || npc.agent.velocity.sqrMagnitude == 0f)
            {
                if (notFirstArrival) npc.actionsLeft = (npc.currentCheckpoint.container._actions.Count <= 0) ? 0 : Random.Range(npc.currentCheckpoint.container.minActions, npc.currentCheckpoint.container.maxActions + 1);
                else npc.OnCheckpointArrive?.Invoke();

                if (npc.usesCheckpoints)
                    npc.SwitchState(NPC_SM.States.Checkpoint);
                else
                    npc.SwitchState(NPC_SM.States.Idle);
            }
        }
    }
}

public class NPC_CheckpointState : NPC_BaseState
{
    public NPC_CheckpointState(NPC_SM npc) : base(npc) { } // constructor

    CancellationTokenSource _cancelToken;

    public override void EnterState()
    {
        _cancelToken = new CancellationTokenSource();
        base.EnterState();
        npc.PlayIdleAnimation();
        ProcessActions();
    }

    private async void ProcessActions()
    {
        await UniTask.Delay(npc.currentCheckpoint.container.AnimationDelayInMS, cancellationToken: _cancelToken.Token).SuppressCancellationThrow(); // Always start with a delay
        if (_cancelToken.IsCancellationRequested) return;

        int loops = npc.actionsLeft;
        for (int i = 0; i < loops; i++)
        {
            await ProcessNextAction(); // Wait for an action and delay again afterwards
            await UniTask.Delay(npc.currentCheckpoint.container.AnimationDelayInMS, cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
        }

        if (_cancelToken.IsCancellationRequested) return; // If we were cancelled, don't proceed to next checkpoint

        npc.SetNextCheckpoint();
        npc.SwitchState(NPC_SM.States.Walk);
        npc.OnCheckpointLeave?.Invoke();
    }

    private async UniTask ProcessNextAction()
    {
        int halfDuration = npc.PlayNextAction() / 2; // Play the action and get back the duration halved

        // Doing this weirdly, but basically we are subtracting the actionsLeft halfway through the action play time. So if it is only 25% played and we interrupt, it will play again.
        await UniTask.Delay(halfDuration, cancellationToken: _cancelToken.Token).SuppressCancellationThrow(); // Where we play the new action
        if (_cancelToken.IsCancellationRequested) return; 

        npc.actionsLeft--;

        await UniTask.Delay(halfDuration, cancellationToken: _cancelToken.Token).SuppressCancellationThrow(); // Where we play the new action
        if (_cancelToken.IsCancellationRequested) return;

        npc.PlayIdleAnimation(); // Once it's done, go back to idle animation
    }

    public override void ExitState()
    {
        base.ExitState();
        _cancelToken.Cancel();
    }
}

public class NPC_InteractState : NPC_BaseState
{
    public NPC_InteractState(NPC_SM npc) : base(npc) { } // constructor

    private InteractableNPC_SM _interactableNPC;
    Vector3 storedDestination;

    Vector3 initialRotation;

    public override async void EnterState()
    {
        base.EnterState();

        _interactableNPC = npc as InteractableNPC_SM; // Cast it to InteractableNPC_SM
        if(_interactableNPC == null)
        {
            Debug.LogError($"NPC {npc.gameObject.name} is not of type InteractableNPC_SM");
            return;
        }

        if(_interactableNPC.alwaysUseStartAnim)
        {
            _interactableNPC.PlayAnimation(_interactableNPC.startAnim.name);
        }
        else
        {
            _interactableNPC.PlayIdleAnimation();
        }

        storedDestination = _interactableNPC.agent.destination;
        if(_interactableNPC.agent != null && _interactableNPC.agent.enabled)
        {
            _interactableNPC.agent.destination = _interactableNPC.bodyObj.transform.position;
        }

        if(_interactableNPC.turnBodyToFacePlayer)
        {
            initialRotation = _interactableNPC.bodyObj.transform.forward;
            Vector3 directionToPlayer = (Player.Instance.CamPosition - _interactableNPC.bodyObj.transform.position).WithY(0);
            await Tween.Rotation(_interactableNPC.bodyObj.transform, Quaternion.LookRotation(directionToPlayer), 0.3f);
        }
        if(_interactableNPC.turnHeadToFacePlayer)
        {
            _interactableNPC.lookAt.LookAtPlayer();
        }

        if (_interactableNPC.currentState != _interactableNPC.states[NPC_SM.States.Interact]) return; // If state changed during the wait, don't start dialogue

        Debug.Log("STARTING DIALOGUE WITH " + _interactableNPC.gameObject.name);
        _interactableNPC.StartDialogue();
    }

    public override void ExitState()
    {
        base.ExitState();
        Player.Instance.IsInteractingWithNPC = false;
        Tween.CompleteAll(this);
        if (_interactableNPC.agent != null && _interactableNPC.agent.enabled) _interactableNPC.agent.destination = storedDestination;

        if (_interactableNPC.turnBodyToFacePlayer && _interactableNPC.turnBackAfterDialogue)
        {
            Tween.Rotation(_interactableNPC.bodyObj.transform, Quaternion.LookRotation(initialRotation), 0.3f);
        }
        if(_interactableNPC.turnHeadToFacePlayer)
        {
            _interactableNPC.lookAt.isLooking = false;
        }
    }
}