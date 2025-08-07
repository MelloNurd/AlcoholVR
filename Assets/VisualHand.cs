using UnityEngine;

public class VisualHand : MonoBehaviour
{
    public Transform controllerTarget;   // The XR controller
    [SerializeField] public HandPhysics physicalHand;  // The physics-based hand
    public float maxDistance = 0.05f;
    public float blendSpeed = 10f;

    private bool inWall = false;

    private void LateUpdate()
    {
        if (physicalHand.isColliding)
        {
            inWall = true;
        }

        Vector3 toPhysical = physicalHand.transform.position - controllerTarget.position;
        float distance = toPhysical.magnitude;

        if (distance < maxDistance)
        {
            inWall = false;
        }

        if (distance > maxDistance && inWall)
        {
            // Blend visual hand toward physical hand
            transform.position = Vector3.Lerp(transform.position, physicalHand.transform.position, Time.deltaTime * blendSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, physicalHand.transform.rotation, Time.deltaTime * blendSpeed);
        }
        else
        {
            // Snap visual hand to controller target WITH the same offset
            transform.position = controllerTarget.TransformPoint(physicalHand.positionOffset);
            transform.rotation = controllerTarget.rotation * Quaternion.Euler(physicalHand.rotationOffsetEuler);
        }
    }
}
