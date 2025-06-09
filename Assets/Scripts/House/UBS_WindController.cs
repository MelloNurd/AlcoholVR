
using UnityEngine;
using System.Collections;


/// <summary>
/// Provides simple simulated wind directon and speed 
/// attach to an empty object in Hierarchy
/// </summary>

public class UBS_WindController : MonoBehaviour
{
    public float updateTime = 5f;
    [Space(10)]
    public float directionVariance = 25f;
    public float directionVarianceLarge = 45f;
    [Space(10)]
    public float speedVariance = 0.2f;
    public float maximumWindSpeed = 3f;
    [Space (10)]
    public float windDirection = 0f;
    public float windSpeed = 1f;


    // Randomly pick starting values and transforms as startup.

    void Start()
    {
        windDirection = Random.Range(0, 360);
        transform.localRotation = Quaternion.Euler(0, windDirection, 0);
        InvokeRepeating("WindChange", updateTime, updateTime);
    }

    // Randomly adjust values.

    /// <summary>
    /// Randomly changes wind direction and speed
    /// </summary>
    void WindChange()
    {
        float delayTrigger = Random.Range(0f, 100f); // occasionally create major wind change
        if (delayTrigger > 95f) { windDirection += Random.Range(-directionVarianceLarge, directionVarianceLarge); }
        windDirection += Random.Range(-directionVariance, directionVariance);

        if (windDirection < 0) windDirection += 360;
        if (windDirection > 360) windDirection -= 360;
        windSpeed = Mathf.Clamp(windSpeed += Random.Range(-speedVariance, speedVariance), 0f, maximumWindSpeed);
        transform.localRotation = Quaternion.Euler(0, 0, windDirection); //update transform with rotation around z axis
    }

    /// <summary>
    /// Direction getter/setter function
    /// </summary>
    public float Direction
    {
        get
        {
            //Some other code
            return windDirection;
        }
        set
        {
            //Some other code
            windDirection = value;
        }
    }
    /// <summary>
    /// Wind speed getter/setter function
    /// </summary>
    public float Speed
    {
        get
        {
            //Some other code
            return windSpeed;
        }
        set
        {
            //Some other code
            windSpeed = value;
        }
    }
}