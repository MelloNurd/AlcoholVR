using UnityEngine;

public class PlayerFace : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out OpenableBottle bottle) && bottle.IsOpen)
        {
            Debug.Log("Player drank booze omg!!!1!");
        }
    }
}
