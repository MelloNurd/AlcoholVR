using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BottleCap : MonoBehaviour
{
    private OpenableBottle _bottle;

    [HideInInspector] public FixedJoint fixedJoint;
    [HideInInspector] public XRBaseInteractable interactable;

    private void Awake()
    {
        fixedJoint = GetComponent<FixedJoint>();
        interactable = GetComponent<XRBaseInteractable>();

        interactable.selectEntered.AddListener((_) => _bottle.onLidGrab?.Invoke());

        _bottle = GetComponentInParent<OpenableBottle>();
    }

    private void OnJointBreak(float breakForce)
    {
        _bottle.onLidOpen?.Invoke();
    }
}
