using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class Phone : MonoBehaviour
{
    public static Phone Instance { get; private set; }

    public bool IsUsingCamera => _phonePhysicalCamera.enabled;

    [Header("Materials")]
    [SerializeField, Required] private Material _uiMaterial;
    [SerializeField, Required] private Material _cameraMaterial;

    [Header("Phone Settings")]
    [SerializeField] private bool _useRealTimeClock = true;

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
        _phoneClockTime = _phoneUICanvas.transform.Find("Clock").Find("Time").GetComponent<TMP_Text>();
        _phoneClockDate = _phoneUICanvas.transform.Find("Clock").Find("Date").GetComponent<TMP_Text>();

        // Initialize values
        _phoneScreenMeshRenderer.material = _uiMaterial;
        _phonePhysicalCamera.enabled = false;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            ToggleCamera();
        }

        if(_useRealTimeClock)
        {
            UpdateClock(); // Uses the time from the system clock
        }
    }

    private void ToggleCamera()
    {
        _phonePhysicalCamera.enabled = !_phonePhysicalCamera.enabled;
        _phoneScreenMeshRenderer.material = IsUsingCamera ? _cameraMaterial : _uiMaterial;
    }

    private void UpdateClock()
    {
        System.DateTime now = System.DateTime.Now;
        _phoneClockTime.text = now.ToString("h:mm tt"); // Format: 1:02 PM
        _phoneClockDate.text = now.ToString("MMM d, yyyy"); // Format: Jan 1, 2023
    }
}
