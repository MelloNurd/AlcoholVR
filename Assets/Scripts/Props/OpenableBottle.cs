using EditorAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class OpenableBottle : MonoBehaviour
{
    [SerializeField] private AudioClip _openSound;

    [field: SerializeField, ReadOnly] public bool IsOpen { get; private set; } = false;
    [field: SerializeField] public bool IsFull { get; set; } = true;

    private BottleCap _cap;

    private XRGrabInteractable _grabInteractable;
    private AudioSource _audioSource;

    private void Awake()
    {
        _cap = GetComponentInChildren<BottleCap>();

        _grabInteractable = GetComponent<XRGrabInteractable>();
        _audioSource = gameObject.GetOrAdd<AudioSource>();

        // Disable collisions between all child colliders to prevent physics issues
        var childColliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < childColliders.Length; i++)
        {
            for (int j = i + 1; j < childColliders.Length; j++)
            {
                Physics.IgnoreCollision(childColliders[i], childColliders[j]);
            }
        }
    }

    private void Start()
    {
        _cap.grabInteractable.enabled = false;

        _cap.fixedJoint.breakForce = Mathf.Infinity;

        _grabInteractable.selectEntered.AddListener((_) => GrabBottle());
        _grabInteractable.selectExited.AddListener((_) => ReleaseBottle());

        _cap.grabInteractable.selectEntered.AddListener((_) => { SetCapBreakForce(5f); });
        _cap.grabInteractable.selectExited.AddListener((_) => { SetCapBreakForce(Mathf.Infinity); });

        _cap.onOpen.AddListener((_) => OpenBottle());
    }

    private void SetCapBreakForce(float force)
    {
        if (_cap.fixedJoint == null) return;

        _cap.fixedJoint.breakForce = force;
    }

    private void OpenBottle()
    {
        IsOpen = true;
        _audioSource.PlayOneShot(_openSound);
        _cap.grabInteractable.enabled = true;
    }

    private void GrabBottle()
    {
        _cap.grabInteractable.enabled = true;
    }

    private void ReleaseBottle()
    {
        if (IsOpen) return;

        _cap.grabInteractable.enabled = false;
    }
}
