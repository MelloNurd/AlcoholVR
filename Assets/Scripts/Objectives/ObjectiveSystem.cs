using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Objective
{
    public string text = "Do something important";
    public int priority = 0;
    public Transform worldPosition;

    public Objective(string text, int priority, Transform position)
    {
        this.text = text;
        this.priority = priority;
        worldPosition = position;
    }
}

public class ObjectiveSystem : MonoBehaviour
{
    public enum Statuses
    {
        ToDo,
        InProgress,
        Completed,
        Failed
    }

    public Objective objectiveDetails;
    public bool hasObjective => !objectiveDetails.text.IsBlank();
    public Statuses currentStatus = Statuses.ToDo;

    public UnityEvent OnBegin = new();
    public UnityEvent OnCompletion = new();
    public UnityEvent OnFailure = new();
    public UnityEvent OnAdvance = new(); // Will be invoked on start, completion, and failure

    public void ChangeObjective(Objective newObjective, bool resetStatus = true)
    {
        objectiveDetails = newObjective;
        if (resetStatus)
        {
            currentStatus = Statuses.ToDo;
        }
    }

    public void Begin() // This is called by the dialogue system, when npc is first interacted with
    {
        currentStatus = Statuses.InProgress;

        OnAdvance?.Invoke();
        OnBegin?.Invoke();
    }

    public void Complete()
    {
        currentStatus = Statuses.Completed;

        OnAdvance?.Invoke();
        OnCompletion?.Invoke();
    }

    public void Fail()
    {
        if (currentStatus == Statuses.Completed) return; // Cannot fail a completed objective

        currentStatus = Statuses.Failed;

        OnAdvance?.Invoke();
        OnFailure?.Invoke();
    }
}
