using UnityEngine;

public class LookAt : MonoBehaviour
{
    public Transform objectToLookAt;

    public float lookAtRange = 3.0f;
    [Range(0, 1)] public float weight = 1.0f;
    [Range(0, 1)] public float bodyWeight = 0.2f;
    [Range(0, 1)] public float headWeight = 1.0f;
    [Range(0, 1)] public float eyesWeight = 1.0f;
    [Range(0, 1)] public float clampWeight = 0.5f;
    // Speed of head movement towards the target position
    public float smoothSpeed = 5f;
    // Speed of head snapping when within range
    public float weightSmoothSpeed = 2f; 


    private Animator animator;
    private Vector3 lookPosition;
    private float currentLookWeight = 0f;

    public bool isLooking = false;
    public bool lookWithinRange = false;
    private bool isInRange = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (objectToLookAt != null)
            lookPosition = objectToLookAt.position;
    }

    void Update()
    {
        if (lookWithinRange)
        {
            isInRange = Vector3.Distance(transform.position, objectToLookAt.position) <= lookAtRange;
        }
        else
        {
            isInRange = true;
        }

        bool shouldLook = isLooking && isInRange;

        float targetWeight = shouldLook ? weight : 0f;
        currentLookWeight = Mathf.Lerp(currentLookWeight, targetWeight, Time.deltaTime * weightSmoothSpeed);

        if (shouldLook)
        {
            lookPosition = Vector3.Lerp(lookPosition, objectToLookAt.position, Time.deltaTime * smoothSpeed);
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator && objectToLookAt)
        {
            animator.SetLookAtWeight(currentLookWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
            animator.SetLookAtPosition(lookPosition);
        }
    }

    public void LookAtPlayer()
    {
        objectToLookAt = Player.Instance.Camera.transform;
        isLooking = true;
    }

    public void SetLookAtRange(float range) => lookAtRange = range;

    void OnDrawGizmos()
    {
        if (objectToLookAt != null && isLooking)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up * 1.5f, objectToLookAt.position);
        }
    }
}
