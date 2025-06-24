using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public static class Utilities
{
    #region Position Generation

    /// <summary>
    /// Returns a random point within a circle of the specified radius around the provided position.
    /// </summary>
    /// <param name="position">The center point of the circle</param>
    /// <param name="radius">The radius of the circle</param>
    /// <returns>A random point inside the circle</returns>
    public static Vector3 GetPointInCircle(Vector3 position, float radius)
    {
        return position + ((Vector3)Random.insideUnitCircle * radius);
    }

    /// <summary>
    /// Returns a random point within a ring (between min and max radius) around the provided position.
    /// </summary>
    /// <param name="position">The center point of the ring</param>
    /// <param name="minRadius">The minimum radius of the ring</param>
    /// <param name="maxRadius">The maximum radius of the ring</param>
    /// <returns>A random point inside the ring area</returns>
    public static Vector3 GetPointInCircle(Vector3 position, float minRadius, float maxRadius)
    {
        if (minRadius > maxRadius)
        {
            Debug.LogError("Min radius cannot be greater than max radius.");
            return position;
        }

        int threshold = 0;

        Vector3 newPos;
        do
        {
            newPos = GetPointInCircle(position, maxRadius);
            threshold++;
        }
        while (Vector3.Distance(newPos, position) <= minRadius && threshold < 100);

        // Threshold is in place to prevent any potential infinite loops
        if (threshold >= 100)
        {
            Debug.LogWarning("Threshold reached while trying to find a point in circle.");
            return position;
        }

        return newPos;
    }

    /// <summary>
    /// Gets a point in a circle that has no colliders at the position.
    /// </summary>
    /// <param name="position">The center point of the circle</param>
    /// <param name="radius">The radius of the circle</param>
    /// <returns>A random empty point inside the circle</returns>
    public static Vector3 GetEmptyPointInCircle(Vector3 position, float radius) => GetEmptyPointInCircle(position, 0f, radius);

    /// <summary>
    /// Gets a point in a ring (between min and max radius) that has no colliders at the position.
    /// </summary>
    /// <param name="position">The center point of the ring</param>
    /// <param name="minRadius">The minimum radius of the ring</param>
    /// <param name="maxRadius">The maximum radius of the ring</param>
    /// <returns>A random empty point inside the ring area</returns>
    public static Vector3 GetEmptyPointInCircle(Vector3 position, float minRadius, float maxRadius)
    {
        if (minRadius > maxRadius)
        {
            Debug.LogError("Min radius cannot be greater than max radius.");
            return position;
        }
        int threshold = 0;
        Vector3 newPos;
        do
        {
            newPos = GetPointInCircle(position, minRadius, maxRadius);
        }
        while (!IsEmptyPosition(newPos) && threshold++ < 100);

        if (threshold >= 100)
        {
            Debug.LogWarning("Threshold reached while trying to find an empty point in circle.");
            return position;
        }

        return newPos;
    }

    #endregion

    #region Position Checking

    /// <summary>
    /// Checks if a position has any 2D colliders.
    /// </summary>
    /// <param name="pos">The position to check</param>
    /// <returns>True if no colliders are present at the position</returns>
    public static bool IsEmptyPosition(Vector3 pos)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, 0f);
        return colliders.Length == 0;
    }

    /// <summary>
    /// Checks if a world position is visible on the main camera's screen.
    /// </summary>
    /// <param name="pos">The world position to check</param>
    /// <param name="margin">Optional margin to expand screen boundaries</param>
    /// <returns>True if the position is on screen</returns>
    public static bool IsOnScreen(Vector3 pos, float margin = 0) => IsOnScreen(Camera.main, pos, margin);

    /// <summary>
    /// Checks if a world position is visible on the specified camera's screen.
    /// </summary>
    /// <param name="cam">The camera to check against</param>
    /// <param name="pos">The world position to check</param>
    /// <param name="margin">Optional margin to expand screen boundaries</param>
    /// <returns>True if the position is on screen</returns>
    public static bool IsOnScreen(Camera cam, Vector3 pos, float margin = 0)
    {
        Vector3 viewportPos = cam.WorldToViewportPoint(pos);

        // Check if in front of the camera
        bool isInFront = viewportPos.z > 0;

        // Check if inside screen bounds
        bool isOnScreen = viewportPos.x >= -margin && viewportPos.x <= 1 + margin &&
                          viewportPos.y >= -margin && viewportPos.y <= 1 + margin;

        return isInFront && isOnScreen;
    }

    #endregion

    #region Screen Position Utilities

    /// <summary>
    /// Gets a random point within the main camera's view.
    /// </summary>
    /// <returns>A random world point that's visible on screen</returns>
    public static Vector3 GetRandomPointOnScreen() => GetRandomPointOnScreen(Camera.main);

    /// <summary>
    /// Gets a random point within the specified camera's view.
    /// </summary>
    /// <param name="cam">The camera to use</param>
    /// <returns>A random world point that's visible on the specified camera</returns>
    public static Vector3 GetRandomPointOnScreen(Camera cam)
    {
        Vector3 randomPoint = new(Random.Range(0f, 1f), Random.Range(0f, 1f), cam.nearClipPlane + 1f);

        return cam.ViewportToWorldPoint(randomPoint);
    }

    /// <summary>
    /// Gets a random point within the main camera's view that has no colliders.
    /// </summary>
    /// <returns>A random empty world point visible on screen</returns>
    public static Vector3 GetRandomEmptyPointOnScreen() => GetRandomEmptyPointOnScreen(Camera.main);

    /// <summary>
    /// Gets a random point within the specified camera's view that has no colliders.
    /// </summary>
    /// <param name="cam">The camera to use</param>
    /// <returns>A random empty world point visible on the specified camera</returns>
    public static Vector3 GetRandomEmptyPointOnScreen(Camera cam)
    {
        Vector3 pos = Vector3.zero;
        int threshhold = 0;
        do
        {
            pos = GetRandomPointOnScreen(cam);
        }
        while (!IsEmptyPosition(pos) && ++threshhold < 100);

        if (threshhold >= 100)
        {
            Debug.LogWarning("Threshold reached while trying to find an empty point on screen.");
            return pos;
        }

        return pos;
    }

    /// <summary>
    /// Gets a random point outside the main camera's view.
    /// </summary>
    /// <param name="radius">Distance to consider for generating points outside screen</param>
    /// <param name="margin">Margin from screen edge to ensure point is off-screen</param>
    /// <returns>A world point not visible on screen</returns>
    public static Vector3 GetRandomPointOffScreen(float radius = 1f, float margin = 0) => GetRandomPointOffScreen(Camera.main, radius, margin);

    /// <summary>
    /// Gets a random point outside the specified camera's view.
    /// </summary>
    /// <param name="cam">The camera to use</param>
    /// <param name="radius">Distance to consider for generating points outside screen</param>
    /// <param name="margin">Margin from screen edge to ensure point is off-screen</param>
    /// <returns>A world point not visible on the specified camera</returns>
    public static Vector3 GetRandomPointOffScreen(Camera cam, float radius = 1f, float margin = 0)
    {
        // Some manual tweaks to try and prevent infinite loops. Multiply by 0.1f to get it closer to size of a Unity unit.
        radius = Mathf.Max(0.05f, Mathf.Abs(radius * 0.1f)); // Ensure radius is positive and above 0.05f.
        margin = Mathf.Min(radius * 0.5f, Mathf.Abs(margin * 0.1f)); // Ensure margin is posiitve and no more than half of the radius.

        Vector3 randomPos;
        int threshold = 0;
        do
        {
            Vector3 temp = new(Random.Range(-radius, 1 + radius), Random.Range(-radius, 1 + radius), cam.nearClipPlane + 1f);
            randomPos = cam.ViewportToWorldPoint(temp);
            threshold++;
        }
        while (IsOnScreen(randomPos, margin) && threshold < 100);

        if (threshold >= 100)
        {
            Debug.LogWarning("Threshold reached while trying to find a point off screen. Check your values and try again.");
            return Vector3.zero;
        }

        return randomPos;
    }

    /// <summary>
    /// Gets a position at the edge of the screen in the specified direction.
    /// </summary>
    /// <param name="pos">Starting position</param>
    /// <param name="direction">Direction to travel to find the edge</param>
    /// <returns>Position at the screen's edge</returns>
    public static Vector3 GetScreenEdgePosition(Vector3 pos, Vector3 direction)
    {
        Vector3 checkedPos = pos;
        Vector3 travelDir = direction.normalized * 0.01f;

        do
        {
            checkedPos += travelDir;
        }
        while (IsOnScreen(checkedPos));

        return checkedPos;
    }

    #endregion

    #region UI Utilities

    /// <summary>
    /// Simulates pressing a UI element
    /// </summary>
    /// <param name="targetUI">The UI element to simulate a press on</param>
    public static void SimulatePress(GameObject targetUI)
    {
        if (targetUI == null) return;

        ExecuteEvents.Execute<IPointerClickHandler>(
            targetUI,
            new PointerEventData(EventSystem.current),
            ExecuteEvents.pointerClickHandler);
    }

    #endregion

    #region Miscellaneous Utilities

    /// <summary>
    /// Creates a new path with beveled corners from the provided points.
    /// </summary>
    /// <param name="segments">Number of segments to create for each bevel (minimum 2)</param>
    /// <param name="radius">Radius factor for the bevel (0-1 range, where 1 is maximum possible radius)</param>
    /// <param name="useSmoothing">Whether to use bezier curve smoothing instead of linear interpolation</param>
    /// <param name="path">Points that make up the path</param>
    /// <returns>A new path with beveled corners</returns>
    public static Vector3[] BevelCorners(float segments = 4, float radius = 0.5f, bool useSmoothing = true, params Vector3[] path)
    {
        return BevelCorners(path, segments, radius, useSmoothing);
    }

    /// <summary>
    /// Creates a new path with beveled corners from the input path.
    /// </summary>
    /// <param name="path">Original path as an array of points</param>
    /// <param name="segments">Number of segments to create for each bevel (minimum 2)</param>
    /// <param name="radius">Radius factor for the bevel (0-1 range, where 1 is maximum possible radius)</param>
    /// <param name="useSmoothing">Whether to use bezier curve smoothing instead of linear interpolation</param>
    /// <returns>A new path with beveled corners</returns>
    public static Vector3[] BevelCorners(Vector3[] path, float segments = 4, float radius = 0.5f, bool useSmoothing = true)
    {
        if (path == null || path.Length < 3)
            return path;

        int segmentsInt = Mathf.Max(2, Mathf.RoundToInt(segments));
        radius = Mathf.Clamp01(radius);
        
        List<Vector3> newPath = new List<Vector3>();
        
        // Add first point
        newPath.Add(path[0]);
        
        for (int i = 1; i < path.Length - 1; i++)
        {
            Vector3 prev = path[i - 1];
            Vector3 current = path[i];
            Vector3 next = path[i + 1];
            
            // Calculate direction vectors
            Vector3 dirToCurrent = (current - prev).normalized;
            Vector3 dirToNext = (next - current).normalized;
            
            // Calculate dot product to determine angle
            float dot = Vector3.Dot(dirToCurrent, dirToNext);
            
            // If the angle is significant (not almost a straight line)
            if (dot < 0.99f)
            {
                // Calculate distances (used for determining control points)
                float distToPrev = Vector3.Distance(current, prev);
                float distToNext = Vector3.Distance(current, next);
                
                // Calculate the bevel radius (limited by the shorter segment length)
                float maxRadius = Mathf.Min(distToPrev, distToNext) * 0.5f;
                float bevelRadius = maxRadius * radius;
                
                // Calculate start and end points of the bevel
                Vector3 bevelStart = current - dirToCurrent * bevelRadius;
                Vector3 bevelEnd = current + dirToNext * bevelRadius;
                
                // Add bevel points
                for (int j = 0; j < segmentsInt; j++)
                {
                    float t = j / (float)(segmentsInt - 1);
                    
                    Vector3 point;
                    if (useSmoothing)
                    {
                        // Bezier curve for smoother transition
                        point = QuadraticBezier(bevelStart, current, bevelEnd, t);
                    }
                    else
                    {
                        // Linear interpolation between bevel start/end
                        point = Vector3.Lerp(bevelStart, bevelEnd, t);
                    }
                    
                    if (j > 0 || i == 1) // Skip first point except for first corner
                        newPath.Add(point);
                }
            }
            else
            {
                // If it's almost a straight line, just add the corner point
                newPath.Add(current);
            }
        }
        
        // Add last point
        newPath.Add(path[path.Length - 1]);
        
        return newPath.ToArray();
    }
    
    /// <summary>
    /// Calculate a point on a quadratic Bezier curve.
    /// </summary>
    public static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * p0 + 2f * oneMinusT * t * p1 + t * t * p2;
    }

    #endregion
}
