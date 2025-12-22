using System.Linq;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI;

public class TriggerEventHandler : MonoBehaviour
{
    [field: SerializeField] public bool EventsEnabled { get; set; } = true;
    [field: SerializeField] public float EventsCooldown { get; set; } = 0.1f; // Minimum time between trigger events
    private bool isOnCooldown = false;

    [Tooltip("List of colliders to run the events on. If empty, will automatically use this object's trigger colliders.")]
    public Collider[] colliders;

    [FoldoutGroup("3D Events", nameof(OnTriggerEnterEvent), nameof(OnTriggerStayEvent), nameof(OnTriggerExitEvent))]
    [SerializeField] private Void _3D;

    [FoldoutGroup("2D Events", nameof(OnTriggerEnter2DEvent), nameof(OnTriggerStay2DEvent), nameof(OnTriggerExit2DEvent))]
    [SerializeField] private Void _2D;

    [HideProperty] public UnityEvent<Collider> OnTriggerEnterEvent = new();
    [HideProperty] public UnityEvent<Collider> OnTriggerStayEvent = new();
    [HideProperty] public UnityEvent<Collider> OnTriggerExitEvent = new();

    [HideProperty] public UnityEvent<Collider2D> OnTriggerEnter2DEvent = new();
    [HideProperty] public UnityEvent<Collider2D> OnTriggerStay2DEvent = new();
    [HideProperty] public UnityEvent<Collider2D> OnTriggerExit2DEvent = new();

    private void Awake()
    {
        if(colliders == null || colliders.Length == 0)
        {
            colliders = GetComponents<Collider>().Where(x => x.isTrigger).ToArray();
        }
    }

    public async void InvokeCooldown()
    {
        isOnCooldown = true;
        await UniTask.Delay(Mathf.RoundToInt(EventsCooldown * 1000));
        isOnCooldown = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!EventsEnabled || isOnCooldown) return;

        OnTriggerEnterEvent?.Invoke(other);
        InvokeCooldown();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!EventsEnabled || isOnCooldown) return;

        OnTriggerEnter2DEvent?.Invoke(collision);
        InvokeCooldown();
    }

    private void OnTriggerStay(Collider other)
    {
        if(!EventsEnabled || isOnCooldown) return;

        OnTriggerStayEvent?.Invoke(other);
        InvokeCooldown();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(!EventsEnabled || isOnCooldown) return;

        OnTriggerStay2DEvent?.Invoke(collision);
        InvokeCooldown();
    }

    private void OnTriggerExit(Collider other)
    {
        if(!EventsEnabled || isOnCooldown) return;

        OnTriggerExitEvent?.Invoke(other);
        InvokeCooldown();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(!EventsEnabled || isOnCooldown) return;

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
