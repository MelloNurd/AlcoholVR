using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UIElements;

[SelectionBase] // Makes it so in scene view, clicking it selects this object, not like the base or top or whatever you physically click
public class PhysicalButton : MonoBehaviour
{
    [Header("Button Settings")]
    [Range(0f, 1f), SerializeField] private float _buttonActivationThreshold = 0.98f; // How far the button needs to be pushed in to activate, as a percentage
    [SerializeField] private float _buttonSpringForce = 8f;

    [SerializeField] private string labelText;
    private TMP_Text _buttonLabel;

    public bool IsActive = true;

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

    private Rigidbody _buttonRb;

    [SerializeField, ReadOnly] private Vector3 _buttonUpPosition;
    [SerializeField, ReadOnly] private Vector3 _buttonDownPosition;
    private Vector3 _upPosition => (_buttonUpPosition.magnitude * _buttonBase.transform.up) + _buttonBase.transform.position;
    private Vector3 _downPosition => (_buttonDownPosition.magnitude * _buttonBase.transform.up) + _buttonBase.transform.position;

    private float _totalTravelDistance;

    private async void Awake()
    {
        _button = transform.Find("Button").gameObject;
        _buttonRb = _button.GetComponent<Rigidbody>();

        _buttonBase = transform.Find("Base").gameObject;

        _buttonLabel = transform.Find("Label").GetComponent<TMP_Text>();
        _buttonLabel.text = labelText;

        Physics.IgnoreCollision(_buttonBase.GetComponent<Collider>(), _button.GetComponent<Collider>()); // Ignore collision between button base and top

        // Setting up and down positions (these are local positions, relative to the base!)
        _buttonUpPosition = new Vector3(0, 0.03f, 0);
        _buttonDownPosition = Vector3.zero;

        _totalTravelDistance = Vector3.Distance(_buttonUpPosition, _buttonDownPosition);

        _previousPressState = IsPressed;

        if(!TryGetComponent(out _audioSource) && (_pressedSound != null || _releasedSound != null))
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        _button.transform.position = _upPosition;
        _buttonLabel.transform.position = _upPosition;
        _buttonLabel.rectTransform.sizeDelta = new Vector2(_button.transform.localScale.x, _button.transform.localScale.z);
        
        // Disable button briefly at the start to make sure it doesn't immediatley get pressed
        if(IsActive)
        {
            Collider col = _button.GetComponent<Collider>();
            col.enabled = false;
            IsActive = false;
            await UniTask.Delay(500); // Wait a bit to ensure everything is set up before enabling the button
            IsActive = true;
            col.enabled = true;
        }
    }

    public void SetButtonText(string text)
    {
        labelText = text;
        _buttonLabel.text = labelText;
    }

    // Update is called once per frame
    void Update()
    {
        ApplyButtonForces();
        IsPressed = CheckIfPressed();
        RunButtonEvents();
    }

    private void ApplyButtonForces()
    {
        _button.transform.rotation = _buttonBase.transform.rotation; // Keep button aligned with base

        var upper = transform.InverseTransformPoint(_upPosition);
        var lower = transform.InverseTransformPoint(_downPosition);
        var clampedPos = Mathf.Clamp(Mathf.Abs(_button.transform.localPosition.y), lower.y, upper.y);

        // Clamp button position between up and down positions
        _button.transform.localPosition = new Vector3(0, clampedPos, 0);

        // Apply spring force
        _buttonRb.AddForce(_button.transform.up * _buttonSpringForce * Time.deltaTime);

        // Set label position
        _buttonLabel.rectTransform.position = _button.transform.position + (_button.transform.localScale.y * 0.51f * transform.localScale.y * _button.transform.up);

        // Disable if not active
        if (!IsActive)
        {
            _buttonRb.isKinematic = true; // Disable physics if not active
            _button.transform.position = _upPosition; // Reset position
        }
        else
        {
            _buttonRb.isKinematic = false; // Enable physics if active
        }
    }

    private bool CheckIfPressed()
    {
        if(!IsActive) return false;

        float percentagePressed = (transform.InverseTransformPoint(_upPosition).y - _button.transform.localPosition.y) / _totalTravelDistance * transform.localScale.y;
        //Debug.Log(percentagePressed);
        return percentagePressed >= _buttonActivationThreshold;
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
        OnButtonDown?.Invoke();
        Debug.Log("audio Source: " + _audioSource);
        if (_pressedSound != null)
        {
            _audioSource.pitch = Random.Range(0.95f, 1.05f);
            _audioSource.PlayOneShot(_pressedSound);
        }
        //Debug.Log("Pressed");
    }
    [Button("Execute Button Hold")]
    private void ButtonHold()
    {
        OnButtonHold?.Invoke();
        //Debug.Log("Held");
    }

    [Button("Execute Button Release")]
    private void ButtonRelease()
    {
        OnButtonUp?.Invoke();
        if (_releasedSound != null)
        {
            _audioSource.pitch = Random.Range(0.95f, 1.05f);
            _audioSource.PlayOneShot(_releasedSound);

        }
        //Debug.Log("Released");
    }
}