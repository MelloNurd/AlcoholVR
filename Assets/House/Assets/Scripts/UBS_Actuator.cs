// UBS - Object Actuation Function

// works with UBS_Operate script
// for actuation of scene objects such as windows and doors
// handles both rotating and sliding actuation

// attach script to object
// optionally select profile or user setting for moving part object
// if user profile selected, set rotation or slide movement parameters in editor
// assign action sounds for open and close in editor

// note that hinged objects rotate around moving part object's mesh pivot point

using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;


[Serializable]
public enum EnumObjectType
{
    User, HingeCW, HingeACW, HingeFwd, HingeBack, SlideUp, SlideDown, SlideLeft, SlideRight
}

[RequireComponent(typeof(AudioSource))]
    /// <summary>
/// Simple Object Operation
/// for actuation of rotating and sliding objects.
/// Use UBS_Operator script on operator object to provide operator interface.
/// </summary>

public class UBS_Actuator : MonoBehaviour
{
    [ContextMenuItem("Set Data for Object Type", "SetMovementValues")]
    public EnumObjectType objectType;
    public Vector3 rotate;
    public float rotationSpeed = 0f;
    public Vector3 slide;
    public float slideSpeed = 0f;

    [Space(10)]
    public bool binaryState;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Vector3 startAngle;
    private Vector3 openAngle;

    bool isOpen;
    bool isClosed;

    public enum EnumObjectState
    {
        Undefined, Closed, Opening, Open, Closing
    }

    public EnumObjectState operationState;      // op state for use by other objects
    [HideInInspector] public bool operating;

    [Space(10)]
    public AudioClip openSound;
    public AudioClip closeSound;
    AudioSource audioSource;
    Text messageText;

    [Header("Controlled Objects")]
    public List<Transform> objectList = new List<Transform>();

    private Transform player;
    private bool init = false;

    /// <summary>
    /// Set default movement values from Editor context menu
    /// </summary>

    private void SetMovementValues()
    {
        switch (objectType)
        {
            case EnumObjectType.HingeACW:
                rotate = new Vector3(0f, -88f, 0f);
                rotationSpeed = 5f;
                slide = new Vector3(0f, 0f, 0f);
                slideSpeed = 0f;
                break;
            case EnumObjectType.HingeCW:
                rotate = new Vector3(0f, 88f, 0f);
                rotationSpeed = 5f;
                slide = new Vector3(0f, 0f, 0f);
                slideSpeed = 0f;
                break;
            case EnumObjectType.HingeFwd: // Hinge Lift
                rotate = new Vector3(0f, 0f, 90f);
                rotationSpeed = 5f;
                slide = new Vector3(0f, 0f, 0f);
                slideSpeed = 0f;
                break;
            case EnumObjectType.HingeBack: // Hinge Drop
                rotate = new Vector3(0f, 0f, -90f);
                rotationSpeed = 3f;
                slide = new Vector3(0f, 0f, 0f);
                slideSpeed = 0f;
                break;
            case EnumObjectType.SlideUp: // Slide Up
                rotate = new Vector3(0f, 0f, 0f);
                rotationSpeed = 0f;
                slide = new Vector3(0f, 0f, 1.3f);
                slideSpeed = 5f;
                break;
            case EnumObjectType.SlideDown: // Slide Down
                rotate = new Vector3(0f, 0f, 0f);
                rotationSpeed = 0f;
                slide = new Vector3(0f, 0f, -1.3f);
                slideSpeed = 5f;
                break;
            case EnumObjectType.SlideLeft: // Slide Left
                rotate = new Vector3(0f, 0f, 0f);
                rotationSpeed = 0f;
                slide = new Vector3(0f, 1.3f, 0f);
                slideSpeed = 5f;
                break;
            case EnumObjectType.SlideRight: // Slide Right
                rotate = new Vector3(0f, 0f, 0f);
                rotationSpeed = 0f;
                slide = new Vector3(0f, -1.3f, 0f);
                slideSpeed = 5f;
                break;
            default:
                ;
                break;
        }
    } // End of SetMovementValues


    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Precalculate closed and open angles
        startAngle = transform.eulerAngles;
        openAngle = startAngle + rotate;

        // Precalculate closed and open slide positions
        closedPosition = transform.localPosition;
        openPosition = closedPosition + slide;

        messageText = GameObject.Find("MessageText1").GetComponent<Text>();
        operating = false;
        operationState = EnumObjectState.Undefined;

        init = true;

    } // End Start


    private void Update()
    {
        if (!init) return;

        ObjectRotate();
        ObjectSlide();

        // check if operation complete
        isClosed = Quaternion.Angle(transform.rotation, Quaternion.Euler(startAngle)) < 1f 
                        && Vector3.Distance(closedPosition, transform.localPosition) < 0.5f;
        if (isClosed) { operationState = EnumObjectState.Closed; operating = false;}
        isOpen = Quaternion.Angle(transform.rotation, Quaternion.Euler(openAngle)) < 1f 
                        && Vector3.Distance(openPosition, transform.localPosition) < 0.5f;
        if (isOpen) { operationState = EnumObjectState.Open; operating = false;}

    } // End Update


    /// <summary>
    /// Activation toggle function for Operator or other Actuator objects
    /// </summary>

    public void ActivateToggle()
    {
        if (!init) return;
        binaryState = !binaryState;
        Operate();
    }//End ActivateToggle


    /// <summary>
    /// Activation open function for Operator or other Actuator objects
    /// </summary>

    public void ActivateOpen()
    {
        if (!init) return;
        binaryState = true;
        if (!isOpen) Operate();
    } // End ActivateOpen


    /// <summary>
    /// Activation close function for Operator or other Actuator objects
    /// </summary>

    public void ActivateClose()
    {
        if (!init) return;
        binaryState = false;
        if (!isClosed) Operate();
    } // End ActivateClose


    void Operate()
    {
        if (!init) return;
        if (!operating)
        {
            operating = true; // operating latch prevents chatter
            messageText.text = "";
            if (binaryState)
            {
                audioSource.PlayOneShot(openSound, 1.0F);
                foreach (var subObject in objectList)
                {
                    subObject.GetComponent<UBS_Actuator>().ActivateOpen(); // activate subordinate objects
                }
            }
            else
            {
                audioSource.PlayOneShot(closeSound, 1.0F);
                foreach (var subObject in objectList) 
                {
                    subObject.GetComponent<UBS_Actuator>().ActivateClose(); // activate subordinate objects
                }
            }

        }
    } // End Operate


    /// <summary>
    /// Rotation around pivot to prededetermined angle (rotation) - all axes
    /// </summary>

    void ObjectRotate()
    {
        if (!init) return;
        openAngle = startAngle + rotate; //Vector3 calculate open angle; add object's start angle to requested rotation

        if (!binaryState)
        {
            // Rotate Close
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(startAngle), Time.deltaTime * rotationSpeed);
            operationState = EnumObjectState.Closing;
        }
        else
        {
            // Rotate Open
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(openAngle), Time.deltaTime * rotationSpeed);
            operationState = EnumObjectState.Opening;
        }
    } // End ObjectRotate


    /// <summary>
    /// Slide object between closed position and predetermined open position
    /// </summary>

    void ObjectSlide()
    {
        if (!init) return;
        openPosition = closedPosition + slide;

        if (!binaryState)
        {
            // Slide Close
            transform.localPosition = Vector3.Lerp(transform.localPosition, closedPosition, slideSpeed * Time.deltaTime);
            operationState = EnumObjectState.Closing;
        }
        else
        {
            // Slide Open
            transform.localPosition = Vector3.Lerp(transform.localPosition, openPosition, slideSpeed * Time.deltaTime);
            operationState = EnumObjectState.Opening;
        }
    } // End ObjectSlide
} // End of Class