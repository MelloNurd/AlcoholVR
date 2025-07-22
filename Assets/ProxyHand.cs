using UnityEngine;

public class ProxyHand : MonoBehaviour
{
    [SerializeField] PhysicsHand physicsHand; // Reference to the PhysicsHand component

    private void OnTriggerEnter(Collider other)
    {
        physicsHand.colliding = true;
        Debug.Log("ProxyHand: Trigger Entered with " + other.name);
    }

    private void OnTriggerExit(Collider other)
    {
        physicsHand.colliding = false;
        Debug.Log("ProxyHand: Trigger Exited with " + other.name);
    }
}
