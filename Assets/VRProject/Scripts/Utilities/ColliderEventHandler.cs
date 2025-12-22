using System.Linq;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Events;

public class ColliderEventHandler : MonoBehaviour
{
    [field: SerializeField] public bool EventsEnabled { get; set; } = true;
    [field: SerializeField] public float EventsCooldown { get; set; } = 0.1f; // Minimum time between collision events
    private bool isOnCooldown = false;

    [Tooltip("List of colliders to run the events on. If empty, will automatically use this object's non-trigger colliders.")]
    public Collider[] colliders;

    [FoldoutGroup("3D Events", nameof(OnCollisionEnterEvent), nameof(OnCollisionStayEvent), nameof(OnCollisionExitEvent))]
    [SerializeField] private Void _3D;

    [FoldoutGroup("2D Events", nameof(OnCollisionEnter2DEvent), nameof(OnCollisionStay2DEvent), nameof(OnCollisionExit2DEvent))]
    [SerializeField] private Void _2D;

    [HideProperty] public UnityEvent<Collision> OnCollisionEnterEvent = new();
    [HideProperty] public UnityEvent<Collision> OnCollisionStayEvent = new();
    [HideProperty] public UnityEvent<Collision> OnCollisionExitEvent = new();

    [HideProperty] public UnityEvent<Collision2D> OnCollisionEnter2DEvent = new();
    [HideProperty] public UnityEvent<Collision2D> OnCollisionStay2DEvent = new();
    [HideProperty] public UnityEvent<Collision2D> OnCollisionExit2DEvent = new();

    private void Awake()
    {
        // Automatically find all non-trigger colliders on this GameObject
        colliders = GetComponents<Collider>().Where(x => !x.isTrigger).ToArray();
    }

    public async void InvokeCooldown()
    {
        isOnCooldown = true;
        await UniTask.Delay(Mathf.RoundToInt(EventsCooldown * 1000));
        isOnCooldown = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!EventsEnabled || isOnCooldown) return;

        OnCollisionEnterEvent?.Invoke(collision);
        InvokeCooldown();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!EventsEnabled || isOnCooldown) return;

        OnCollisionEnter2DEvent?.Invoke(collision);
        InvokeCooldown();
    }

    private void OnCollisionStay(Collision collision)
    {
        if(!EventsEnabled || isOnCooldown) return;

        OnCollisionStayEvent?.Invoke(collision);
        InvokeCooldown();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(!EventsEnabled || isOnCooldown) return;

        OnCollisionStay2DEvent?.Invoke(collision);
        InvokeCooldown();
    }

    private void OnCollisionExit(Collision collision)
    {
        if(!EventsEnabled || isOnCooldown) return;

        OnCollisionExitEvent?.Invoke(collision);
        InvokeCooldown();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(!EventsEnabled || isOnCooldown) return;

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
