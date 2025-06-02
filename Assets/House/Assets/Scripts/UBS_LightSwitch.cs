using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Light switch controller
/// </summary>

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BoxCollider))]

public class UBS_LightSwitch : MonoBehaviour
{
    // UBS - Simple Light Switch Function

    // Switches one or more lights on and off

    // Script is attached to light switch object
    // Light objects are added to the lights list array in the editor
    // Sounds are attached to light switch function in the editor
    // Action text is configured to indicate player action when switch object is in focus
    // During play, Player toggles light switch to toggle the array of lights

    public enum EnumSwitchType
    {
        Toggle, Main
    }
    [ContextMenuItem("Set Data for Object Type", "SetMovementValues")]
    public EnumSwitchType switchType;

    [Space(10)]
    public Transform toggleObject;
    public Vector3 toggleRotation = new Vector3(0f, 60f, 0f);

    [Space(10)]
    public string actionText = "Press Mouse 0 to Toggle Light";
    Material highlightMaterial;
    public AudioClip sound1;
    public AudioClip sound2;

    [Space(10)]
    public bool binaryState;
    [Space(10)]
    public List<GameObject> LightBulbList = new List<GameObject>();

    AudioSource audioSource;
    Vector3 startAngle;
    Vector3 toggleAngle;

    Material originalMaterial;

    Text messageText;
    Color tempColor;


    Image crossHair;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        highlightMaterial = UBS_Global.instance.defaultObjects.highlightMaterial;

        messageText = GameObject.Find("MessageText1").GetComponent<Text>();
        crossHair = GameObject.Find("CrossHair2").GetComponent<Image>();
        crossHair.enabled = false;

        startAngle = toggleObject.localRotation.eulerAngles;
        toggleAngle = startAngle + toggleRotation;

        foreach (var linkedLightBulb in LightBulbList) // turn all lights off at startup
        {
            linkedLightBulb.GetComponent<UBS_LightBulb>().Off();
        }
    }


    void OnMouseDown()
    {
        audioSource.PlayOneShot(sound1, 1.0F);
        binaryState = !binaryState;
        if (binaryState)
        {
            toggleObject.localRotation = Quaternion.Euler(toggleAngle);
        }
        else
        {
            toggleObject.localRotation = Quaternion.Euler(startAngle);
        }
        foreach (var linkedLightBulb in LightBulbList)
        {
            if (switchType == EnumSwitchType.Toggle)
            {
                linkedLightBulb.GetComponent<UBS_LightBulb>().binaryState = !linkedLightBulb.GetComponent<UBS_LightBulb>().binaryState;
            }
            if (switchType == EnumSwitchType.Main)
            {
                linkedLightBulb.GetComponent<UBS_LightBulb>().binaryState = binaryState;
            }
        }
    }

    void OnMouseEnter()
    {
        originalMaterial = GetComponent<Renderer>().material;
        GetComponent<Renderer>().material = highlightMaterial;
        messageText.fontSize = 24;
        messageText.text = actionText;
        crossHair.enabled = true;
    }

    void OnMouseExit()
    {
        GetComponent<Renderer>().material = originalMaterial;
        messageText.text = "";
        crossHair.enabled = false;
    }

 
}