using System;
using UnityEngine;
using UnityEngine.Events;

public enum QuestState
{
    NotStarted,
    Incomplete,
    Complete,
    Failed
}

[Serializable]
public class Quest
{
    public QuestState State = QuestState.NotStarted;

    public bool CanFailBeforeStarting = true;

    public UnityEvent OnCompletion = new();
    public UnityEvent OnFailure = new();
    public UnityEvent OnAdvance = new(); // Will be invoked on both start, completion, and failure

    public void Start()
    {
        OnAdvance?.Invoke();
        State = QuestState.Incomplete;
    }

    public void Complete()
    {
        OnAdvance?.Invoke();
        State = QuestState.Complete;
        OnCompletion?.Invoke();
    }

    public void Fail()
    {
        if(!CanFailBeforeStarting && State == QuestState.NotStarted) return;
        if (State == QuestState.Complete) return; // Cannot fail a completed quest

        OnAdvance?.Invoke();
        State = QuestState.Failed;
        OnFailure.Invoke();
    }
}
