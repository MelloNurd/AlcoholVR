using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using PrimeTween;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Random = UnityEngine.Random;

public class Phone : MonoBehaviour
{
    public static Phone Instance { get; private set; }

    [Header("General")]
    [SerializeField] private bool _useRealTimeClock = true;
    [SerializeField] private bool _useRealBattery = true;
    private DateTime _phoneTime = DateTime.Now;
    private float _batteryLevel = 0.73f;
    private RectTransform _notificationPanel;
    [SerializeField] private AudioClip _clickSound;
    public bool IsActive => _phoneObj.activeSelf;
    public bool IsHandNearPhone => IsActive && Vector3.Distance(Player.Instance.RightHand.transform.position, transform.position) < 0.3f;
    public bool IsInteractable { get; set; } = true;
    private Vector3 _phoneSize;

    [Header("Audio")]
    [SerializeField] private AudioClip _phoneAppearSound;
    [SerializeField] private AudioClip _phoneDisappearSound;
    [SerializeField] private AudioClip _notificationSound;
    private AudioSource _phoneAudioSource;

    [Header("Objectives")]
    [SerializeField] private GameObject _objectivePrefab;
    private Transform _objectivesContainer;

    [Header("Messages")]
    [SerializeField] private GameObject _messagePrefab;
    private Transform _messagesContainer;
    private List<PhoneMessage> _messages = new();
    private Queue<PhoneMessage> _messageQueue = new(); // For future use, if we want to queue messages

    [Header("Settings")]
    [SerializeField, InlineButton(nameof(ApplyCurrentTheme), "Apply", buttonWidth: 50)] private PhoneTheme _currentTheme;
    [Space(3)]
    [SerializeField] private List<PhoneTheme> _availableThemes;
    private TMP_Text _batteryLevelText;
    private Image _batteryFillImage;

    private Transform _handTransform;
    private GameObject _phoneObj;
    private Camera _phonePhysicalCamera;
    private Camera _phoneUICamera; // The camera that renders the phone's UI
    private Canvas _phoneUICanvas; // The canvas that contains the phone's UI elements
    private TMP_Text _phoneClockTime;
    private TMP_Text _smallClockTime;
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

    private bool _buttonPressed = false;
    private bool _lastButtonState = true;

    private ParticleSystem _appearParticles;

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

        _phoneObj = transform.GetChild(0).gameObject;

        // Assign component variables
        _appearParticles = _phoneObj.transform.Find("AppearParticles").GetComponent<ParticleSystem>();

        _phoneUICamera = _phoneObj.transform.Find("Camera Screen").GetComponent<Camera>();
        _phoneUICanvas = _phoneObj.transform.Find("Screen (Canvas)").GetComponent<Canvas>();
        _phoneBG = _phoneUICanvas.transform.Find("BG").GetComponent<Image>();

        _batteryLevelText = _phoneUICanvas.transform.Find("Battery/Text").GetComponent<TMP_Text>();
        _batteryFillImage = _phoneUICanvas.transform.Find("Battery/IconFill").GetComponent<Image>();

        _homeScreenGroup = _phoneUICanvas.transform.Find("HomeScreen").GetComponent<CanvasGroup>();
        var temp = _homeScreenGroup.transform.Find("Clock");
        _phoneClockTime = temp.Find("Time").GetComponent<TMP_Text>();
        _phoneClockDate = temp.Find("Date").GetComponent<TMP_Text>();
        _smallClockTime = _phoneUICanvas.transform.Find("Time").GetComponentInChildren<TMP_Text>();

        _messagesScreenGroup = _phoneUICanvas.transform.Find("MessagesScreen").GetComponent<CanvasGroup>();
        _messagesContainer = _messagesScreenGroup.transform.Find("Messages");

        _objectivesScreenGroup = _phoneUICanvas.transform.Find("ObjectivesScreen").GetComponent<CanvasGroup>();
        _objectivesContainer = _objectivesScreenGroup.transform.Find("Objectives");

        _settingsScreenGroup = _phoneUICanvas.transform.Find("SettingsScreen").GetComponent<CanvasGroup>();

        _notificationGroup = _phoneUICanvas.transform.Find("Notification Popup").GetComponentInChildren<CanvasGroup>();
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

        // We leave it on its own gameobject so we can disable/enable the phone without interrupting the audio source
        var tempAudioSourceObj = new GameObject("Phone Audio Source");
        tempAudioSourceObj.transform.parent = transform.parent;
        _phoneAudioSource = tempAudioSourceObj.AddComponent<AudioSource>();
        _phoneAudioSource.playOnAwake = false;
        _phoneAudioSource.loop = false;

        if (_currentTheme == null)
        {
            int index = PlayerPrefs.GetInt("Phone_ThemeIndex", -1);
            _currentTheme = _availableThemes[index < 0 ? Random.Range(0, _availableThemes.Count) : index];

        }
        ApplyTheme(_currentTheme);
    }

    private void Start()
    {
        _handTransform = _phoneObj.transform.parent;
        _screenObject = _phoneObj.transform.Find("Screen (Canvas)").gameObject;
        _phonePhysicalCamera = GetComponentInChildren<Camera>();

        // Initialize screens (start at home)
        HideAllScreens();
        ShowHomeScreen();

        // Set phone to follow hand position (set parent to hand)
        if (_handTransform != null)
        {
            _phoneObj.transform.parent = _handTransform;
        }
        else
        {
            Debug.LogError("_handTransform not found. Phone will not follow hand position.");
        }

        _phoneSize = _phoneObj.transform.localScale;
        DisablePhone(0f, false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            string msg = Input.GetKey(KeyCode.LeftShift) ? "This is a test message" : "This is a second test message";
            QueueNotification("Test Sender", "This is a test message");
        }
        
        if (InputManager.Instance.leftController.TryGetFeatureValue(CommonUsages.menuButton, out _buttonPressed))
        {
            if (_buttonPressed && !_lastButtonState)
            {
                _lastButtonState = true;
                TogglePhone();
                return;
            }
            if (!_buttonPressed && _lastButtonState)
            {
                _lastButtonState = false;
            }
        }

        if(Input.GetKeyDown(KeyCode.C)) // Right click to show home screen
        {
            ShowCameraScreen();
        }

        UpdateClock();
        UpdateBattery();
    }

    private async void EnablePhone(float time = 0.25f, bool effects = true)
    {
        if (_messagesScreenGroup.IsVisible())
        {
            LoadMessages();
        }
        else if (_objectivesScreenGroup.IsVisible())
        {
            LoadObjectives();
        }

        if (_phoneAppearSound != null && effects)
        {
            _phoneAudioSource.transform.position = transform.position;
            _phoneAudioSource.PlayOneShot(_phoneAppearSound, 0.5f); // Play phone appear sound
        }
        IsInteractable = false;
        _phoneObj.SetActive(true);
        _phoneObj.transform.localScale = Vector3.zero;
        await Tween.Scale(_phoneObj.transform, _phoneSize, time, ease: Ease.OutBack);
        IsInteractable = true;

        DisplayNotifications();
    }

    private async void DisablePhone(float time = 0.25f, bool effects = true)
    {

        if (_phoneDisappearSound != null && effects)
        {
            _phoneAudioSource.transform.position = transform.position;
            _phoneAudioSource.PlayOneShot(_phoneDisappearSound, 0.5f); // Play phone appear sound
        }
        IsInteractable = false;
        await Tween.Scale(_phoneObj.transform, Vector3.zero, time, ease: Ease.InBack);
        _phoneObj.SetActive(false);
        if (_appearParticles != null && effects)
        {
            _appearParticles.transform.position = transform.position; // Set particle position to phone position
            _appearParticles.transform.rotation = transform.rotation;
            _appearParticles.Play();
        }

        ObjectiveManager.Instance.HideAllPaths();
    }

    private void TogglePhone()
    {
        Player.Instance.ToggleUIInteractor(); // Toggle the UI interactor to allow interaction with the phone
        if (IsActive)
        {
            DisablePhone();
        }
        else
        {
            EnablePhone();
        }
    }

    private void UpdateClock()
    {
        if (_useRealTimeClock)
        {
            _phoneTime = DateTime.Now;
        }
        _phoneClockTime.text = _phoneTime.ToString("h:mm tt"); // Format: 1:02 PM
        _smallClockTime.text = _phoneTime.ToString("h:mm tt"); // Format: 1:02
        _phoneClockDate.text = _phoneTime.ToString("MMM d, yyyy"); // Format: Jan 1, 2023
    }

    private void UpdateBattery()
    {
        if (_useRealBattery)
        {
            _batteryLevel = SystemInfo.batteryLevel == -1 ? _batteryLevel : SystemInfo.batteryLevel;
        }
        _batteryLevelText.text = $"{Mathf.Round(_batteryLevel * 100)}%";
        _batteryFillImage.fillAmount = _batteryLevel;
    }

    [Button]
    public void ApplyRandomTheme() => ApplyTheme(_availableThemes.GetRandomUnique(_currentTheme));
    public void ApplyCurrentTheme() => ApplyTheme(_currentTheme);
    public void ApplyTheme(PhoneTheme theme)
    {
        if (!Application.isPlaying) return;

        if(!_availableThemes.Contains(theme))
        {
            _availableThemes.Add(theme);
        }


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

        // This is ugly but not called often... if ever expanded upon, implement this via an interface
        foreach(Image image in _phoneUICanvas.GetComponentsInChildren<Image>())
        {
            if(!image.CompareTag("ColoredUI")) continue;

            image.color = theme.PrimaryColor;
            if (image.name == "BG") image.color = theme.TertiaryColor;
        }

        foreach (Shadow shadow in _phoneUICanvas.GetComponentsInChildren<Shadow>())
        {
            if (!shadow.CompareTag("ColoredUI")) continue;

            shadow.effectColor = theme.SecondaryColor;
        }

        foreach (TMP_Text text in _phoneUICanvas.GetComponentsInChildren<TMP_Text>())
        {
            if (!text.CompareTag("ColoredUI")) continue;

            text.color = theme.PrimaryColor;
        }

        PlayerPrefs.SetInt("Phone_ThemeIndex", _availableThemes.IndexOf(theme));
    }

    public void QueueNotification(string sender, string content) => QueueNotification(new PhoneMessage { Sender = sender, Content = content, Timestamp = DateTime.Now });
    public async void QueueNotification(PhoneMessage msg, bool showTutorial = false)
    {
        // if showTutorial is true, show player how to open phone
        _messageQueue.Enqueue(msg);

        if(!IsActive)
        {
            _phoneAudioSource.PlayOneShot(_notificationSound);
            InputManager.Instance.leftController.SendHapticImpulse(0, 0.5f, 0.3f); // Vibrate left controller
            await UniTask.Delay(600);
            InputManager.Instance.leftController.SendHapticImpulse(0, 0.5f, 0.3f); // Vibrate left controller
        }
        else
        {
            DisplayNotifications();
        }
    }

    public async void DisplayNotifications()
    {
        while (_messageQueue.Count > 0 && IsActive)
        {
            await ShowNotification(_messageQueue.Dequeue());
        }
    }

    private async UniTask ShowNotification(PhoneMessage msg)
    {
        Tween.StopAll(_notificationPanel);

        _notificationPanel.Find("Title").GetComponent<TMP_Text>().text = msg.Sender;
        _notificationPanel.Find("Text").GetComponent<TMP_Text>().text = msg.Content;
       
        AddMessage(msg);

        if(msg.Objective != null)
        {
            ObjectiveManager.Instance.CreateObjectiveObject(msg.Objective);
        }

        // notification sound + vibration
        _phoneAudioSource.transform.position = transform.position;
        _phoneAudioSource.PlayOneShot(_notificationSound, 0.5f);
        InputManager.Instance.leftController.SendHapticImpulse(0, 0.5f, 0.2f); // Vibrate left controller

        await Tween.UIAnchoredPositionY(_notificationPanel, -85, 0.4f); // move down

        await UniTask.Delay(3_000); // wait

        await Tween.UIAnchoredPositionY(_notificationPanel, 475, 0.4f); // move up
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

        LoadMessages();
    }

    public void LoadMessages()
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

    public void LoadObjectives()
    {
        foreach(Transform child in _objectivesContainer)
        {
            Destroy(child.gameObject);
        }

        var objectives = ObjectiveManager.Instance.GetSortedList();
        foreach (var objective in objectives)
        {
            // Note for later: if point is null, we don't ahve the button to guide me

            ObjectiveUI objectiveObject = Instantiate(_objectivePrefab, _objectivesContainer).GetComponent<ObjectiveUI>();
            objectiveObject.Initialize(objective);
        }
    }

    private void HideAllScreens()
    {
        _homeScreenGroup.Hide();
        _messagesScreenGroup.Hide();
        _objectivesScreenGroup.Hide();
        _settingsScreenGroup.Hide();
        _cameraScreenGroup.Hide();
        _phonePhysicalCamera.enabled = false;
        _phoneBG.enabled = true;
        _smallClockTime.enabled = true; // This should show on all screens, EXCEPT home
    }

    [Button]
    public void ShowHomeScreen()
    {
        HideAllScreens();

        _smallClockTime.enabled = false;
        _homeScreenGroup.Show();
    }

    [Button]
    public void ShowMessagesScreen()
    {
        HideAllScreens();

        _messagesScreenGroup.Show();
        LoadMessages();
    }

    [Button]
    public void ShowObjectivesScreen()
    {
        HideAllScreens();

        _objectivesScreenGroup.Show();
        LoadObjectives();
    }

    [Button]
    public void ShowSettingsScreen()
    {
        HideAllScreens();

        _settingsScreenGroup.Show();
    }

    [Button]
    public void ShowCameraScreen()
    {
        HideAllScreens();

        _phoneBG.enabled = false;
        _phonePhysicalCamera.enabled = true;
        _cameraScreenGroup.Show();
    }
}
