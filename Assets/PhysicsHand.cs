using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsHand : MonoBehaviour
{
    [SerializeField] private GameObject followObject;
    public float followSpeed = 30f;
    public float rotationSpeed = 100f;

    [Header("Offsets")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;

    [Header("Reset Settings")]
    public float resetSpeed = 50f; // Speed for returning to target when not colliding
    public float resetThreshold = 0.01f; // Distance threshold to consider "reset complete"

    private Transform followTarget;
    private Rigidbody body;

    public bool colliding = false;
    public bool exitReset = true;

    void Start()
    {
        followTarget = followObject.transform;
        body = GetComponent<Rigidbody>();

        // Set initial position with offset
        Vector3 targetPosition = followTarget.position + followTarget.TransformDirection(positionOffset);
        Quaternion targetRotation = followTarget.rotation * Quaternion.Euler(rotationOffset);

        body.position = targetPosition;
        body.rotation = targetRotation;
    }

    void FixedUpdate()
    {
        if (colliding)
        {
            PhysicsMove();
        }
        else if (exitReset)
        {
            bool done = ResetToTarget();
            if (done)
            {
                exitReset = false;
            }
        }
    }

    void PhysicsMove()
    {
        // Calculate target position with offset
        Vector3 targetPosition = followTarget.position + followTarget.TransformDirection(positionOffset);
        Vector3 toTarget = targetPosition - body.position;
        float distance = toTarget.magnitude;
        Vector3 direction = toTarget.normalized;

        body.linearVelocity = direction * (followSpeed * distance);

        // Calculate target rotation with offset
        Quaternion targetRotation = followTarget.rotation * Quaternion.Euler(rotationOffset);
        Quaternion rotationDelta = targetRotation * Quaternion.Inverse(body.rotation);
        rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;
        if (angle != 0)
        {
            Vector3 angularVelocity = axis * angle * Mathf.Deg2Rad * rotationSpeed;
            body.angularVelocity = angularVelocity;
        }
    }

    bool ResetToTarget()
    {
        Vector3 targetPosition = followTarget.position + followTarget.TransformDirection(positionOffset);
        Vector3 toTarget = targetPosition - body.position;
        float distance = toTarget.magnitude;

        // Only apply reset forces if we're beyond the threshold
        if (distance > resetThreshold)
        {
            Vector3 direction = toTarget.normalized;
            body.linearVelocity = direction * (resetSpeed * distance);

            // Calculate target rotation with offset
            Quaternion targetRotation = followTarget.rotation * Quaternion.Euler(rotationOffset);
            Quaternion rotationDelta = targetRotation * Quaternion.Inverse(body.rotation);
            rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f) angle -= 360f;
            if (angle != 0)
            {
                Vector3 angularVelocity = axis * angle * Mathf.Deg2Rad * rotationSpeed;
                body.angularVelocity = angularVelocity;
            }

            return false; // Not done resetting
        }
        else
        {
            // Close enough - stop all movement
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            return true; // Done resetting
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        colliding = true;
        exitReset = false; // Reset exitReset when entering a collision
    }

    private void OnCollisionExit(Collision collision)
    {
        colliding = false;
        exitReset = true; // Set exitReset to true when exiting the collision
    }
}
