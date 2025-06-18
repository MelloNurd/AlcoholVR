using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using PrimeTween;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Phone : MonoBehaviour
{
    public static Phone Instance { get; private set; }

    [Header("General")]
    [SerializeField] private bool _useRealTimeClock = true;
    [SerializeField] private bool _useRealBattery = true;
    private DateTime _phoneTime = DateTime.Now;
    private float _batteryLevel = 0.73f;
    private RectTransform _notificationPanel;

    [Header("Objectives")]
    // stuff here

    [Header("Messages")]
    [SerializeField] private GameObject _messagePrefab;
    private Transform _messagesContainer;
    private List<PhoneMessage> _messages = new();

    [Header("Settings")]
    [SerializeField] private PhoneTheme _currentTheme;
    [SerializeField] private PhoneTheme[] _availableThemes;
    private TMP_Text _batteryLevelText;
    private Image _batteryFillImage;

    private GameObject _phoneObject;
    private Camera _phonePhysicalCamera;
    private Camera _phoneUICamera; // The camera that renders the phone's UI
    private Canvas _phoneUICanvas; // The canvas that contains the phone's UI elements
    private TMP_Text _phoneClockTime;
    private TMP_Text _phoneClockDate;

    private GameObject _screenObject;

    private Image _phoneBG;

    private CanvasGroup _homeScreenGroup;
    private CanvasGroup _messagesScreenGroup;
    private CanvasGroup _objectivesScreenGroup;
    private CanvasGroup _settingsScreenGroup;
    private CanvasGroup _notificationGroup;
    private CanvasGroup _cameraScreenGroup;

    private AppButton _cameraButton;
    private AppButton _messagesButton;
    private AppButton _objectivesButton;
    private AppButton _settingsButton;

    public GameObject debugObj;

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
        _screenObject = _phoneObject.transform.Find("Screen").gameObject;
        _phonePhysicalCamera = _phoneObject.GetComponentInChildren<Camera>();

        _phoneUICamera = transform.Find("Phone Screen Camera").GetComponent<Camera>();
        _phoneUICanvas = transform.Find("Phone Canvas").GetComponent<Canvas>();
        _phoneBG = _phoneUICanvas.transform.Find("BG").GetComponent<Image>();

        _batteryLevelText = _phoneUICanvas.transform.Find("Battery/Text").GetComponent<TMP_Text>();
        _batteryFillImage = _phoneUICanvas.transform.Find("Battery/IconFill").GetComponent<Image>();

        _homeScreenGroup = _phoneUICanvas.transform.Find("HomeScreen").GetComponent<CanvasGroup>();
        var temp = _homeScreenGroup.transform.Find("Clock");
        _phoneClockTime = temp.Find("Time").GetComponent<TMP_Text>();
        _phoneClockDate = temp.Find("Date").GetComponent<TMP_Text>();

        _messagesScreenGroup = _phoneUICanvas.transform.Find("MessagesScreen").GetComponent<CanvasGroup>();
        _messagesContainer = _messagesScreenGroup.transform.Find("Messages");

        _objectivesScreenGroup = _phoneUICanvas.transform.Find("ObjectivesScreen").GetComponent<CanvasGroup>();

        _settingsScreenGroup = _phoneUICanvas.transform.Find("SettingsScreen").GetComponent<CanvasGroup>();

        _notificationGroup = _phoneUICanvas.transform.Find("Notification").GetComponent<CanvasGroup>();
        _notificationPanel = _notificationGroup.GetComponent<RectTransform>();

        _cameraScreenGroup = _phoneUICanvas.transform.Find("CameraScreen").GetComponent<CanvasGroup>();


        temp = _homeScreenGroup.transform.Find("Apps");
        _cameraButton = temp.Find("Camera").GetComponent<AppButton>();
        _cameraButton.OnClick.AddListener(ShowCameraScreen);
        _messagesButton = temp.Find("Messages").GetComponent<AppButton>();
        _messagesButton.OnClick.AddListener(ShowMessagesScreen);
        _objectivesButton = temp.Find("Objectives").GetComponent<AppButton>();
        _objectivesButton.OnClick.AddListener(ShowObjectivesScreen);
        _settingsButton = temp.Find("Settings").GetComponent<AppButton>();
        _settingsButton.OnClick.AddListener(ShowSettingsScreen);

        if(_currentTheme == null) _currentTheme = _availableThemes.GetRandom();
        ApplyTheme();

        _homeScreenGroup.Show();
        _messagesScreenGroup.Hide();
        _objectivesScreenGroup.Hide();
        _settingsScreenGroup.Hide();
        _cameraScreenGroup.Hide();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            ShowNotification("Test Sender", "This is a test message");
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            ShowNotification("Test Sender", "This is a second test message");
        }

        // This is temporary, tesing with mouse input for screen interaction
        if (Input.GetMouseButtonDown(0))
        {
            // Cast a ray from the main camera through the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.name != "Screen") return;

                SimulateScreenPressAtPoint(hit.point);
            }
        }
    }

    private void FixedUpdate()
    {
        UpdateClock();
        UpdateBattery();
    }

    private void SimulateScreenPressAtPoint(Vector3 point)
    {
        Vector3 localHitPoint = _screenObject.transform.InverseTransformPoint(point);

        // Calculate normalized position (assuming screen mesh is centered and properly scaled)
        // Adjust these calculations based on your phone screen's local orientation and dimensions
        float normalizedX = 1 - (localHitPoint.x + 0.5f); // Map from -0.5 to 0.5 to 0-1 range and flip X
        float normalizedY = 1 - (localHitPoint.y + 0.5f); // Map from -0.5 to 0.5 to 0-1 range and flip Y

        // Convert to phone UI camera's texture coordinates
        Vector2 virtualPos = new Vector2(
            normalizedX * _phoneUICamera.targetTexture.width,
            normalizedY * _phoneUICamera.targetTexture.height
        );

        // Create pointer event data for UI raycasting
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = virtualPos;

        // Raycast against UI elements
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        GameObject[] hitObjects = results
            .Where(result => result.gameObject.CompareTag("AppIcon")) // Only get objects with the "AppIcon" tags
            .Select(result => result.gameObject)
            .ToArray();

        foreach (GameObject obj in hitObjects)
        {
            Utilities.SimulatePress(obj);
        }
    }
    
    private void UpdateClock()
    {
        if (_useRealTimeClock)
        {
            _phoneTime = DateTime.Now;
        }
        _phoneClockTime.text = _phoneTime.ToString("h:mm tt"); // Format: 1:02 PM
        _phoneClockDate.text = _phoneTime.ToString("MMM d, yyyy"); // Format: Jan 1, 2023
    }

    private void UpdateBattery()
    {
        if(_useRealBattery)
        {
            _batteryLevel = SystemInfo.batteryLevel == -1 ? _batteryLevel : SystemInfo.batteryLevel;
        }
        _batteryLevelText.text = $"{Mathf.Round(_batteryLevel * 100)}%";
        _batteryFillImage.fillAmount = _batteryLevel;
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
        
        _phoneBG.sprite = theme.BackgroundImage;
        _phoneUICanvas.transform.Find("HomeScreen").Find("ClockBG").GetComponent<Image>().color = theme.ClockBackgroundColor;

        _phoneClockTime.color = theme.PrimaryColor;
        _phoneClockDate.color = theme.PrimaryColor;

        // This is ugly but not called often
        foreach(Image image in _phoneUICanvas.GetComponentsInChildren<Image>())
        {
            if(!image.CompareTag("AppIcon") && !image.CompareTag("ColoredUI")) continue;

            image.color = theme.PrimaryColor;
            if (image.name == "BG") image.color = theme.TertiaryColor;
        }

        foreach (Shadow shadow in _phoneUICanvas.GetComponentsInChildren<Shadow>())
        {
            if (!shadow.CompareTag("AppIcon") && !shadow.CompareTag("ColoredUI")) continue;

            shadow.effectColor = theme.SecondaryColor;
        }

        foreach (TMP_Text text in _phoneUICanvas.GetComponentsInChildren<TMP_Text>())
        {
            if (!text.CompareTag("AppIcon") && !text.CompareTag("ColoredUI")) continue;

            text.color = theme.PrimaryColor;
        }
    }

    public void ShowNotification(string sender, string content) => ShowNotification(new PhoneMessage { Sender = sender, Content = content, Timestamp = DateTime.Now });
    public async void ShowNotification(PhoneMessage msg)
    {
        Tween.CompleteAll(_notificationPanel); // Resets any existing tweens
        // FIX THE ABOVE ^^^

        _notificationPanel.Find("Title").GetComponent<TMP_Text>().text = msg.Sender;
        _notificationPanel.Find("Text").GetComponent<TMP_Text>().text = msg.Content;
       
        AddMessage(msg);

        await Tween.UIAnchoredPositionY(_notificationPanel, -85, 0.4f);
        await UniTask.Delay(3_000);
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
        foreach(Transform child in _messagesContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var msg in _messages)
        {
            GameObject messageObject = Instantiate(_messagePrefab, _messagesContainer);
            messageObject.transform.Find("Name").GetComponent<TMP_Text>().text = msg.Sender;
            messageObject.transform.Find("Text").GetComponent<TMP_Text>().text = msg.Content;
            messageObject.transform.Find("Time").GetComponent<TMP_Text>().text = msg.Timestamp.ToString("h:mm tt"); // Format: 1:02 PM
        }
    }

    private void HideAllScreens()
    {
        _homeScreenGroup.Hide();
        _messagesScreenGroup.Hide();
        _objectivesScreenGroup.Hide();
        _settingsScreenGroup.Hide();
        _cameraScreenGroup.Hide();
        _phoneBG.enabled = true;
        _phonePhysicalCamera.enabled = false;
    }

    public void ShowHomeScreen()
    {
        HideAllScreens();

        _homeScreenGroup.Show();
    }

    public void ShowMessagesScreen()
    {
        HideAllScreens();

        _messagesScreenGroup.Show();
        LoadMessages();
    }

    public void ShowObjectivesScreen()
    {
        HideAllScreens();

        _objectivesScreenGroup.Show();
    }

    public void ShowSettingsScreen()
    {
        HideAllScreens();

        _settingsScreenGroup.Show();
    }
    
    public void ShowCameraScreen()
    {
        HideAllScreens();

        _phoneBG.enabled = false;
        _phonePhysicalCamera.enabled = true;
        _cameraScreenGroup.Show();
    }
}
