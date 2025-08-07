using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HeadCollisionDetector : MonoBehaviour
{
    public float _detectionDelay = 0.05f;
    public float _detectionRadius = 0.15f;
    public LayerMask _collisionLayerMask;
    public List<RaycastHit> _collisionHits = new List<RaycastHit>();
    private float _currentTime = 0f;

    List<Vector3> directions = new()
        {
            Vector3.up,
            Vector3.down,
            Vector3.left,
            Vector3.right,
            Vector3.forward,
            Vector3.back
        };

    private void Start()
    {
        _collisionHits = PerformDetection(transform.position, _detectionRadius, _collisionLayerMask);
    }

    private void Update()
    {
        _currentTime += Time.deltaTime;
        if (_currentTime >= _detectionDelay)
        {
            _currentTime = 0f;
            _collisionHits = PerformDetection(transform.position, _detectionRadius, _collisionLayerMask);
        }
    }

    private List<RaycastHit> PerformDetection(Vector3 position, float distance, LayerMask mask)
    {
        List<RaycastHit> detectedHits = new();

        RaycastHit hit;
        foreach (var direction in directions)
        {
            if (Physics.Raycast(position, direction, out hit, distance, mask))
            {
                if (!HasGrabInteractableComponent(hit.collider.gameObject))
                {
                    detectedHits.Add(hit);
                }
            }
        }

        return detectedHits;
    }

    private bool HasGrabInteractableComponent(GameObject hitObject)
    {
        // Check if the hit object itself has an XRGrabInteractable component
        if (hitObject.GetComponent<XRGrabInteractable>() != null)
        {
            return true;
        }

        // Check if any of the children have an XRGrabInteractable component
        XRGrabInteractable grabInteractable = hitObject.GetComponentInChildren<XRGrabInteractable>();
        return grabInteractable != null;
    }

    private void OnDrawGizmos()
    {
        Color c = Color.green;
        c.a = 0.5f;
        if(_collisionHits.Count > 0)
        {
            c = Color.red;
            c.a = 0.5f;
        }

        Gizmos.color = c;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);

        Gizmos.color = Color.magenta;
        foreach(var dir in directions)
        {
            Gizmos.DrawRay(transform.position, dir * _detectionRadius);
        }
    }
}
