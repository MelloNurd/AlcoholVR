using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEventHandler : MonoBehaviour
{
    public Collider[] colliders;

    [field: SerializeField] public bool IsEnabled { get; set; } = true;
    [field: SerializeField] public float TriggerCooldown { get; set; } = 0.1f; // Minimum time between trigger events

    public UnityEvent<Collider> OnTriggerEnterEvent = new();
    public UnityEvent<Collider> OnTriggerStayEvent = new();
    public UnityEvent<Collider> OnTriggerExitEvent = new();

    public UnityEvent<Collider2D> OnTriggerEnter2DEvent = new();
    public UnityEvent<Collider2D> OnTriggerStay2DEvent = new();
    public UnityEvent<Collider2D> OnTriggerExit2DEvent = new();

    private void Awake()
    {
        // Automatically find all trigger colliders on this GameObject
        colliders = GetComponents<Collider>().Where(x => x.isTrigger).ToArray();
    }

    public async void InvokeCooldown()
    {
        IsEnabled = false;
        await UniTask.Delay(Mathf.RoundToInt(TriggerCooldown * 1000));
        IsEnabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsEnabled) return;

        OnTriggerEnterEvent?.Invoke(other);
        InvokeCooldown();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!IsEnabled) return;

        OnTriggerEnter2DEvent?.Invoke(collision);
        InvokeCooldown();
    }

    private void OnTriggerStay(Collider other)
    {
        if(!IsEnabled) return;

        OnTriggerStayEvent?.Invoke(other);
        InvokeCooldown();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(!IsEnabled) return;

        OnTriggerStay2DEvent?.Invoke(collision);
        InvokeCooldown();
    }

    private void OnTriggerExit(Collider other)
    {
        if(!IsEnabled) return;

        OnTriggerExitEvent?.Invoke(other);
        InvokeCooldown();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(!IsEnabled) return;

        OnTriggerExit2DEvent?.Invoke(collision);
        InvokeCooldown();
    }

    public void ClearAllEvents()
    {
        OnTriggerEnterEvent.RemoveAllListeners();
        OnTriggerStayEvent.RemoveAllListeners();
        OnTriggerExitEvent.RemoveAllListeners();
        OnTriggerEnter2DEvent.RemoveAllListeners();
        OnTriggerStay2DEvent.RemoveAllListeners();
        OnTriggerExit2DEvent.RemoveAllListeners();
    }
}
