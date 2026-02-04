using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class CrouchToggler : MonoBehaviour
{
    private CharacterController _characterController;
    private Transform _crouchOffset;
    private float _normalHeight;
    private float _crouchedHeight;
    private Vector3 _normalCameraOffsetParentPos;
    private bool _isCrouching;
    
    [SerializeField] private float crouchHeightMultiplier = 0.5f;
    
    private bool _lastButtonState = false;
    private bool _lastKeyState = false;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        
        if (_characterController == null)
        {
            Debug.LogError("CharacterController not found on CrouchToggler gameobject!");
            return;
        }

        // Find the Camera Offset parent object in the XR Origin hierarchy
        XROrigin xrOrigin = GetComponentInChildren<XROrigin>();
        if (xrOrigin == null)
        {
            Debug.LogError("XROrigin not found in children!");
            return;
        }

        _crouchOffset = xrOrigin.transform.Find("Crouch Offset");
        if (_crouchOffset == null)
        {
            Debug.LogError("Camera Offset Parent not found in XROrigin! Make sure you've created it and named it exactly 'Camera Offset Parent'");
            return;
        }

        _normalHeight = _characterController.height;
        _crouchedHeight = _normalHeight * crouchHeightMultiplier;
        _normalCameraOffsetParentPos = _crouchOffset.localPosition;
        _isCrouching = false;
        
        Debug.Log("CrouchToggler initialized. Normal height: " + _normalHeight + ", Crouched height: " + _crouchedHeight);
    }

    private void Update()
    {
        // Check keyboard input (C key for testing)
        if (Keyboard.current != null && Keyboard.current.cKey.isPressed)
        {
            if (!_lastKeyState)
            {
                _lastKeyState = true;
                Debug.Log("C key pressed - Current crouch state: " + _isCrouching);
                ToggleCrouch();
            }
        }
        else
        {
            _lastKeyState = false;
        }

        bool buttonPressed = false;

        // Check right controller's primary button (typically the A button)
        if (InputManager.Instance != null && InputManager.Instance.rightController.isValid)
        {
            if (InputManager.Instance.rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonPressed))
            {
                if (buttonPressed && !_lastButtonState)
                {
                    _lastButtonState = true;
                    Debug.Log("A button pressed - Current crouch state: " + _isCrouching);
                    ToggleCrouch();
                }
                else if (!buttonPressed && _lastButtonState)
                {
                    _lastButtonState = false;
                }
            }
        }
    }

    private void ToggleCrouch()
    {
        Debug.Log("ToggleCrouch called. Currently crouching: " + _isCrouching);
        
        if (_isCrouching)
        {
            StopCrouch();
        }
        else
        {
            StartCrouch();
        }
    }

    private void StartCrouch()
    {
        Debug.Log("StartCrouch called");
        _isCrouching = true;
        
        // Calculate height difference
        float heightDifference = _normalHeight - _crouchedHeight;
        
        // Halve the CharacterController height
        _characterController.height = _crouchedHeight;
        
        // Adjust center to keep collider grounded
        Vector3 newCenter = _characterController.center;
        newCenter.y = _crouchedHeight / 2f;
        _characterController.center = newCenter;
        
        // Move Camera Offset Parent down
        Vector3 parentPos = _normalCameraOffsetParentPos;
        parentPos.y -= heightDifference / 2f;
        _crouchOffset.localPosition = parentPos;
        
        Debug.Log("Crouched! CC Height: " + _characterController.height + " | Camera Offset Parent Y: " + _crouchOffset.localPosition.y);
    }

    private void StopCrouch()
    {
        Debug.Log("StopCrouch called");
        _isCrouching = false;
        
        // Restore CharacterController height
        _characterController.height = _normalHeight;
        
        // Reset center
        Vector3 newCenter = _characterController.center;
        newCenter.y = _normalHeight / 2f;
        _characterController.center = newCenter;
        
        // Restore Camera Offset Parent to normal position
        _crouchOffset.localPosition = _normalCameraOffsetParentPos;
        
        Debug.Log("Standing! CC Height: " + _characterController.height + " | Camera Offset Parent Y: " + _crouchOffset.localPosition.y);
    }
}
