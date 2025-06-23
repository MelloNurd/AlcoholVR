using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using ReadOnly = NaughtyAttributes.ReadOnlyAttribute;

[SelectionBase] // Makes it so in scene view, clicking it selects this object, not like the base or top or whatever you physically click
public class PhysicalButton : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private LayerMask _interactableColliders;
    [Range(0f, 1f), SerializeField] private float _buttonActivationThreshold = 0.98f; // How far the button needs to be pushed in to activate, as a percentage

    [SerializeField] private string labelText;
    private TMP_Text _buttonLabel;

    [field: SerializeField, Tooltip("Whether or not the button can be pressed.")] public bool IsInteractable { get; set; } = true;

    [ReadOnly] public bool IsPressed;
    private bool _previousPressState;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip _pressedSound;
    [SerializeField] private AudioClip _releasedSound;
    private AudioSource _audioSource;

    [Header("Events")]
    public UnityEvent OnButtonDown; // Runs the first frame the button is pressed
    public UnityEvent OnButtonUp; // Runs the first frame the button is released
    public UnityEvent OnButtonHold; // Runs every frame the button is pressed

    private GameObject _button;
    private GameObject _buttonBase;

    [SerializeField, ReadOnly] private float _buttonUpDistance;
    private Vector3 _upPosition => (_buttonUpDistance * _buttonBase.transform.up) + _buttonBase.transform.position;

    private Collider[] _collisionResults = new Collider[8]; // Pre-allocated array for overlap detection

    private async void Awake()
    {
        _button = transform.Find("Button").gameObject;

        _buttonBase = transform.Find("Base").gameObject;

        _buttonLabel = transform.Find("Label").GetComponent<TMP_Text>();
        _buttonLabel.text = labelText;

        // Setting up distance (how far up from base)
        _buttonUpDistance = 0.03f;

        _previousPressState = IsPressed;

        if(!TryGetComponent(out _audioSource) && (_pressedSound != null || _releasedSound != null))
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        _button.transform.position = _upPosition;
        _buttonLabel.transform.position = _upPosition;
        _buttonLabel.rectTransform.sizeDelta = new Vector2(_button.transform.localScale.x, _button.transform.localScale.z);
        
        // Disable button briefly at the start to make sure it doesn't immediatley get pressed
        if(IsInteractable)
        {
            IsInteractable = false;
            await UniTask.Delay(500); // Wait a bit to ensure everything is set up before enabling the button
            IsInteractable = true;
        }
    }

    void FixedUpdate()
    {
        // Since this uses a box cast and an overlap box, run it in FixedUpdate to optimize performance
        ApplyButtonPhysics();
    }

    void Update()
    {
        RunButtonEvents();
    }

    private void ApplyButtonPhysics()
    {
        _button.transform.rotation = _buttonBase.transform.rotation; // Keep button aligned with base

        // Set button position
        Vector3 scale = transform.localScale.WithY(0.06f) * 0.5f;

        // Check for valid collisions with the button
        float hitDistance = _buttonUpDistance;
        if (IsInteractable && IsButtonObstructed(scale, _buttonUpDistance, out hitDistance)) { } // hitDistance is set by the out parameter, so the if can be empty

        _button.transform.position = _buttonBase.transform.position + (_button.transform.up * hitDistance);

        // Set label position
        _buttonLabel.rectTransform.position = _button.transform.position + (_button.transform.localScale.y * 0.51f * transform.localScale.y * _button.transform.up);

        // Set pressed state
        IsPressed = IsInteractable && hitDistance < (1 - _buttonActivationThreshold) * _buttonUpDistance;
    }

    private bool IsButtonObstructed(Vector3 scale, float distance, out float hitPoint)
    {
        hitPoint = distance;

        // Use a box cast to check for collisions AND determine how far the button is pressed in
        bool boxCastHit = Physics.BoxCast(
            _buttonBase.transform.position,
            scale,
            _buttonBase.transform.up,
            out RaycastHit hitInfo,
            transform.rotation,
            distance,
            _interactableColliders);

        // Check if anything is overlapping with the button (needed because if the button is inside an object, the box cast won't hit it)
        bool overlapHit = Physics.OverlapBoxNonAlloc(
            _buttonBase.transform.position + (_button.transform.up * hitPoint),
            scale,
            _collisionResults,
            transform.rotation,
            _interactableColliders) > 0;

        // Only consider "PlayerBody" layer objects if they're tagged as "Hand"
        bool isValidHit = boxCastHit &&
            (hitInfo.collider.gameObject.layer != LayerMask.NameToLayer("PlayerBody") ||
             hitInfo.collider.CompareTag("Hand"));

        if ((boxCastHit && isValidHit) || overlapHit)
        {
            hitPoint = hitInfo.distance;
            return true;
        }

        return false;
    }

    private void RunButtonEvents()
    {
        if(IsPressed)
        {
            if (_previousPressState == false)
            {
                ButtonPress();
            }
            else
            {
                ButtonHold();
            }
        }
        else if(_previousPressState == true)
        {
            ButtonRelease();
        }

        _previousPressState = IsPressed;
    }

    [Button("Execute Button Press")]
    private void ButtonPress()
    {
        if (!IsInteractable) return;

        if (_pressedSound != null)
        {
            PlaySound(_pressedSound);
        }
        //Debug.Log("Pressed");
        OnButtonDown?.Invoke();
    }
    [Button("Execute Button Hold")]
    private void ButtonHold()
    {
        if (!IsInteractable) return;

        //Debug.Log("Held");
        OnButtonHold?.Invoke();
    }

    [Button("Execute Button Release")]
    private void ButtonRelease()
    {
        if (!IsInteractable) return;

        if (_releasedSound != null)
        {
            PlaySound(_releasedSound);
        }
        //Debug.Log("Released");
        OnButtonUp?.Invoke();
    }

    public void SetButtonText(string text)
    {
        labelText = text;
        _buttonLabel.text = labelText;
    }

    public void PlaySound(AudioClip sound, bool randomizePitch = true)
    {
        if (randomizePitch)
        {
            _audioSource.pitch = Random.Range(0.95f, 1.05f);
        }
        _audioSource.PlayOneShot(sound);
    }
}