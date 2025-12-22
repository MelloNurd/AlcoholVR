using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class MaxGrabDistance : MonoBehaviour
{
    public float maxDistance = 1.5f;
    private XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    void Update()
    {
        if (grab.isSelected)
        {
            // Get all interactors currently selecting this object  
            List<IXRSelectInteractor> interactors = grab.interactorsSelecting;

            foreach (var interactor in interactors)
            {
                // Check distance for each interacting hand  
                Transform interactorTransform = interactor.transform;
                if (Vector3.Distance(transform.position, interactorTransform.position) > maxDistance)
                {
                    // Release the grab for this specific interactor when it exceeds the max distance  
                    grab.interactionManager.SelectExit(interactor, grab);
                }
            }
        }
    }
}
