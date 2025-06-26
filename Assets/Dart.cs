using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Dart : MonoBehaviour
{
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;

    private bool stuck = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (stuck)
        {
            // Release dart if it is stuck
            if (grabInteractable.isSelected)
            {
                var oldestInteractor = grabInteractable.GetOldestInteractorSelecting();
                grabInteractable.interactionManager.SelectExit((IXRSelectInteractor)oldestInteractor, (IXRSelectInteractable)grabInteractable);
            }
        }

        // Freeze dart motion
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Joint to target if it has a Rigidbody
        if (collision.GetComponent<Rigidbody>() != null)
        {
            StickToMovingTarget(collision);
        }
        else 
        {
            // Stops dart's physics if it hits a non-rigidbody object
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        
        stuck = true;
    }

    void StickToMovingTarget(Collider collision)
    {
        // Joints rigidbodies together
        Rigidbody targetRb = collision.GetComponent<Rigidbody>();
        FixedJoint joint = gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = targetRb;
        // Prevents weird forces in some cases
        joint.enablePreprocessing = false;
        // Prevents joint breaking
        joint.breakForce = 100f;
        joint.breakTorque = Mathf.Infinity;
    }

    public void DartReset()
    {
        stuck = false;

        // Break the joint if it exists
        if (gameObject.TryGetComponent<FixedJoint>(out FixedJoint joint))
        {
            Destroy(joint);
        }

        // Reset dart's physics
        rb.constraints = RigidbodyConstraints.None; // Remove all constraints
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
