using UnityEngine;

public class CarSounds : MonoBehaviour
{
    [SerializeField] AudioSource carAudioSource;
    [SerializeField] AudioClip carStart;
    [SerializeField] AudioClip carAccelerate;
    [SerializeField] AudioClip carCrash;
    public float carStartVolume = 1f;
    public float carAccelerateVolume = 1f;
    public float carCrashVolume = 1f;
    
    public void PlayCarStart()
    {
        carAudioSource.PlayOneShot(carStart, carStartVolume);
    }

    public void PlayCarAccelerate()
    {
        carAudioSource.PlayOneShot(carAccelerate, carAccelerateVolume);
    }

    public void PlayCarCrash()
    {
        carAudioSource.PlayOneShot(carCrash, carCrashVolume);
    }
}
