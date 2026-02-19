using UnityEngine;

public class LookAt : MonoBehaviour
{
    public Transform objectToLookAt;

    public float lookAtRange = 3.0f;
    [Range(0, 1)] public float weight = 1.0f;
    [Range(0, 1)] public float bodyWeight = 0.05f;
    [Range(0, 1)] public float headWeight = 1.0f;
    [Range(0, 1)] public float eyesWeight = 1.0f;
    [Range(0, 1)] public float clampWeight = 0.5f;
    // Speed of head movement towards the target position
    public float smoothSpeed = 5f;
    // Speed of head snapping when within range
    public float weightSmoothSpeed = 2f; 

    // Maximum yaw (degrees) the NPC is allowed to turn head left/right
    public float maxYawDegrees = 90f;
    // Vertical offset used as the head origin when computing clamped direction
    public float headHeight = 1.5f;

    private Animator animator;
    private Vector3 lookPosition;
    private float currentLookWeight = 0f;

    public bool isLooking = false;
    public bool lookWithinRange = false;
    private bool isInRange = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (objectToLookAt != null)
            lookPosition = objectToLookAt.position;
    }

    void Update()
    {
        if (lookWithinRange)
        {
            isInRange = Vector3.Distance(transform.position, objectToLookAt.position) <= lookAtRange;
        }
        else
        {
            isInRange = true;
        }

        bool shouldLook = isLooking && isInRange;

        float targetWeight = shouldLook ? weight : 0f;
        currentLookWeight = Mathf.Lerp(currentLookWeight, targetWeight, Time.deltaTime * weightSmoothSpeed);

        if (shouldLook)
        {
            lookPosition = Vector3.Lerp(lookPosition, objectToLookAt.position, Time.deltaTime * smoothSpeed);
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator && objectToLookAt)
        {
            animator.SetLookAtWeight(currentLookWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
            Vector3 clampedPos = GetClampedLookPosition(lookPosition);
            animator.SetLookAtPosition(clampedPos);
        }
    }

    public void LookAtPlayer()
    {
        objectToLookAt = Player.Instance.Camera.transform;
        isLooking = true;
    }

    public void SetLookAtRange(float range) => lookAtRange = range;

    void OnDrawGizmos()
    {
        if (objectToLookAt != null && isLooking)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up * headHeight, objectToLookAt.position);
        }
    }

    // Clamp the look position so the yaw relative to the NPC forward doesn't exceed maxYawDegrees.
    private Vector3 GetClampedLookPosition(Vector3 desiredPosition)
    {
        Vector3 origin = transform.position + Vector3.up * headHeight;
        Vector3 toTarget = desiredPosition - origin;
        if (toTarget.sqrMagnitude < 1e-6f)
            return desiredPosition;

        // Convert direction to NPC local space (relative to NPC rotation)
        Vector3 localDir = Quaternion.Inverse(transform.rotation) * toTarget;

        // Yaw angle in degrees (left/right)
        float yaw = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
        float clampedYaw = Mathf.Clamp(yaw, -maxYawDegrees, maxYawDegrees);

        // If within limits, return original desiredPosition
        if (Mathf.Approximately(yaw, clampedYaw))
            return desiredPosition;

        // Preserve the original XZ distance and vertical offset (pitch)
        float xzDistance = new Vector2(localDir.x, localDir.z).magnitude;
        Vector3 limitedLocalDir = Quaternion.Euler(0f, clampedYaw, 0f) * Vector3.forward * xzDistance;
        limitedLocalDir.y = localDir.y; // preserve vertical component

        // Convert back to world space and return a world position
        Vector3 limitedWorldDir = transform.rotation * limitedLocalDir;
        return origin + limitedWorldDir;
    }
}
