using UnityEngine;

public class Footsteps : MonoBehaviour
{
    [SerializeField] private AudioClip _footstepSound;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    
}
