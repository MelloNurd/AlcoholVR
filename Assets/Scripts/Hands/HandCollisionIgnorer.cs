using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class HandCollisionIgnorer : MonoBehaviour
{
    public XRBaseInteractor leftHandInteractor;
    public XRBaseInteractor rightHandInteractor;

    public GameObject leftHandObject;
    public GameObject rightHandObject;

    private void OnEnable()
    {
        leftHandInteractor.selectEntered.AddListener(OnGrab);
        leftHandInteractor.selectExited.AddListener(OnRelease);

        rightHandInteractor.selectEntered.AddListener(OnGrab);
        rightHandInteractor.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        leftHandInteractor.selectEntered.RemoveListener(OnGrab);
        leftHandInteractor.selectExited.RemoveListener(OnRelease);

        rightHandInteractor.selectEntered.RemoveListener(OnGrab);
        rightHandInteractor.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        ToggleCollision(args.interactableObject.transform.gameObject, true);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        ToggleCollision(args.interactableObject.transform.gameObject,false);
    }

    private void ToggleCollision(GameObject grabbedObject, bool ignore)
    {
        Collider[] objectColliders = grabbedObject.GetComponentsInChildren<Collider>();
        Collider[] leftHandColliders = leftHandObject.GetComponentsInChildren<Collider>();
        Collider[] rightHandColliders = rightHandObject.GetComponentsInChildren<Collider>();

        foreach (var objCol in objectColliders)
        {
            // Toggle collision with left hand colliders
            foreach (var handCol in leftHandColliders)
            {
                if (objCol != null && handCol != null)
                    Physics.IgnoreCollision(objCol, handCol, ignore);
            }

            // Toggle collision with right hand colliders
            foreach (var handCol in rightHandColliders)
            {
                if (objCol != null && handCol != null)
                    Physics.IgnoreCollision(objCol, handCol, ignore);
            }
        }
    }
}
