using UnityEngine;
using System;

public enum TriggerType
{
    EnterArea,
    Interact,
    Timer
}

[CreateAssetMenu(fileName = "NewObjectiveDetails", menuName = "ObjectivesDetails/ObjectiveDetail")]
public class ObjectiveDetails : ScriptableObject
{
    [Header("Basic Info")]
    // Text for the objective
    public string objectiveText = "Do something important";
    // Priority level for sorting HUD list
    public int priorityLevel = 0;

    [Header("World Position")]
    // Position in the world where the objective is located
    public Vector3 worldPosition;

    [Header("Trigger Type")]
    // Type of trigger that activates this objective
    public TriggerType triggerType;
}
