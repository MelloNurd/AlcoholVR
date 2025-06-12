using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public float flickerIntensity = 1;
    public float flickerSpeed = 1f; // Speed of flickering

    private float _initialIntensity;

    private Light _lightSource;

    private void Awake()
    {
        _lightSource = GetComponent<Light>();
        _initialIntensity = _lightSource.intensity;
    }

    private void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed * 2.0f, 0) * 2 - 1;
        float randomJitter = Random.Range(-0.5f, 0.5f) * flickerIntensity * 0.4f * Mathf.Lerp(0.5f, 1.5f, flickerSpeed);

        // Occasional larger flicker - probability increases with flickerSpeed
        if (Random.value < 0.05f * flickerSpeed)
        {
            randomJitter *= 3f;
        }

        float speedMultiplier = Mathf.Lerp(0.7f, 1.3f, flickerSpeed);
        _lightSource.intensity = _initialIntensity + ((noise * flickerIntensity) + randomJitter) * speedMultiplier;
    }
}
