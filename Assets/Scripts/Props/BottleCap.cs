using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BottleCap : MonoBehaviour
{
    XRGrabInteractable grabInteractable;
    FixedJoint fixedJoint;
    AudioSource audioSource;
    bool broken = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        fixedJoint = GetComponent<FixedJoint>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if(grabInteractable.isSelected && !broken)
        {
            fixedJoint.breakTorque = 5f;
        }
    }

    private void OnJointBreak(float breakForce)
    {
        audioSource.Play();
        broken = true;
    }
}
