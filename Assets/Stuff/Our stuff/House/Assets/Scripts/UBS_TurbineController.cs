using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple Turbine Controller - 
/// Spins turbines/windmills based on wind speed and diameter
/// </summary>
public class UBS_TurbineController : MonoBehaviour
{

    public Transform windSpeedObject;
    public float diameter = 2f;
    public AudioClip audioClip;
    [Space (10)]
    public float rotationSpeed; // rotation speed output

     AudioSource audioSource;

    // Turbine speed constants
    float TSR = 6f;
    float Pi = 3.141592654f;
    float constant; // precalculated constant

    void Start ()
    {
        constant = 60 * TSR / (Pi * diameter);
        audioSource = GetComponent<AudioSource>();
    }

    void Update ()
    {
        float V = windSpeedObject.GetComponent<UBS_WindController>().windSpeed;
        rotationSpeed = V * constant;
        transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));

        //float delayTrigger = Random.Range(0f, 100f); // occasionally make creaking sound
        //if (delayTrigger > 99f) { audioSource.PlayOneShot(audioClip, 0.6F); }
    }
    
}
