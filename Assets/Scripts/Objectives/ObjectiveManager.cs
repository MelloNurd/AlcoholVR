using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    public List<(Objective, LineRenderer)> objectives = new();
    private List<Objective> _sortedObjectives = new();

    private NavMeshPath _navPath;

    private Transform _playerTransform;

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
        }

        _playerTransform = Camera.main.transform;
    }

    private void Update()
    {
        HandleTracking();
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
            lineRenderer.startWidth = 0.025f;
            lineRenderer.endWidth = 0.025f;
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
            lineRenderer.numCornerVertices = 6;
            lineRenderer.numCapVertices = 6;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Use a simple shader for visibility
        }

        objectives.Add((objective, lineRenderer));
        Phone.Instance.LoadObjectives();

        Debug.Log("Added new objective: " + objective.text);
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
        Phone.Instance.LoadObjectives();
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

            if (objective.CalculatePath(_playerTransform.position, out NavMeshPath path))
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
                lr.SetPosition(lr.positionCount - 1, objective.point.position); // Set last position to objective
                
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
        float rayHeight = 10f;
        Vector3 rayOrigin = new Vector3(point.x, point.y + rayHeight, point.z);
        
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayHeight + 10f, LayerMask.GetMask("Ground")))
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
