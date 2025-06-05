using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[SelectionBase] // Makes it so in scene view, clicking it selects this object, not like the base or top or whatever you physically click
public class PhysicalButton : MonoBehaviour
{
    [Header("Button Settings")]
    [Range(0f, 1f), SerializeField] private float buttonActivationThreshold = 0.95f; // How far the button needs to be pushed in to activate, as a percentage
    [SerializeField] private float buttonSpringForce = 7.5f;

    public string labelText;
    private TMP_Text buttonLabel;

    [ReadOnly] public bool isPressed;
    private bool previousPressState;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip pressedSound;
    [SerializeField] private AudioClip releasedSound;
    private AudioSource _audioSource;

    [Header("Events")]
    public UnityEvent OnButtonDown; // Runs the first frame the button is pressed
    public UnityEvent OnButtonUp; // Runs the first frame the button is released
    public UnityEvent OnButtonHold; // Runs every frame the button is pressed

    private GameObject button;
    private GameObject buttonBase;

    private Rigidbody buttonRb;

    private Vector3 buttonUpPosition;
    private Vector3 buttonDownPosition;

    float totalTravelDistance;

    private void Awake()
    {
        button = transform.Find("Button").gameObject;
        buttonRb = button.GetComponent<Rigidbody>();

        buttonBase = transform.Find("Base").gameObject;

        buttonLabel = transform.Find("Label").GetComponent<TMP_Text>();
        buttonLabel.text = labelText;

        Physics.IgnoreCollision(buttonBase.GetComponent<Collider>(), button.GetComponent<Collider>()); // Ignore collision between button base and top

        // Setting up and down positions (these are local positions, relative to the base!)
        buttonUpPosition = new Vector3(0, (buttonBase.transform.localScale.y * 0.5f) + (button.transform.localScale.y * 0.5f), 0);
        buttonDownPosition = new Vector3(0, (buttonBase.transform.localScale.y * 0.5f) - (button.transform.localScale.y * 0.25f), 0);

        totalTravelDistance = Vector3.Distance(buttonUpPosition, buttonDownPosition);

        previousPressState = isPressed;

        if(!TryGetComponent(out AudioSource _audioSource) && (pressedSound != null || releasedSound != null))
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        button.transform.position = buttonBase.transform.position + buttonUpPosition;
        buttonLabel.rectTransform.sizeDelta = new Vector2(button.transform.localScale.x, button.transform.localScale.z);
    }

    public void SetButtonText(string text)
    {
        labelText = text;
        buttonLabel.text = labelText;
    }

    // Update is called once per frame
    void Update()
    {
        ApplyButtonForces();
        isPressed = CheckIfPressed();
        RunButtonEvents();
    }

    private void ApplyButtonForces()
    {
        // Clamp button position between up and down positions
        button.transform.position = button.transform.position.Clamp(buttonBase.transform.position + buttonDownPosition, buttonBase.transform.position + buttonUpPosition);

        // Apply spring force if needed
        if (button.transform.localPosition.y < buttonUpPosition.y)
        {
            buttonRb.AddForce(button.transform.up * buttonSpringForce * Time.deltaTime);
        }

        // Set label position
        buttonLabel.rectTransform.position = new Vector3(button.transform.position.x, button.transform.position.y + button.transform.localScale.y * 0.51f, button.transform.position.z);
    }

    private bool CheckIfPressed()
    {
        float percentagePressed = (button.transform.position - (buttonBase.transform.position + buttonUpPosition)).magnitude / totalTravelDistance;
        return percentagePressed >= buttonActivationThreshold;
    }

    private void RunButtonEvents()
    {
        if(isPressed)
        {
            if (previousPressState == false)
            {
                OnButtonDown?.Invoke();
                if(pressedSound != null) _audioSource.PlayOneShot(pressedSound);
                Debug.Log("Pressed");
            }
            else
            {
                OnButtonHold?.Invoke();
                Debug.Log("Held");
            }
        }
        else if(previousPressState == true)
        {
            OnButtonUp?.Invoke();
            if(releasedSound != null) _audioSource.PlayOneShot(releasedSound);
            Debug.Log("Released");
        }

        previousPressState = isPressed;
    }
}