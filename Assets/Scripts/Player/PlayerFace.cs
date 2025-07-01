using UnityEngine;

public class PlayerFace : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"PlayerFace collided with: {other.gameObject.name}");
    }
}
