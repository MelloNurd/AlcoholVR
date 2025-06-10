using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using PrimeTween;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Phone : MonoBehaviour
{
    public static Phone Instance { get; private set; }

    [Header("General")]
    [SerializeField, Required] private Material _uiMaterial;
    [SerializeField, Required] private Material _cameraMaterial;
    [SerializeField] private bool _useRealTimeClock = true;
    private DateTime _phoneTime;
    private RectTransform _notificationPanel;

    [Header("Camera")]
    public bool IsUsingCamera => _phonePhysicalCamera.enabled;

    [Header("Objectives")]
    // stuff here

    [Header("Messages")]
    [SerializeField] private GameObject _messagePrefab;
    private Transform _messagesContainer;
    private List<PhoneMessage> _messages = new();

    [Header("Settings")]
    [SerializeField] private PhoneTheme _currentTheme;
    [SerializeField] private PhoneTheme[] _availableThemes;

    private GameObject _phoneObject;
    private MeshRenderer _phoneScreenMeshRenderer;
    private Camera _phonePhysicalCamera; // The camera that renders the phone's camera (the camera app)
    private Camera _phoneUICamera; // The camera that renders the phone's UI
    private Canvas _phoneUICanvas; // The canvas that contains the phone's UI elements
    private TMP_Text _phoneClockTime;
    private TMP_Text _phoneClockDate;


    private void Awake()
    {
        // Singleton implementation
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Assign component variables
        _phoneObject = transform.Find("Physical Phone").gameObject;
        _phoneScreenMeshRenderer = _phoneObject.transform.Find("Screen").GetComponent<MeshRenderer>();
        _phonePhysicalCamera = _phoneObject.GetComponentInChildren<Camera>();
        _phoneUICamera = transform.Find("Phone Screen Camera").GetComponent<Camera>();
        _phoneUICanvas = transform.Find("Phone Canvas").GetComponent<Canvas>();
        _phoneClockTime = _phoneUICanvas.transform.Find("HomeScreen").Find("Clock").Find("Time").GetComponent<TMP_Text>();
        _phoneClockDate = _phoneUICanvas.transform.Find("HomeScreen").Find("Clock").Find("Date").GetComponent<TMP_Text>();
        _messagesContainer = _phoneUICanvas.transform.Find("MessagesScreen").Find("Messages");
        _notificationPanel = _phoneUICanvas.transform.Find("Notification").GetComponent<RectTransform>();

        // Initialize values
        _phoneScreenMeshRenderer.material = _uiMaterial;
        _phonePhysicalCamera.enabled = false;

        if(_currentTheme == null) _currentTheme = _availableThemes.GetRandom();
        ApplyTheme();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            ToggleCamera();
        }
        else if(Input.GetKeyDown(KeyCode.N))
        {
            ShowNotification("Test Notification", "This is a test notification message.");
        }

        if (_useRealTimeClock)
        {
            _phoneTime = DateTime.Now;
        }
        UpdateClock();
    }

    private void ToggleCamera()
    {
        _phonePhysicalCamera.enabled = !_phonePhysicalCamera.enabled;
        _phoneScreenMeshRenderer.material = IsUsingCamera ? _cameraMaterial : _uiMaterial;
    }

    private void UpdateClock()
    {
        _phoneClockTime.text = _phoneTime.ToString("h:mm tt"); // Format: 1:02 PM
        _phoneClockDate.text = _phoneTime.ToString("MMM d, yyyy"); // Format: Jan 1, 2023
    }

    [Button("Apply Theme", EButtonEnableMode.Playmode)]
    private void ApplyTheme() => ApplyTheme(_currentTheme);
    private void ApplyRandomTheme() => ApplyTheme(_availableThemes.GetRandom());
    private void ApplyTheme(PhoneTheme theme)
    {
        if(theme == null)
        {
            Debug.LogWarning("No theme provided. Using default theme.");
            return;
        }

        _currentTheme = theme;
        
        _phoneUICanvas.transform.Find("BG").GetComponent<Image>().sprite = theme.BackgroundImage;
        _phoneUICanvas.transform.Find("HomeScreen").Find("ClockBG").GetComponent<Image>().color = theme.ClockBackgroundColor;

        _phoneClockTime.color = theme.PrimaryColor;
        _phoneClockDate.color = theme.PrimaryColor;

        Transform appsDrawer = _phoneUICanvas.transform.Find("HomeScreen").transform.Find("Apps");
        foreach(Image image in appsDrawer.GetComponentsInChildren<Image>())
        {
            image.color = theme.PrimaryColor;
            if (image.name == "BG") image.color = theme.TertiaryColor;
        }
        foreach (Shadow shadow in appsDrawer.GetComponentsInChildren<Shadow>())
        {
            shadow.effectColor = theme.SecondaryColor;
        }
    }

    public void ShowNotification(PhoneMessage msg) => ShowNotification(msg.Sender, "Message from: " + msg.Content);
    public async void ShowNotification(string title, string message)
    {
        Tween.CompleteAll(_notificationPanel); // Resets any existing tweens
        // FIX THE ABOVE ^^^

        _notificationPanel.Find("Title").GetComponent<TMP_Text>().text = title;
        _notificationPanel.Find("Text").GetComponent<TMP_Text>().text = message;

        await Tween.UIAnchoredPositionY(_notificationPanel, -85, 0.4f);
        await UniTask.Delay(5_000);
        _ = Tween.UIAnchoredPositionY(_notificationPanel, 475, 0.4f);
    }

    public void AddMessage(PhoneMessage message)
    {
        if(message == null)
        {
            Debug.LogWarning("Attempted to add a null message.");
            return;
        }

        _messages.RemoveAll(x => x.Sender == message.Sender); // We only keep the latest message from each sender
        _messages.Add(message);
    }

    private void LoadMessages()
    {
        foreach(var msg in _messages)
        {
            GameObject messageObject = Instantiate(_messagePrefab, _messagesContainer);
            messageObject.transform.Find("Name").GetComponent<TMP_Text>().text = msg.Sender;
            messageObject.transform.Find("Text").GetComponent<TMP_Text>().text = msg.Content;
            messageObject.transform.Find("Time").GetComponent<TMP_Text>().text = msg.Timestamp.ToString("h:mm tt"); // Format: 1:02 PM
        }
    }
}
