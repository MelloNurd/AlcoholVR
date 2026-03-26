using UnityEngine;

public class ContinuousMusic : MonoBehaviour
{
    public static ContinuousMusic Instance { get; private set; }
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        audioSource = GetComponent<AudioSource>();
    }

    public void PauseMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.UnPause();
        }
    }

    public void ToggleMusicPlaying()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
        else
        {
            audioSource.UnPause();
        }
    }

    public void StopMusic()
    {
        Destroy(gameObject);
        Instance = null;
    }
}
