using UnityEngine;

public class HandPhysics : MonoBehaviour
{
    public Transform target;
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffsetEuler = Vector3.zero;

    public float positionStrength = 100f;   // Higher = snappier
    public float rotationStrength = 100f;

    public float maxDistance = 1f; // Maximum distance to the target before snapping

    private Rigidbody rb;
    private Quaternion rotationOffset;

    // Store the latest target data here to prevent input lag
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
        // Get the latest pose in Update, when XR input updates
        latestTargetPosition = target.TransformPoint(positionOffset);
        latestTargetRotation = target.rotation * rotationOffset;
    }

    void FixedUpdate()
    {
        Vector3 toTarget = latestTargetPosition - rb.position;

        // Default movement
        Vector3 desiredVelocity = toTarget * positionStrength;

        // If colliding, project movement onto surface plane
        if (isColliding && lastCollisionNormal != Vector3.zero)
        {
            // Slide movement along the wall
            desiredVelocity = Vector3.ProjectOnPlane(desiredVelocity, lastCollisionNormal);
        }

        rb.linearVelocity = desiredVelocity;

        // Rotation logic (unchanged)
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
        lastCollisionNormal = collision.contacts[0].normal;
    }

    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
        lastCollisionNormal = Vector3.zero;
    }

}
