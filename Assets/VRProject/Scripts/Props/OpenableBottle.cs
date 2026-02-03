using EditorAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class OpenableBottle : MonoBehaviour
{
    [SerializeField] private AudioClip _openSound;

    [field: SerializeField, ReadOnly] public bool IsOpen { get; private set; } = false;
    [field: SerializeField] public bool IsFull { get; set; } = true;
    [field: SerializeField] public bool IsAlcoholic { get; set; } = true;

    private BottleCap _cap;

    private XRGrabInteractable _grabInteractable;
    private AudioSource _audioSource;

    public UnityEvent onLidGrab = new();
    public UnityEvent onLidOpen = new();

    private void Awake()
    {
        _cap = GetComponentInChildren<BottleCap>();

        _grabInteractable = GetComponent<XRGrabInteractable>();
        _audioSource = gameObject.GetOrAdd<AudioSource>();

        // Disable collisions between the bottle and any child colliders (like the cap)
        Collider meshCol = GetComponent<Collider>();

        if (!_cap) return;

        foreach (Collider col in _cap.GetComponents<Collider>())
        {
            Physics.IgnoreCollision(meshCol, col);
        }
    }

    private void Start()
    {
        if (!_cap) return;
        if (_cap.interactable == null)
        {
            Debug.LogError("BottleCap does not have a XRGrabInteractable component attached.", gameObject);
            return;
        }

        _cap.interactable.enabled = false;
        SetCollisions(false);

        _grabInteractable.selectEntered.AddListener((_) => GrabBottle());
        _grabInteractable.selectExited.AddListener((_) => ReleaseBottle());

        if(_cap.interactable is XRGrabInteractable)
        {
            SetCapBreakForce(Mathf.Infinity);
            _cap.interactable.selectEntered.AddListener((_) => { SetCapBreakForce(5f); });
            _cap.interactable.selectExited.AddListener((_) => { SetCapBreakForce(Mathf.Infinity); });
        }

        onLidOpen.AddListener(OpenBottle);
    }

    private void SetCapBreakForce(float force)
    {
        if (_cap.fixedJoint == null) return;

        _cap.fixedJoint.breakForce = force;
    }

    public void TryOpenBottle()
    {
        if (_cap == null || _cap.interactable == null) return;
        if (!_cap.interactable.isSelected || IsOpen) return;

        OpenBottle();
    }

    private void OpenBottle()
    {
        IsOpen = true;
        _audioSource.PlayOneShot(_openSound);
        _cap.interactable.enabled = (_cap.interactable is XRGrabInteractable); // disable/enable whether the cap is grabbable or not (determines whether it comes off)
        if(_cap.interactable.enabled)
        {
            SetCollisions(true);
        }
    }

    private void GrabBottle()
    {
        _cap.interactable.enabled = true;
    }

    private void ReleaseBottle()
    {
        if (IsOpen) return;

        _cap.interactable.enabled = false;
    }

    private void SetCollisions(bool state) // Enable or disable collisions between the bottle and the cap
    {
        var temp = GetComponents<Collider>();
        Collider capCol = _cap.GetComponent<Collider>();
        foreach (var col in temp)
        {
            Physics.IgnoreCollision(col, capCol, !state);
        }
    }
}
