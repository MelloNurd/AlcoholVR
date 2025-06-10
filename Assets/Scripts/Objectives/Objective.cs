using UnityEngine;

public enum ObjectiveStatus
{
    ToDo,
    InProgress,
    Completed,
    Failed 
}

public class Objective : MonoBehaviour
{
    public ObjectiveDetails objectiveDetails;
    public ObjectiveStatus status = ObjectiveStatus.ToDo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void StartObjective()
    {
        Debug.Log($"Starting Objective: {objectiveDetails.objectiveText}");
        status = ObjectiveStatus.InProgress;
    }

    public void CompleteObjective()
    {
        Debug.Log($"Completing Objective: {objectiveDetails.objectiveText}");
        status = ObjectiveStatus.Completed;
        // Trigger any completion events or logic here
    }

    public void FailObjective()
    {
        Debug.Log($"Failing Objective: {objectiveDetails.objectiveText}");
        status = ObjectiveStatus.Failed;
        // Trigger any failure events or logic here
    }
}
