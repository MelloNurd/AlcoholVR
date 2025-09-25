using UnityEngine;

public class HandPhysics : MonoBehaviour
{
    public Transform target;
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffsetEuler = Vector3.zero;

    public float positionStrength = 100f;   // Higher = snappier
    public float rotationStrength = 100f;
    public float maxDistance = 1f;          // Max allowed distance before snapping

    private Rigidbody rb;
    private Quaternion rotationOffset;

    private Vector3 latestTargetPosition;
    private Quaternion latestTargetRotation;

    public bool isColliding = false;
    private Vector3 lastCollisionNormal = Vector3.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rotationOffset = Quaternion.Euler(rotationOffsetEuler);

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        latestTargetPosition = target.TransformPoint(positionOffset);
        latestTargetRotation = target.rotation * rotationOffset;
    }

    void FixedUpdate()
    {
        Vector3 toTarget = latestTargetPosition - rb.position;
        Vector3 desiredVelocity = toTarget * positionStrength;

        if (isColliding && lastCollisionNormal != Vector3.zero)
        {
            Vector3 handToController = latestTargetPosition - rb.position;

            // Only project movement if controller is pushing into the surface
            if (Vector3.Dot(handToController, lastCollisionNormal) <= 0f)
            {
                desiredVelocity = Vector3.ProjectOnPlane(desiredVelocity, lastCollisionNormal);
            }
        }

        // Optional: snap back if hand gets too far away
        if (toTarget.magnitude > maxDistance)
        {
            rb.position = latestTargetPosition;
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            rb.linearVelocity = desiredVelocity;
        }

        // Apply rotation
        Quaternion deltaRotation = latestTargetRotation * Quaternion.Inverse(rb.rotation);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f) angle -= 360f;

        if (angle != 0 && axis != Vector3.zero)
        {
            Vector3 angularVel = axis.normalized * angle * Mathf.Deg2Rad * rotationStrength;
            rb.angularVelocity = angularVel;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        isColliding = true;
        lastCollisionNormal = collision.contacts[0].normal;
    }

    private void OnCollisionStay(Collision collision)
    {
        isColliding = true;
        lastCollisionNormal = collision.contacts[0].normal;
    }

    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
        lastCollisionNormal = Vector3.zero;
    }

    public void ResetCollision()
    {
        isColliding = false;
    }
}
