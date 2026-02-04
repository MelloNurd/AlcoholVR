using Cysharp.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [HideInInspector] public Loading loading;

    [HideInInspector] public XROrigin xrOrigin;
    [HideInInspector] public CharacterController controller;

    public Camera Camera => xrOrigin.Camera;

    public Vector3 CamPosition => Camera == null ? Vector3.zero : Camera.transform.position;
    public Vector3 Position => _xrRig == null ? Vector3.zero : _xrRig.transform.position;
    public Vector3 Forward => Camera == null ? Vector3.forward : Camera.transform.forward;
    public float CameraHeight
    {
        get
        {
            if(Camera == null || _xrRig == null)
            {
                return -1f;
            }
            return Camera.transform.position.y - _xrRig.transform.position.y;
        }
    }
    
    private Transform _xrRig;

    public bool IsInteractingWithNPC { get; set; } = false;
    public bool IsInDialogue { get; set; } = false;

    public GameObject RightHand { get; private set; }
    public GameObject LeftHand { get; private set; }

    private DynamicMoveProvider _moveProvider;

    private float _initialSpeed;

    TunnelingVignetteController _tunnelingVignetteController;
    GameObject RightController;
    GameObject LeftController;
    NearFarInteractor _rightNearFarInteractor;
    NearFarInteractor _leftNearFarInteractor;
    ControllerInputActionManager rightControllerInputActionManager;
    ContinuousTurnProvider continuousTurnProvider;

    bool queue = false;
    
    private bool _isInitialized = false;

    private void Awake()
    {
        Instance = this;

        xrOrigin = GetComponentInChildren<XROrigin>();

        controller = GetComponentInChildren<CharacterController>();
        _moveProvider = GetComponentInChildren<DynamicMoveProvider>();
        _initialSpeed = _moveProvider.moveSpeed;
        loading = Camera.GetComponentInChildren<Loading>();

        RightHand = transform.Find("RightHand").gameObject;
        LeftHand = transform.Find("LeftHand").gameObject;

        if (RightHand == null || LeftHand == null)
        {
            Debug.LogError("Could not find hand objects in player hierarchy. Make sure they are named 'RightHand' and 'LeftHand' and are children of the XR Rig.");
        }

        _tunnelingVignetteController = GetComponentInChildren<TunnelingVignetteController>();
        _xrRig = transform.Find("XR Origin (XR Rig)");
        RightController = _xrRig.Find("Crouch Offset/Camera Offset/Right Controller").gameObject;
        LeftController = _xrRig.Find("Crouch Offset/Camera Offset/Left Controller").gameObject;
        _rightNearFarInteractor = RightController.GetComponentInChildren<NearFarInteractor>();
        _leftNearFarInteractor = LeftController.GetComponentInChildren<NearFarInteractor>();

        rightControllerInputActionManager = RightController.GetComponent<ControllerInputActionManager>();
        continuousTurnProvider = GetComponentInChildren<ContinuousTurnProvider>();
    }

    public void Start()
    {
        _tunnelingVignetteController.enabled = SettingsManager.Instance.TunnelingVignette;
        _tunnelingVignetteController.defaultParameters.apertureSize = SettingsManager.Instance.TunnelingVignetteAperatureSize;
        _tunnelingVignetteController.defaultParameters.featheringEffect = SettingsManager.Instance.TunnelingVignetteFeathering;

        if (SettingsManager.Instance.ToggleGrab)
        {
            _rightNearFarInteractor.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.Toggle;
            _leftNearFarInteractor.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.Toggle;
        }
        else
        {
            _rightNearFarInteractor.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.StateChange;
            _leftNearFarInteractor.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.StateChange;
        }

        if (SettingsManager.Instance.RangedInteractors)
        {
            _rightNearFarInteractor.interactionLayers = LayerMask.GetMask("Default", "UI");
            _rightNearFarInteractor.enableFarCasting = true;
            _leftNearFarInteractor.interactionLayers = LayerMask.GetMask("Default", "UI");
            _leftNearFarInteractor.enableFarCasting = true;
        }
        else
        {
            _rightNearFarInteractor.enableFarCasting = false;
            _leftNearFarInteractor.enableFarCasting = false;
        }

        if(SettingsManager.Instance.SmoothTurning)
        {
            rightControllerInputActionManager.smoothTurnEnabled = true;
        }
        else
        {
            rightControllerInputActionManager.smoothTurnEnabled = false;
        }

        continuousTurnProvider.turnSpeed = SettingsManager.Instance.SmoothTurningSpeed;
        
        _isInitialized = true;
    }

    public void Update()
    {
        if (queue && _rightNearFarInteractor.interactablesSelected.Count == 0 && !SettingsManager.Instance.RangedInteractors)
        {
            _rightNearFarInteractor.enableFarCasting = true;
            //Set interaction layer to UI and Default
            _rightNearFarInteractor.interactionLayers = LayerMask.GetMask("Default", "UI");
            queue = false;
        }
    }

    public void CloseEyes(float speed = 1f) => loading?.CloseEyes(speed);
    public void OpenEyes(float speed = 1f) => loading?.OpenEyes(speed);

    public void EnableUIInteractor()
    {
        if (SettingsManager.Instance.RangedInteractors)
        {
            Debug.Log("Ranged Interactors are enabled. Shouldn't change interactors");
            return;
        }

        queue = true;
    }

    public void DisableUIInteractor()
    {
        queue = false;

        if (SettingsManager.Instance.RangedInteractors)
        {
            Debug.Log("Ranged Interactors are enabled. Shouldn't change interactors");
            return;
        }

        _rightNearFarInteractor.enableFarCasting = false;
        _rightNearFarInteractor.interactionLayers = LayerMask.GetMask("Default");
    }

    public void ToggleTunnelingVignette()
    {
        _tunnelingVignetteController.enabled = !_tunnelingVignetteController.enabled;
        SettingsManager.Instance.TunnelingVignette = _tunnelingVignetteController.enabled;
    }

    public void SetTunnelingVignetteAperatureSize(float value)
    {
        _tunnelingVignetteController.defaultParameters.apertureSize = value;
        SettingsManager.Instance.TunnelingVignetteAperatureSize = value;
    }

    public void SetTunnelingVignetteFeathering(float value)
    {
        _tunnelingVignetteController.defaultParameters.featheringEffect = value;
        SettingsManager.Instance.TunnelingVignetteFeathering = value;
    }

    public void ToggleSmoothTurning(bool value)
    {
        SettingsManager.Instance.SmoothTurning = value;
        rightControllerInputActionManager.smoothTurnEnabled = value;
    }

    public void SetSmoothTurningSpeed(float value)
    {
        SettingsManager.Instance.SmoothTurningSpeed = value;
        continuousTurnProvider.turnSpeed = value;
    }

    public void ToggleGrabToggle(bool value)
    {
        if (value)
        {
            _rightNearFarInteractor.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.Toggle;
            _leftNearFarInteractor.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.Toggle;
        }
        else
        {
            _rightNearFarInteractor.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.StateChange;
            _leftNearFarInteractor.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.StateChange;
        }
    }

    public void ToggleRangedInteractors(bool value)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("Player not yet initialized. Deferring ToggleRangedInteractors until after Start().");
            return;
        }

        SettingsManager.Instance.RangedInteractors = value;
        if (value)
        {
            _rightNearFarInteractor.interactionLayers = LayerMask.GetMask("Default", "UI");
            _rightNearFarInteractor.enableFarCasting = true;
            _leftNearFarInteractor.interactionLayers = LayerMask.GetMask("Default", "UI");
            _leftNearFarInteractor.enableFarCasting = true;
        }
        else
        {
            _rightNearFarInteractor.interactionLayers = LayerMask.GetMask("UI");
            _leftNearFarInteractor.interactionLayers = LayerMask.GetMask("UI");
            _rightNearFarInteractor.enableFarCasting = false;
            _leftNearFarInteractor.enableFarCasting = false;
        }
    }

    public void DisableTutorialSettings()
    {
        _rightNearFarInteractor.interactionLayers = LayerMask.GetMask("UI");
        _rightNearFarInteractor.enableFarCasting = false;
        _leftNearFarInteractor.interactionLayers = LayerMask.GetMask("UI");
        _leftNearFarInteractor.enableFarCasting = false;
        SettingsManager.Instance.RangedInteractors = false;
    }

    public void DisabledRangedInteractors()
    {
        _leftNearFarInteractor.enableFarCasting = false;
        _rightNearFarInteractor.enableFarCasting = false;
    }

    public void DisableMovement()
    {
        _moveProvider.moveSpeed = 0f;
    }

    public void EnableMovement()
    {
        _moveProvider.moveSpeed = _initialSpeed;
    }

    public void ForceRelease(string hand)
    {
        // make text all lowercase
        hand = hand.ToLowerInvariant();
        if (hand == "right")
        {
            _rightNearFarInteractor.enabled = false;
            _rightNearFarInteractor.enabled = true;
        }
        else if (hand == "left")
        {
            _leftNearFarInteractor.enabled = false;
            _leftNearFarInteractor.enabled = true;
        }
        else
        {
            Debug.LogError("Invalid hand specified for ForceRelease: " + hand);
        }
    }
}
