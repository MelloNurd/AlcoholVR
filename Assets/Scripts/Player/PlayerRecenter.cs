using UnityEngine;
using UnityEngine.XR;

public class PlayerRecenter : MonoBehaviour
{
    public Transform xrOrigin; // XR Origin (root of the rig)
    public Transform cameraTransform; // The Main Camera (VR headset)
    public CharacterController characterController;

    private Vector3 lastCameraWorldPos;

    void Start()
    {
        lastCameraWorldPos = cameraTransform.position;
    }

    void LateUpdate()
    {
        // Get headset movement in world space (horizontal only)
        Vector3 worldOffset = cameraTransform.position - lastCameraWorldPos;
        Vector3 horizontalOffset = new Vector3(worldOffset.x, 0, worldOffset.z);

        // Move the character controller
        characterController.Move(horizontalOffset);

        // Update last known headset position
        lastCameraWorldPos = cameraTransform.position;
    }
}
