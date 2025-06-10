using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    // Singleton instance
    public static ObjectiveManager Instance { get; private set; }

    public List<Objective> objectives = new List<Objective>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void AddObjective(Objective objective)
    {
        Debug.Log($"Adding Objective: {objective.objectiveDetails.objectiveText}");
        objectives.Add(objective);
        ObjectiveHUDManager.Instance.AddObjectiveToHUD(objective);
    }

    public void RemoveObjective(Objective objective)
    {
        Debug.Log($"Removing Objective: {objective.objectiveDetails.objectiveText}");
        objectives.Remove(objective);
    }
}
