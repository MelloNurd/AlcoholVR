using UnityEngine;

public class Footsteps : MonoBehaviour
{
    [SerializeField] private AudioClip _footstepSound;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayFootstepSound()
    {
        _audioSource.pitch = Random.Range(0.85f, 1.15f);
        _audioSource.PlayOneShot(_footstepSound);
    }
}
