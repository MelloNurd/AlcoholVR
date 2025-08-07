using UnityEngine;

public class VisualHand : MonoBehaviour
{
    public Transform controllerTarget;   // The XR controller
    [SerializeField] public HandPhysics physicalHand;  // The physics-based hand
    public float blendSpeed = 10f;
    public float postCollisionBlendTime = 0.15f; // Short duration to keep blending after exit

    private float blendTimer = 0f;

    private void Update()
    {
        if (physicalHand.isColliding)
        {
            // Reset the blend timer while colliding
            blendTimer = postCollisionBlendTime;
        }
        else if (blendTimer > 0f)
        {
            blendTimer -= Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        bool shouldBlend = blendTimer > 0f;

        if (shouldBlend)
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
