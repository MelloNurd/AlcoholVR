using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BottleCap : MonoBehaviour
{
    XRGrabInteractable grabInteractable;
    FixedJoint fixedJoint;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        fixedJoint = GetComponent<FixedJoint>();
    }

    private void Update()
    {
        if(grabInteractable.isSelected)
        {
            fixedJoint.breakTorque = 5f;
        }
    }
}
