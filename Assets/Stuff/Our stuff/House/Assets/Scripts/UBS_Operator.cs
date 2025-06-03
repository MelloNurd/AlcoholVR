using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object Operation Highlight and Mouse Trigger Controller
/// for actuation of objects with attached Actuator script
/// </summary>


[RequireComponent(typeof(BoxCollider))]

public class UBS_Operator : MonoBehaviour
{

    // UBS - Operation Trigger

    // object selection and mouse triggered operation

    public enum EnumOperatorType
    {
        Toggle, Open, Close
    }
    public EnumOperatorType operatorType;

    private bool init = false;
    private Text messageText;
    private Transform player;

    public float operateRange = 3.5f;
    public string actionText = "Press Mouse 0 to Operate";

    Image crossHair;

    Material highlightMaterial;
    Material originalMaterial;
    GameObject lastHighlightedObject;

    [Header("Status")]
    public bool inRange;         //"in range to operate" flag
    [HideInInspector] public bool triggered;         //one-shot trigger to initiate operation locally or by external objects

    [Header("Controlled Objects")]
    public List<Transform> objectList = new List<Transform>();



    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        messageText = GameObject.Find("MessageText1").GetComponent<Text>();
        crossHair = GameObject.Find("CrossHair2").GetComponent<Image>();
        highlightMaterial = UBS_Global.instance.defaultObjects.highlightMaterial;
        crossHair.enabled = false;
        init = true;

    } // End Start


    private void Update()
    {
        if (!init) return;

        HighlightObjectInCenterOfCam();
        UserInput();

    }// End Update


    /// <summary>
	/// Highlight operator object that is wiithin range
	/// </summary>
    void HighlightObjectInCenterOfCam()
    {
        Ray ray;
        RaycastHit rayHit;
        bool isHit;
        GameObject hitObject;
        bool isHittingMe;
        float dist;

        if (!init) return;

        try
        { 
            // Ray from the center of the viewport.
            ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            // Check if we hit something.
            isHit = Physics.Raycast(ray, out rayHit, operateRange);
            hitObject = rayHit.collider.gameObject;
            isHittingMe = isHit && (hitObject == gameObject); // The object hit is this object
            dist = Vector3.Distance(player.position, hitObject.transform.position);

            if (isHittingMe && dist <= operateRange)
            {
                HighlightObject();
                inRange = true;
            }
            else
            {
                ClearHighlighted();
                inRange = false;
            }
        }
        catch
        { }

    }// End of HighlightObjectInCenterOfCam


    /// <summary>
	/// Highlight operator object
	/// </summary>

    void HighlightObject()
    {
        if (!init) return;
        if (lastHighlightedObject != gameObject)
        {
            ClearHighlighted();
            originalMaterial = transform.GetComponent<MeshRenderer>().sharedMaterial;
            transform.GetComponent<MeshRenderer>().sharedMaterial = highlightMaterial;
            lastHighlightedObject = gameObject;
            messageText.fontSize = 24;
            messageText.text = actionText;
            crossHair.enabled = true;
        }
    }// End of HighlightObject


    /// <summary>
	/// Clear highlight of operator object
	/// </summary>
    void ClearHighlighted()
    {
        if (!init) return;
        if (lastHighlightedObject != null)
        {
            lastHighlightedObject.GetComponent<MeshRenderer>().sharedMaterial = originalMaterial;
            lastHighlightedObject = null;
            messageText.text = "";
            crossHair.enabled = false;
        }
    }// End of ClearHighlighted


    void UserInput()
    {
        if (!init) return;
        if (inRange)
        {
            if (Input.GetMouseButtonDown(0))
            {
                triggered = true;
            }
            if (Input.GetMouseButtonUp(0))
            {
                triggered = false;
            }
        }
        if (triggered)
        {
            triggered = false; // One shot trigger latch prevents chatter
            foreach (var subObject in objectList)
            {
                switch (operatorType)
                {
                    case EnumOperatorType.Toggle:
                        subObject.GetComponent<UBS_Actuator>().ActivateToggle(); // activate toggle for controlled objects
                        break;
                    case EnumOperatorType.Open:
                        subObject.GetComponent<UBS_Actuator>().ActivateOpen(); // activate toggle for controlled objects
                        break;
                    case EnumOperatorType.Close:
                        subObject.GetComponent<UBS_Actuator>().ActivateClose(); // activate toggle for controlled objects
                        break;
                    default:
                        ;
                        break;
                }
            }
        }
    } // End TriggerOperation

} //End of Class