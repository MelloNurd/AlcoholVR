using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class ColliderEventHandler : MonoBehaviour
{
    public Collider[] colliders;

    [field: SerializeField] public bool IsEnabled { get; set; } = true;
    [field: SerializeField] public float CollisionCooldown { get; set; } = 0.1f; // Minimum time between collision events

    public UnityEvent<Collision> OnCollisionEnterEvent = new();
    public UnityEvent<Collision> OnCollisionStayEvent = new();
    public UnityEvent<Collision> OnCollisionExitEvent = new();

    public UnityEvent<Collision2D> OnCollisionEnter2DEvent = new();
    public UnityEvent<Collision2D> OnCollisionStay2DEvent = new();
    public UnityEvent<Collision2D> OnCollisionExit2DEvent = new();

    private void Awake()
    {
        // Automatically find all non-trigger colliders on this GameObject
        colliders = GetComponents<Collider>().Where(x => !x.isTrigger).ToArray();
    }

    public async void InvokeCooldown()
    {
        IsEnabled = false;
        await UniTask.Delay(Mathf.RoundToInt(CollisionCooldown * 1000));
        IsEnabled = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsEnabled) return;

        OnCollisionEnterEvent?.Invoke(collision);
        InvokeCooldown();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!IsEnabled) return;

        OnCollisionEnter2DEvent?.Invoke(collision);
        InvokeCooldown();
    }

    private void OnCollisionStay(Collision collision)
    {
        if(!IsEnabled) return;

        OnCollisionStayEvent?.Invoke(collision);
        InvokeCooldown();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(!IsEnabled) return;

        OnCollisionStay2DEvent?.Invoke(collision);
        InvokeCooldown();
    }

    private void OnCollisionExit(Collision collision)
    {
        if(!IsEnabled) return;

        OnCollisionExitEvent?.Invoke(collision);
        InvokeCooldown();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(!IsEnabled) return;

        OnCollisionExit2DEvent?.Invoke(collision);
        InvokeCooldown();
    }

    public void ClearAllEvents()
    {
        OnCollisionEnterEvent.RemoveAllListeners();
        OnCollisionStayEvent.RemoveAllListeners();
        OnCollisionExitEvent.RemoveAllListeners();
        OnCollisionEnter2DEvent.RemoveAllListeners();
        OnCollisionStay2DEvent.RemoveAllListeners();
        OnCollisionExit2DEvent.RemoveAllListeners();
    }
}
