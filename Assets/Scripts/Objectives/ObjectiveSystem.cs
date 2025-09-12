using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[Serializable]
public class Objective
{
    public string text = "Do something important";
    public int priority = 0;
    public Transform point;
    
    public bool IsTracking { get; set; } = false;
    public ObjectiveUI Ui { get; set; }

    public Objective(string text, int priority, Transform position)
    {
        this.text = text;
        this.priority = priority;
        point = position;
    }

    public bool CalculatePath(Vector3 position, out NavMeshPath path)
    {
        path = new NavMeshPath();

        // Ensure both the start and end positions are on the NavMesh
        position = NavMesh.SamplePosition(position, out NavMeshHit hit, 8f, NavMesh.AllAreas) ? hit.position : position;
        point.position = NavMesh.SamplePosition(point.position, out hit, 8f, NavMesh.AllAreas) ? hit.position : point.position;

        return NavMesh.CalculatePath(position, point.position, NavMesh.AllAreas, path);
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

    public Objective objective;
    public bool hasObjective => !objective.text.IsBlank();
    public Statuses currentStatus = Statuses.ToDo;

    public UnityEvent OnBegin = new();
    public UnityEvent OnCompletion = new();
    public UnityEvent OnFailure = new();
    public UnityEvent OnAdvance = new(); // Will be invoked on start, completion, and failure

    private GameObject _player;

    private void Awake()
    {
        _player = Camera.main.gameObject;
    }

    private void Start()
    {
        //OnAdvance.AddListener(Phone.Instance.LoadObjectives); // This will force the list to update when any objective is advanced
    }

    private string SetTags(string dirtyText)
    {
        dirtyText = dirtyText.Replace("{object}", gameObject.name);
        return dirtyText;
    }

    public void ChangeObjective(Objective newObjective, bool resetStatus = true)
    {
        objective = newObjective;
        if (resetStatus)
        {
            currentStatus = Statuses.ToDo;
        }
    }

    public void Begin() // This is called by the dialogue system, when npc is first interacted with
    {
        currentStatus = Statuses.InProgress;
        if(ObjectiveManager.Instance != null && hasObjective)
        {
            objective.text = SetTags(objective.text);
            ObjectiveManager.Instance.AddObjective(objective);
        }

        OnAdvance?.Invoke();
        OnBegin?.Invoke();
    }

    public void Complete()
    {
        if(currentStatus == Statuses.Completed) return; // Cannot complete an already completed objective

        currentStatus = Statuses.Completed;
        if (ObjectiveManager.Instance != null && hasObjective)
        {
            ObjectiveManager.Instance.RemoveObjective(objective);
        }

        OnAdvance?.Invoke();
        OnCompletion?.Invoke();
    }

    public void Fail()
    {
        if (currentStatus == Statuses.Completed) return; // Cannot fail a completed objective
        if (ObjectiveManager.Instance != null && hasObjective)
        {
            ObjectiveManager.Instance.RemoveObjective(objective);
        }

        currentStatus = Statuses.Failed;

        OnAdvance?.Invoke();
        OnFailure?.Invoke();
    }
}
