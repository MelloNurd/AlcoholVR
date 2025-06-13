using UnityEngine;
using UnityEngine.EventSystems;

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
}
