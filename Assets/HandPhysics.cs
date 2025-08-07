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
        // Position
        Vector3 toTarget = latestTargetPosition - rb.position;
        if(toTarget.magnitude > maxDistance)
        {
            // If too far, snap to target position
            rb.position = latestTargetPosition;
            rb.linearVelocity = Vector3.zero; // Stop any existing velocity

            if (transform.name == "RightHand")
            {
                Player.Instance.ForceRelease("right");
                Debug.Log("Right hand snapped to target position, releasing grip.");
            }
            else if (transform.name == "LeftHand")
            {
                Player.Instance.ForceRelease("left");
                Debug.Log("Left hand snapped to target position, releasing grip.");
            }
            else
            {
                Debug.LogWarning("HandPhysics: Hand is not named correctly for snapping.");
            }

                return;
        }
        rb.linearVelocity = toTarget * positionStrength;

        // Rotation
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
    }

    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
    }
}
