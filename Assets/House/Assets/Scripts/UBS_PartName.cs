using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UBS - Simple Object Label Highlighter
/// Names object that is in camer centre and within range
/// </summary>


public class UBS_PartName : MonoBehaviour {
// Identify objects by displaying their name when at mouse focus point
// Attach to any object that is to be highlighted

    public Transform player;
    public Text objectPartName;
    public Image crossHairPointer;

    public bool showPartId;
    public float operateRange = 3.5f;

    bool triggered;

    private void Start()
    {
        crossHairPointer.enabled = false;
    }


    private void Update()
    {
        UserInput();
        if (showPartId) NameObjectInCenterOfCam();
    }


    /// <summary>
    /// Name object that is within range
    /// </summary>
    void NameObjectInCenterOfCam()
    {
        try
        { 
            // Ray from the center of the viewport
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit rayHit;
            // Check if camera hitting something
            bool isHit = Physics.Raycast(ray, out rayHit, operateRange);
            GameObject hitObject = rayHit.collider.gameObject;
            float dist = Vector3.Distance(player.position, hitObject.transform.position);

            if (isHit && dist <= operateRange && triggered)
            {
                objectPartName.text = hitObject.name;
                crossHairPointer.enabled = true;
            }
            else
            {
                objectPartName.text = "";
                crossHairPointer.enabled = false;
            }
        }
        catch { }
    }// End of HighlightObjectInCenterOfCam

    void UserInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            triggered = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            triggered = false;
        }
    }
}