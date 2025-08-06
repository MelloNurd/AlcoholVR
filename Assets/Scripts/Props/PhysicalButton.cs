using Cysharp.Threading.Tasks;
using EditorAttributes;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using ReadOnly = EditorAttributes.ReadOnlyAttribute;

[SelectionBase] // Makes it so in scene view, clicking it selects this object, not like the base or top or whatever you physically click
public class PhysicalButton : MonoBehaviour
{
    [Header("Button Settings")]
    public LayerMask interactableLayers;

    [Tooltip("How far the button needs to be pushed in to activate, as a percentage"), Range(0f, 1f), SerializeField]
    private float _buttonActivationThreshold = 0.98f;
    [Tooltip("How far the button needs to be pressed (after already pressing) to deactivate, as a percentage."), Range(0f, 1f), SerializeField]
    private float _buttonReleaseThreshold = 0.1f;

    [SerializeField] private string labelText;
    private TMP_Text _buttonLabel;

    [field: SerializeField, Tooltip("Whether or not the button can be pressed.")] 
    public bool IsInteractable { get; set; } = true;

    [ReadOnly] public bool IsPressed;
    private bool _previousPressState;

    // Buttons in the inspector
    [ButtonField(nameof(ButtonPress), buttonLabel: "Execute Press"), SerializeField, HideInEditMode] private EditorAttributes.Void buttonStruct0;
    [ButtonField(nameof(ButtonRelease), buttonLabel: "Execute Release"), SerializeField, HideInEditMode] private EditorAttributes.Void buttonStruct1;
    [ButtonField(nameof(ButtonHold), true, buttonLabel: "Execute Hold"), SerializeField, HideInEditMode] private EditorAttributes.Void buttonStruct2;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip _pressedSound;
    [SerializeField] private AudioClip _releasedSound;
    private AudioSource _audioSource;

    [Header("Events")]
    public UnityEvent OnButtonDown; // Runs the first frame the button is pressed
    public UnityEvent OnButtonUp; // Runs the first frame the button is released
    public UnityEvent OnButtonHold; // Runs every frame the button is pressed

    private GameObject _button;
    private GameObject _buttonDarkness;
    private GameObject _buttonBase;

    [SerializeField, ReadOnly] private float _buttonUpDistance;
    private Vector3 _upPosition => (_buttonUpDistance * _buttonBase.transform.up) + _buttonBase.transform.position;

    private Collider[] _collisionResults = new Collider[8]; // Pre-allocated array for overlap detection

    public void EnableButton()
    {
        _buttonDarkness.SetActive(false);
        _buttonLabel.color = Color.white;
        IsInteractable = true;
        Debug.Log("Enabling button: " + gameObject.name);
    }

    public void DisableButton()
    {
        _buttonDarkness.SetActive(true);
        _buttonLabel.color = Color.gray;
        IsInteractable = false;
        Debug.Log("Disabling button: " + gameObject.name);
    }

    private void Awake()
    {
        _button = transform.Find("Button").gameObject;
        _buttonDarkness = _button.transform.Find("ButtonDarkness").gameObject;

        _buttonBase = transform.Find("Base").gameObject;

        _buttonLabel = transform.Find("Label").GetComponent<TMP_Text>();
        _buttonLabel.text = labelText;

        var childColliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < childColliders.Length; i++)
        {
            for (int j = i + 1; j < childColliders.Length; j++)
            {
                Physics.IgnoreCollision(childColliders[i], childColliders[j]);
            }
        }

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
    }

    private async void Start()
    {
        if (IsInteractable)
        {
            EnableButton();
            IsInteractable = false;
            await UniTask.Delay(500); // Disable button briefly at the start to make sure it doesn't immediatley get pressed
            IsInteractable = true;
        }
        else DisableButton();
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

        // Adjust the button's up distance based on the scale of the button and its parents
        float scaleMultiplier = 1;
        foreach (Transform parent in transform.GetComponentsInParent<Transform>(true))
        {
            scaleMultiplier *= parent.localScale.x;
        }

        float upDistanceToScale = _buttonUpDistance * 9f * scaleMultiplier;

        // Check for valid collisions with the button
        float hitDistance = _buttonUpDistance;
        if (IsInteractable && IsButtonObstructed(Vector3.one.WithY(0.06f) * 0.5f * scaleMultiplier, upDistanceToScale, out hitDistance)) { } // hitDistance is set by the out parameter, so the if can be empty

        _button.transform.position = _buttonBase.transform.position + (_button.transform.up * hitDistance*0.45f);

        // Set label position
        _buttonLabel.rectTransform.position = _button.transform.position + (_button.transform.localScale.y * 0.51f * scaleMultiplier * _button.transform.up);

        if (!IsPressed)
        {
            // If not pressed, check if it should become pressed (using activation threshold)
            IsPressed = IsInteractable && hitDistance < (1 - _buttonActivationThreshold) * upDistanceToScale;
        }
        else
        {
            // If already pressed, check if it should be released (using release threshold)
            IsPressed = IsInteractable && hitDistance < (1 - _buttonReleaseThreshold) * upDistanceToScale;
        }
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
            interactableLayers,
            QueryTriggerInteraction.Ignore);

        // Check if anything is overlapping with the button (needed because if the button is inside an object, the box cast won't hit it)
        bool overlapHit = Physics.OverlapBoxNonAlloc(
            _buttonBase.transform.position + (_button.transform.up * hitPoint),
            scale,
            _collisionResults,
            transform.rotation,
            interactableLayers,
            QueryTriggerInteraction.Ignore) > 0;

        // Only consider "PlayerBody" layer objects if they're tagged as "Hand"
        bool isValidHit = boxCastHit && hitInfo.collider.gameObject.layer != LayerMask.NameToLayer("PlayerBody");

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

    public void ButtonPress()
    {
        if (!IsInteractable) return;

        if (_pressedSound != null)
        {
            PlaySound(_pressedSound);
        }
        //Debug.Log("Pressed");
        OnButtonDown?.Invoke();
    }
    public void ButtonHold()
    {
        if (!IsInteractable) return;

        //Debug.Log("Held");
        OnButtonHold?.Invoke();
    }

    public void ButtonRelease()
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