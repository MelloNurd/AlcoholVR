using UnityEngine;
using UnityEngine.Events;

public class TriggerEvents : MonoBehaviour
{
    public UnityEvent onTriggerEntered;

    public void OnTriggerEnter(Collider other)
    {
        onTriggerEntered?.Invoke();
    }
}
