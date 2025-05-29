using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class PhysicsRig : MonoBehaviour
{
    public Transform playerHead;
    public CapsuleCollider bodyCollider;
    [SerializeField] Transform playerBody;
    [SerializeField] private DynamicMoveProvider moveProvider;

    public float bodyHeightMin = 0.5f;
    public float bodyHeightMax = 2.0f;
    public float movementSpeed = 5.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetColliderPosition();
        moveProvider.moveSpeed = movementSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        SetColliderPosition();
    }

    void SetColliderPosition()
    {
        bodyCollider.height = Mathf.Clamp(playerHead.localPosition.y, bodyHeightMin, bodyHeightMax);
        //bodyCollider.center = new Vector3(playerHead.localPosition.x, bodyCollider.height / 2, playerHead.localPosition.z);
        //playerBody.localPosition = new Vector3(playerHead.localPosition.x, bodyCollider.height / 2, playerHead.localPosition.z);
    }
}
