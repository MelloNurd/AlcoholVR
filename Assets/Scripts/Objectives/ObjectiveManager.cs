using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    public List<(Objective, LineRenderer)> objectives = new();
    private List<Objective> _sortedObjectives = new();

    private NavMeshPath _navPath;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.parent);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            // Clear all objectives when a new scene is loaded
            Debug.Log($"Scene loaded from ObjectiveManager script");
            RemoveAllObjectives();
        };
    }

    private void Update()
    {
        HandleTracking();

        // DEbug
        if(Phone.Instance == null || !Phone.Instance.IsActive) return;
        if (Keyboard.current.digit1Key.wasPressedThisFrame && objectives.Count > 0)
        {
            if (objectives[0].Item1.Ui.buttonState)
                objectives[0].Item1.Ui.DisableButton();
            else
                objectives[0].Item1.Ui.EnableButton();
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame && objectives.Count > 1)
        {
            if (objectives[1].Item1.Ui.buttonState)
                objectives[1].Item1.Ui.DisableButton();
            else
                objectives[1].Item1.Ui.EnableButton();
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame && objectives.Count > 2)
        {
            if (objectives[2].Item1.Ui.buttonState)
                objectives[2].Item1.Ui.DisableButton();
            else
                objectives[2].Item1.Ui.EnableButton();
        }
        else if (Keyboard.current.digit4Key.wasPressedThisFrame && objectives.Count > 3)
        {
            if (objectives[3].Item1.Ui.buttonState)
                objectives[3].Item1.Ui.DisableButton();
            else
                objectives[3].Item1.Ui.EnableButton();
        }
        else if (Keyboard.current.digit5Key.wasPressedThisFrame && objectives.Count > 4)
        {
            if (objectives[4].Item1.Ui.buttonState)
                objectives[4].Item1.Ui.DisableButton();
            else
                objectives[4].Item1.Ui.EnableButton();
        }
    }

    public ObjectiveSystem CreateObjectiveObject(Objective objective)
    {
        GameObject newObj = new GameObject("New objective", typeof(ObjectiveSystem));
        ObjectiveSystem objectiveSystem = newObj.GetComponent<ObjectiveSystem>();
        objectiveSystem.objective = objective;
        objectiveSystem.Begin();
        return objectiveSystem;
    }

    public void AddObjective(Objective objective)
    {
        if (objective == null) return;

        // Check if objective already exists in any tuple
        foreach (var tuple in objectives)
        {
            if (tuple.Item1 == objective) return;
        }

        var temp = new GameObject("LineRenderer for " + objective.text, typeof(LineRenderer));
        temp.transform.parent = transform;
        LineRenderer lineRenderer = temp.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = 0.04f;
            lineRenderer.endWidth = 0.04f;
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
            lineRenderer.numCornerVertices = 6;
            lineRenderer.numCapVertices = 6;
            lineRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit")); // Use a simple shader for visibility
            lineRenderer.material.color = Color.white;
        }

        objectives.Add((objective, lineRenderer));
        if (Phone.Instance != null)
        {
            Debug.Log("Loading from objective addition");
            Phone.Instance.LoadObjectives();
        }

        Debug.Log("Added new objective: " + objective.text);
    }

    public void RemoveAllObjectives()
    {
        Debug.Log("Removing all objectives");
        for (int i = objectives.Count - 1; i >= 0; i--)
        {
            // Destroy the LineRenderer GameObject
            if (objectives[i].Item2 != null)
            {
                Destroy(objectives[i].Item2.gameObject);
            }
            objectives[i].Item2.positionCount = 0; // Clear the positions
            objectives.RemoveAt(i);
        }
        objectives.Clear();
    }

    public void ClearCompleteObjectives()
    {
        for (int i = objectives.Count - 1; i >= 0; i--)
        {
            if (objectives[i].Item1.IsComplete == true)
            {
                RemoveObjective(objectives[i].Item1);
            }
        }
    }

    public void RemoveObjective(Objective objective)
    {
        if (objective == null) return;

        for (int i = 0; i < objectives.Count; i++)
        {
            if (objectives[i].Item1 == objective)
            {
                // Destroy the LineRenderer GameObject
                if (objectives[i].Item2 != null)
                {
                    Destroy(objectives[i].Item2.gameObject);
                }

                objectives[i].Item2.positionCount = 0; // Clear the positions
                objectives.RemoveAt(i);
                return;
            }
        }

        if (Phone.Instance != null)
        {
            Debug.Log("Loading from objective addition");
            Phone.Instance.LoadObjectives();
        }
    }

    public void HideAllPaths()
    {
        foreach (var tuple in objectives)
        {
            tuple.Item2.positionCount = 0; // Clear the line renderer
        }
    }

    public void DisableAllTracking()
    {
        foreach (var tuple in objectives)
        {
            tuple.Item1.IsTracking = false;
            tuple.Item2.positionCount = 0; // Clear the line renderer
        }
    }

    private void HandleTracking()
    {
        if (Phone.Instance == null || !Phone.Instance.IsActive) return; // Only update paths when the phone is active

        foreach ((Objective objective, LineRenderer lr) in objectives)
        {
            if (!objective.IsTracking)
            {
                lr.positionCount = 0;
                continue;
            }

            if (objective.CalculatePath(Player.Instance.CamPosition, out NavMeshPath path))
            {
                // First smooth the original path
                var smoothedPath = Utilities.BevelCorners(path.corners, radius: 0.75f);
                
                // Add intermediate points for better terrain following
                List<Vector3> densePath = DensifyPath(smoothedPath);
                
                // Create array for terrain-adjusted points
                Vector3[] adjustedPathPoints = new Vector3[densePath.Count];
                
                // Process each point to follow terrain
                for (int i = 0; i < densePath.Count; i++)
                {   
                    adjustedPathPoints[i] = AdjustPointToGround(densePath[i]);
                }

                // Set the line renderer positions
                lr.positionCount = adjustedPathPoints.Length + 1; // +1 for objective point
                lr.SetPositions(adjustedPathPoints);
                // Adjust the final objective point to ground
                lr.SetPosition(lr.positionCount - 1, AdjustPointToGround(objective.point));

                // Scrolling effect
                lr.material.mainTextureOffset = new Vector2(Time.time * 0.1f, 0);
            }
            else
            {
                Debug.Log("Could not calculate path for objective: " + objective.text);
            }
        }
    }

    // Add intermediate points to better follow terrain contours
    private List<Vector3> DensifyPath(Vector3[] originalPath)
    {
        List<Vector3> densePath = new List<Vector3>();
        
        for (int i = 0; i < originalPath.Length - 1; i++)
        {
            Vector3 start = originalPath[i];
            Vector3 end = originalPath[i + 1];
            float distance = Vector3.Distance(start, end);
            
            // Add current point
            densePath.Add(start);
            
            // If points are far apart, add intermediate points
            if (distance > 1.0f)
            {
                int pointCount = Mathf.CeilToInt(distance / 0.5f);
                for (int j = 1; j < pointCount; j++)
                {
                    float t = j / (float)pointCount;
                    densePath.Add(Vector3.Lerp(start, end, t));
                }
            }
        }
        
        // Add the last point
        if (originalPath.Length > 0)
        {
            densePath.Add(originalPath[originalPath.Length - 1]);
        }
        
        return densePath;
    }

    // Method to adjust a point to follow ground accurately
    private Vector3 AdjustPointToGround(Vector3 point)
    {
        // Start raycast from well above the point to handle elevation differences
        float rayHeight = 1f;
        Vector3 rayOrigin = new Vector3(point.x, point.y + rayHeight, point.z);
        
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayHeight + 10f, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
        {
            // Calculate slope angle from surface normal
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            
            // Adjust height offset based on slope steepness
            float heightOffset = 0.05f + (slopeAngle / 90f) * 0.15f;
            
            return new Vector3(point.x, hit.point.y + heightOffset, point.z);
        }
        
        // Fallback if raycast missed
        return point;
    }

    public List<Objective> GetSortedList()
    {
        _sortedObjectives.Clear();
        _sortedObjectives.AddRange(objectives.Select(tuple => tuple.Item1));
        _sortedObjectives.Sort((a, b) => a.priority.CompareTo(b.priority));
        return _sortedObjectives;
    }
}
