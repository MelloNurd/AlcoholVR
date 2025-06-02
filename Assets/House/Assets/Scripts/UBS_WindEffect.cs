using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

/// <summary>
/// Wind Effect Controller - 
/// Simulates effect of wind on rotation of object
/// </summary>
public class UBS_WindEffect : MonoBehaviour
{
    public Transform windDirectionObject;
    public float rotationSpeed = 0.1f;
    public float updateTime = 4f;
    public float directionVariance = 8f; // local variance
    public AudioClip audioClip;

    float direction;
    float directionOffset;
    Vector3 targetRotation;
    AudioSource audioSource;
    bool init;

    private void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        init = true;
        WindChange();
        InvokeRepeating("WindChange", updateTime, updateTime);
    }
    private void Update()
    {
        if (!init) return;
        float direction = windDirectionObject.GetComponent<UBS_WindController>().windDirection + directionOffset;
        targetRotation = new Vector3(transform.rotation.x - 90, transform.rotation.y , transform.rotation.z + direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(targetRotation), Time.deltaTime * rotationSpeed);
    }

    void WindChange()
    {
        if (!init) return;
            float delayTrigger = Random.Range(0f, 100f); // occasionally create major wind change
        if (delayTrigger > 60f)
        {
            directionOffset += Random.Range(-directionVariance, directionVariance);
            audioSource.PlayOneShot(audioClip, 0.6F);
        }
    }
}
