using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Dart : MonoBehaviour
{
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;

    private bool stuck = false;
    float StickDelay = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void Update()
    {
        if(StickDelay > 0)
        {
            StickDelay -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        //Debug.Log("Dart collided with: " + collision.gameObject.name);
        if (stuck)
        {
            // Release dart if it is stuck
            if (grabInteractable.isSelected)
            {
                var oldestInteractor = grabInteractable.GetOldestInteractorSelecting();
                grabInteractable.interactionManager.SelectExit((IXRSelectInteractor)oldestInteractor, (IXRSelectInteractable)grabInteractable);
            }
        }

        // Don't stick to other darts
        if(collision.gameObject.layer == LayerMask.NameToLayer("Dart") || collision.gameObject.layer == LayerMask.NameToLayer("PlayerFace") || StickDelay > 0)
        {
            return;
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
        StickDelay = 0.5f; // Delay before the dart can stick again
    }

    void OnDrawGizmos()
    {
        if (rb != null)
        {
            Gizmos.color = Color.red;
            Vector3 worldCenterOfMass = transform.TransformPoint(rb.centerOfMass);
            Gizmos.DrawSphere(worldCenterOfMass, 0.01f);
        }
    }
}
