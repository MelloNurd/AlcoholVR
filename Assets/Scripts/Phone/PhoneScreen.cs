using Cysharp.Threading.Tasks;
using UnityEngine;

public class PhoneScreen : MonoBehaviour
{
    private Phone _phone;

    private bool _canInteract = true;

    private void Awake()
    {
        _phone = FindFirstObjectByType<Phone>();
    }

    private async void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Index")) return; // Only check for index finger
        if(!_canInteract) return;
        if (Vector3.Dot((other.transform.position - transform.position).normalized, -transform.forward) <= 0) return; // Check if specifically the front of the screen was pressed (return otherwise)

        _canInteract = false;
        _phone.SimulateScreenPressAtPoint(other.ClosestPoint(transform.position));
        await UniTask.Delay(200);
        _canInteract = true;
    }
}
