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

    [HideInInspector] public Camera playerCamera;
    [HideInInspector] public CharacterController controller;

    public Vector3 Position => playerCamera == null ? Vector3.zero : playerCamera.transform.position;
    public Vector3 Forward => playerCamera == null ? Vector3.forward : playerCamera.transform.forward;

    public bool IsInteractingWithNPC { get; set; } = false;

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        playerCamera = Camera.main;

        controller = GetComponentInChildren<CharacterController>();
        _moveProvider = GetComponentInChildren<DynamicMoveProvider>();
        _initialSpeed = _moveProvider.moveSpeed;
        loading = playerCamera.GetComponentInChildren<Loading>();

        RightHand = transform.Find("XR Origin (XR Rig)/Camera Offset/Right Controller/RightHand").gameObject;
        LeftHand = transform.Find("XR Origin (XR Rig)/Camera Offset/Left Controller/LeftHand").gameObject;

        if (RightHand == null || LeftHand == null)
        {
            Debug.LogError("Could not find hand objects in player hierarchy. Make sure they are named 'RightHand' and 'LeftHand' and are children of the XR Rig.");
        }

        _tunnelingVignetteController = GetComponentInChildren<TunnelingVignetteController>();
        RightController = transform.Find("XR Origin (XR Rig)/Camera Offset/Right Controller").gameObject;
        LeftController = transform.Find("XR Origin (XR Rig)/Camera Offset/Left Controller").gameObject;
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
            _rightNearFarInteractor.enableFarCasting = true;
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
    }

    public void CloseEyes(float speed = 1f) => loading?.CloseEyes(speed);
    public void OpenEyes(float speed = 1f) => loading?.OpenEyes(speed);

    public void DisableMovement()
    {
        _moveProvider.moveSpeed = 0f;
    }

    public void EnableMovement()
    {
        _moveProvider.moveSpeed = _initialSpeed;
    }
}
