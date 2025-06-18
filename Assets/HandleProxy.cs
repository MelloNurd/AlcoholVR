using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HandleProxy : MonoBehaviour
{
    [SerializeField] GameObject realHandle;
    [SerializeField] GameObject proxyHandle;

    [Header("Spring Settings")]
    public float springStrength = 1000f;
    public float springDamping = 100f;

    [Header("Distance Limit")]
    public bool distanceLimit = false;
    public float maxDistance = 1.5f;

    XRGrabInteractable interactable;
    Rigidbody realRb;

    Transform realHandleTransform;
    Transform proxyHandleTransform;

    public float linearThreshold = 0.1f;
    public float angularThreshold = 0.1f;

    float maxDistanceSqr;

    void Awake()
    {
        // Slightly faster than GetComponent in Start  
        interactable = proxyHandle.GetComponent<XRGrabInteractable>();
        realRb = realHandle.GetComponent<Rigidbody>();

        realHandleTransform = realHandle.transform;
        proxyHandleTransform = proxyHandle.transform;
        maxDistanceSqr = maxDistance * maxDistance;
    }

    void FixedUpdate()
    {
        // Reset the position to the real handle's position if not selected and outside thresholds and stop forces  
        if (!interactable.isSelected &&
            (Vector3.Distance(realHandleTransform.position, proxyHandleTransform.position) > linearThreshold ||
             Quaternion.Angle(realHandleTransform.rotation, proxyHandleTransform.rotation) > angularThreshold))
        {
            proxyHandleTransform.position = realHandleTransform.position;
            proxyHandleTransform.rotation = realHandleTransform.rotation;
            //realRb.linearVelocity = Vector3.zero; // Stop forces when not selected  
            return;
        }

        // Compute delta  
        Vector3 posDelta = proxyHandleTransform.position - realHandleTransform.position;

        // Optional distance limit check  
        if (distanceLimit && posDelta.sqrMagnitude > maxDistanceSqr)
        {
            var interactor = interactable.GetOldestInteractorSelecting();
            if (interactor != null)
            {
                interactable.interactionManager.SelectExit(interactor, interactable);
                return; // Prevent force being applied after releasing  
            }
        }

        // Critically damped spring force  
        Vector3 force = (posDelta * springStrength) + (-realRb.linearVelocity * springDamping);
        realRb.AddForce(force, ForceMode.Force);
    }
}
