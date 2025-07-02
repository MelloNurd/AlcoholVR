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

    [SerializeField] private bool _isLidRemovable = false;

    private BottleCap _cap;

    private XRGrabInteractable _grabInteractable;
    private AudioSource _audioSource;

    private void Awake()
    {
        _cap = GetComponentInChildren<BottleCap>();

        _grabInteractable = GetComponent<XRGrabInteractable>();
        _audioSource = gameObject.GetOrAdd<AudioSource>();
    }

    private void Start()
    {
        if (_cap.grabInteractable == null)
        {
            Debug.LogError("BottleCap does not have a XRGrabInteractable component attached.", _cap);
            return;
        }

        _cap.grabInteractable.enabled = false;

        _grabInteractable.selectEntered.AddListener((_) => GrabBottle());
        _grabInteractable.selectExited.AddListener((_) => ReleaseBottle());

        if(_isLidRemovable)
        {
            SetCapBreakForce(Mathf.Infinity);
            _cap.grabInteractable.selectEntered.AddListener((_) => { SetCapBreakForce(5f); });
            _cap.grabInteractable.selectExited.AddListener((_) => { SetCapBreakForce(Mathf.Infinity); });
        }

        _cap.onOpen.AddListener((_) => OpenBottle());
    }

    private void SetCapBreakForce(float force)
    {
        if (_cap.fixedJoint == null) return;

        _cap.fixedJoint.breakForce = force;
    }

    public void TryOpenBottle()
    {
        if (_cap == null || _cap.grabInteractable == null) return;
        if (!_cap.grabInteractable.isSelected || IsOpen) return;

        OpenBottle();
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
