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

    void Awake()
    {
        // Slightly faster than GetComponent in Start
        interactable = proxyHandle.GetComponent<XRGrabInteractable>();
        realRb = realHandle.GetComponent<Rigidbody>();

        realHandleTransform = realHandle.transform;
        proxyHandleTransform = proxyHandle.transform;
    }

    void FixedUpdate()
    {
        if (!interactable.isSelected)
        {
            // Snap proxy back when released (only if offset exists)
            if (!proxyHandleTransform.position.Equals(realHandleTransform.position))
                proxyHandleTransform.position = realHandleTransform.position;

            if (!proxyHandleTransform.rotation.Equals(realHandleTransform.rotation))
                proxyHandleTransform.rotation = realHandleTransform.rotation;

            return;
        }

        // Compute delta
        Vector3 posDelta = proxyHandleTransform.position - realHandleTransform.position;

        // Optional distance limit check
        if (distanceLimit && posDelta.sqrMagnitude > maxDistance * maxDistance)
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
