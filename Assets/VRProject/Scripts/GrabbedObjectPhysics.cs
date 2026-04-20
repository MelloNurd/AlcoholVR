using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class GrabbedObjectPhysics : MonoBehaviour
{
    [SerializeField] NearFarInteractor leftInteractor;
    [SerializeField] NearFarInteractor rightInteractor;
    [SerializeField] ObjectPool propBubblePool; // Pool for prop bubble objects
    [SerializeField] float bubbleSmoothSpeed = 0.1f; // Adjust for desired smoothness
    GameObject leftPropBubble;
    GameObject rightPropBubble;
    GameObject leftHeldObject;
    GameObject rightHeldObject;
    bool leftIsGrabbing;
    bool rightIsGrabbing;
    Vector3 leftVelocity = Vector3.zero;
    Vector3 rightVelocity = Vector3.zero;
    public float delay = 5f;
    PropPhysics leftPropPhysics;
    PropPhysics rightPropPhysics;

    void Start()
    {
        leftInteractor.selectEntered.AddListener(OnLeftGrab);
        leftInteractor.selectExited.AddListener(OnLeftRelease);
        rightInteractor.selectEntered.AddListener(OnRightGrab);
        rightInteractor.selectExited.AddListener(OnRightRelease);
    }

    public void Update()
    {
        if(leftIsGrabbing && leftPropBubble != null)
        {
            leftPropBubble.transform.position = Vector3.SmoothDamp(
                leftPropBubble.transform.position,
                leftHeldObject.transform.position,
                ref leftVelocity,
                bubbleSmoothSpeed
            );
            // Keep refreshing the bubble timer every frame while grabbing
            if (leftPropPhysics != null)
            {
                leftPropPhysics.RefreshBubble();
            }
        }
        if(rightIsGrabbing && rightPropBubble != null)
        {
            rightPropBubble.transform.position = Vector3.SmoothDamp(
                rightPropBubble.transform.position,
                rightHeldObject.transform.position,
                ref rightVelocity,
                bubbleSmoothSpeed
            );
            // Keep refreshing the bubble timer every frame while grabbing
            if (rightPropPhysics != null)
            {
                rightPropPhysics.RefreshBubble();
            }
        }
    }

    void OnLeftGrab(SelectEnterEventArgs args)
    {
        leftHeldObject = args.interactableObject.transform.gameObject;
        leftPropBubble = propBubblePool.GetObject();
        leftPropBubble.transform.position = leftHeldObject.transform.position;
        leftPropBubble.SetActive(true);
        leftIsGrabbing = true;
        leftVelocity = Vector3.zero; // Reset velocity for smooth start
        
        leftPropPhysics = leftHeldObject.GetComponent<PropPhysics>();
        if (leftPropPhysics != null)
        {
            leftPropPhysics.EnterBubble();
        }
    }

    void OnLeftRelease(SelectExitEventArgs args)
    {
        leftIsGrabbing = false;
        leftHeldObject = null;
        if (leftPropBubble != null)
        {
            StartCoroutine(ReleaseDelay(leftPropBubble, true)); // true = is left
            leftPropBubble = null; // Clear reference after coroutine starts
        }
    }

    void OnRightGrab(SelectEnterEventArgs args)
    {
        rightHeldObject = args.interactableObject.transform.gameObject;
        rightPropBubble = propBubblePool.GetObject();
        rightPropBubble.transform.position = rightHeldObject.transform.position;
        rightPropBubble.SetActive(true);
        rightIsGrabbing = true;
        rightVelocity = Vector3.zero; // Reset velocity for smooth start
        
        rightPropPhysics = rightHeldObject.GetComponent<PropPhysics>();
        if (rightPropPhysics != null)
        {
            rightPropPhysics.EnterBubble();
        }
    }   

    void OnRightRelease(SelectExitEventArgs args)
    {
        rightIsGrabbing = false;
        rightHeldObject = null;
        if (rightPropBubble != null)
        {
            StartCoroutine(ReleaseDelay(rightPropBubble, false)); // false = is right
            rightPropBubble = null; // Clear reference after coroutine starts
        }
    }

    IEnumerator ReleaseDelay(GameObject propBubble, bool isLeftBubble)
    {
        yield return new WaitForSeconds(delay);
        // Safely return the bubble to pool
        if (propBubble != null)
        {
            propBubblePool.ReturnObject(propBubble);
        }
    }
}
