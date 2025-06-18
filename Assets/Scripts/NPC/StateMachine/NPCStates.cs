using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public abstract class NPCBaseState
{
    protected NPCStateMachine npc;

    public NPCBaseState(NPCStateMachine npc)
    {
        this.npc = npc;
    }

    // NOTE: These can be overridden as async
    public virtual void EnterState() { }
    public virtual void UpdateState() { }
    public virtual void ExitState() { }
}

public class NPCIdleState : NPCBaseState
{
    public NPCIdleState(NPCStateMachine npc) : base(npc) { } // constructor
    public override void EnterState()
    {
        base.EnterState();
        npc.PlayIdleAnimation();
    }
}

public class NPCWalkState : NPCBaseState
{
    public NPCWalkState(NPCStateMachine npc) : base(npc) { } // constructor
    public override void EnterState()
    {
        base.EnterState();
        npc.PlayWalkAnimation();
        npc.agent.destination = npc.currentCheckpoint.transform.position;

        CheckIfAtDestination(notFirstArrival: false); // Do an inital check in case we're already there. If we are, don't reset actions left (assume some may have played already)
    }

    public override void UpdateState()
    {
        base.UpdateState();
        CheckIfAtDestination();
    }

    private void CheckIfAtDestination(bool notFirstArrival = true)
    {
        if (!npc.agent.pathPending && npc.agent.remainingDistance <= npc.agent.stoppingDistance)
        {
            if (!npc.agent.hasPath || npc.agent.velocity.sqrMagnitude == 0f)
            {
                if (notFirstArrival) npc.actionsLeft = (npc.currentCheckpoint._actions.Count <= 0) ? 0 : Random.Range(npc.currentCheckpoint.minActions, npc.currentCheckpoint.maxActions + 1);
                else npc.OnCheckpointArrive?.Invoke();
                npc.SwitchState(NPCStateMachine.States.Checkpoint);
            }
        }
    }
}

public class NPCCheckpointState : NPCBaseState
{
    public NPCCheckpointState(NPCStateMachine npc) : base(npc) { } // constructor

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
        await UniTask.Delay(npc.currentCheckpoint.AnimationDelayInMS, cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
        if (_cancelToken.IsCancellationRequested) return;

        int loops = npc.actionsLeft;
        for (int i = 0; i < loops; i++)
        {
            await ProcessNextAction();
            await UniTask.Delay(npc.currentCheckpoint.AnimationDelayInMS, cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
        }

        if (_cancelToken.IsCancellationRequested) return; // If we were cancelled, don't proceed to next checkpoint

        npc.SetNextCheckpoint();
        npc.SwitchState(NPCStateMachine.States.Walk);
        npc.OnCheckpointLeave?.Invoke();
    }

    private async UniTask ProcessNextAction()
    {
        int halfDuration = npc.PlayNextAction() / 2;

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

public class NPCInteractState : NPCBaseState
{
    public NPCInteractState(NPCStateMachine npc) : base(npc) { } // constructor

    Vector3 tempDestination;

    public override void EnterState()
    {
        base.EnterState();
        npc.PlayIdleAnimation();
        tempDestination = npc.agent.destination;
        npc.agent.destination = npc.bodyPosition;
    }

    public override void ExitState()
    {
        base.ExitState();
        npc.agent.destination = tempDestination;
    }   
}