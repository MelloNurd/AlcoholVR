using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ColliderAdd : MonoBehaviour
{
    [HelpBox("Add colliders in here to automatically add them to the Interactable colliders list on startup. This will allow it to populate itself with child colliders first.")]
    [SerializeField] private List<Collider> collidersToAdd;

    private async void Start()
    {
        await UniTask.Delay(2000); // Wait to allow the interactable to initialize its own colliders

        XRSimpleInteractable interactable = GetComponent<XRSimpleInteractable>();

        if(interactable != null)
        {
            foreach (Collider col in collidersToAdd)
            {
                if (!interactable.colliders.Contains(col))
                {
                    interactable.colliders.Add(col);
                }
            }
        }
    }
}
