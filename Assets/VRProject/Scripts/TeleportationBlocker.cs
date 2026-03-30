using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TeleportationBlocker : MonoBehaviour
{
    private XRRayInteractor _rayInteractor;
    private int _blockLayerMask;

    void Awake()
    {
        _rayInteractor = GetComponent<XRRayInteractor>();
        _blockLayerMask = LayerMask.GetMask("Boundary");
    }

    void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 0.1f, _blockLayerMask);
        _rayInteractor.enabled = hits.Length == 0;
    }
}
