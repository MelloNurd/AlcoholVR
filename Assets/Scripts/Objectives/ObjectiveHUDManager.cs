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

    public void AddObjectiveToHUD(Objective objective)
    {
        Debug.Log($"Adding Objective to HUD: {objective.objectiveDetails.objectiveText}");
        GameObject objectiveUI = Instantiate(ObjectiveUIPrefab, transform);
        TextMeshProUGUI objectiveText = objectiveUI.GetComponentInChildren<TextMeshProUGUI>();
        objectiveText.text = objective.objectiveDetails.objectiveText;
    }
}
