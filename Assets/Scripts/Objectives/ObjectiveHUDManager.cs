using TMPro;
using UnityEngine;

public class ObjectiveHUDManager : MonoBehaviour
{
    public static ObjectiveHUDManager Instance { get; private set; }

    [SerializeField] GameObject ObjectiveUIPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddObjectiveToHUD(ObjectiveSystem objective)
    {
        Debug.Log($"Adding Objective to HUD: {objective.objectiveDetails.text}");
        GameObject objectiveUI = Instantiate(ObjectiveUIPrefab, transform);
        TextMeshProUGUI objectiveText = objectiveUI.GetComponentInChildren<TextMeshProUGUI>();
        objectiveText.text = objective.objectiveDetails.text + "\nStatus: " + objective.currentStatus.ToString() + "Priority level: " + objective.objectiveDetails.priority;
        SortByPriorityValue();
    }

    public void SortByPriorityValue()
    {
        //go through list and place objectives with lower value above others
        Debug.Log("Sorting objectives by priority value.");
        var objectives = ObjectiveManager.Instance.objectives;
        objectives.Sort((a, b) => a.objectiveDetails.priority.CompareTo(b.objectiveDetails.priority));
        // Clear current HUD
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        // Re-add sorted objectives to HUD
        foreach (var objective in objectives)
        {
            RefillHUD(objective);
        }
    }

    public void RefillHUD(ObjectiveSystem objective)
    {
        Debug.Log($"Adding Objective to HUD: {objective.objectiveDetails.text}");
        GameObject objectiveUI = Instantiate(ObjectiveUIPrefab, transform);
        TextMeshProUGUI objectiveText = objectiveUI.GetComponentInChildren<TextMeshProUGUI>();
        objectiveText.text = objective.objectiveDetails.text + "\nStatus: " + objective.currentStatus.ToString() + "Priority level: " + objective.objectiveDetails.priority;
    }
}
