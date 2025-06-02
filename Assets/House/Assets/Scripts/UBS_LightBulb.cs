using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class UBS_LightBulb : MonoBehaviour {

    public Vector3 lightPosition = new Vector3(0f, -0.1f, 0f);
    public Flare lightFlare;
    Light lightB;
    GameObject lightBGO;

    public Material glowMaterial;
    Material originalMaterial;
    [Space(10)]
    public bool binaryState;

    private bool init;

    // Use this for initialization
    void Start ()
    {
        var materials = transform.GetComponent<MeshRenderer>().materials;
        originalMaterial = materials[1];
        lightBGO = Instantiate(Resources.Load("DefaultLightBulb")) as GameObject;
        lightBGO.transform.parent = transform;
        lightBGO.transform.name = "bulbLight";
        lightBGO.transform.position = transform.position + lightPosition;
        lightB = lightBGO.GetComponent<Light>();
        lightB.flare = lightFlare;
        lightB.enabled = binaryState;

        init = true;
    }

    private void Update()
    {
        if (!init) return;
        if (binaryState)
        {

            On();
        }
        else
        {
            Off();
        }
    }

    /// <summary>
    /// Light bulb on simulation
    /// </summary>

    public void On()
    {
        if (!init) return;
        binaryState = true;
        lightB.enabled = true;
        var materials = transform.GetComponent<MeshRenderer>().materials;
        materials[1] = glowMaterial;
        transform.GetComponent<MeshRenderer>().materials = materials;
    }// End of On


    /// <summary>
	/// Light bulb off simulation
	/// </summary>
    public void Off()
    {
        if (!init) return;
        binaryState = false;
        lightB.enabled = false;
        var materials = transform.GetComponent<MeshRenderer>().materials;
        materials[1] = originalMaterial;
        transform.GetComponent<MeshRenderer>().materials = materials;
    }// End of Off

 }
