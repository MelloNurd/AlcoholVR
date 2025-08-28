using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class HeadCollisionHandler : MonoBehaviour
{
    [SerializeField] HeadCollisionDetector detector;
    [SerializeField] CharacterController characterController;
    public float pushbackFroce = 1f;

    private void Update()
    {
        if (detector._collisionHits.Count > 0)
        {
            Vector3 pushbackDirection = CalculatePushbackDirection(detector._collisionHits);

            Debug.DrawRay(transform.position, pushbackDirection.normalized, Color.magenta);

            characterController.Move(pushbackDirection * pushbackFroce * Time.deltaTime);
        }
    }

    private Vector3 CalculatePushbackDirection(List<RaycastHit> hits)
    {
        Vector3 pushbackDirection = Vector3.zero;

        foreach (var hit in hits)
        {
            pushbackDirection += new Vector3(hit.normal.x, 0, hit.normal.z);
        }

        return pushbackDirection;
    }
}
