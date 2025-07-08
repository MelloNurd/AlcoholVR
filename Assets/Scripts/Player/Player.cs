using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public Camera playerCamera;
    public CharacterController controller;

    public Vector3 Position => playerCamera == null ? Vector3.zero : playerCamera.transform.position;

    public GameObject RightHand { get; private set; }
    public GameObject LeftHand { get; private set; }

    private DynamicMoveProvider _moveProvider;

    private float _initialSpeed;

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

        RightHand = transform.Find("PhysicsRig/XR Origin (XR Rig)/Camera Offset/Right Controller/RightHand").gameObject;
        LeftHand = transform.Find("PhysicsRig/XR Origin (XR Rig)/Camera Offset/Left Controller/LeftHand").gameObject;

        if (RightHand == null || LeftHand == null)
        {
            Debug.LogError("Could not find hand objects in player hierarchy. Make sure they are named 'RightHand' and 'LeftHand' and are children of the XR Rig.");
        }
    }

    public void DisableMovement()
    {
        _moveProvider.moveSpeed = 0f;
    }

    public void EnableMovement()
    {
        _moveProvider.moveSpeed = _initialSpeed;
    }
}
