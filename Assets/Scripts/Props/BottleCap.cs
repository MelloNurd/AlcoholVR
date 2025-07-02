using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BottleCap : MonoBehaviour
{
    public UnityEvent<float> onOpen;

    [HideInInspector] public FixedJoint fixedJoint;
    [HideInInspector] public XRBaseInteractable grabInteractable;

    private void Awake()
    {
        fixedJoint = GetComponent<FixedJoint>();
        grabInteractable = GetComponent<XRBaseInteractable>();
    }

    private void OnJointBreak(float breakForce)
    {
        onOpen?.Invoke(breakForce);
    }
}
