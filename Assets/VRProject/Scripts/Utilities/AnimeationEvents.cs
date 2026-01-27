using UnityEngine;

public class AnimeationEvents : MonoBehaviour
{
    [SerializeField] private AudioClip[] rageSlamSounds;
    [SerializeField] private AudioClip _footstepSound;

    int slamIndex = -1;

    private int GetRageSlamSounds()
    {
        // Rage sounds are ordered in order of intensity, so we always want to play two in a row
        if (rageSlamSounds.Length == 0)
        {
            Debug.LogError("[PartyScene] Rage sounds have zero clips assigned.");
            return -1;
        }

        return Random.Range(0, rageSlamSounds.Length - 1);
    }

    public void PlayFirstSlam()
    {
        slamIndex = GetRageSlamSounds();
        if (slamIndex == -1) return;

        SoundManager.PlaySoundAtPoint(rageSlamSounds[slamIndex], transform.position);
    }

    public void PlaySecondSlam()
    {
        if (slamIndex == -1) return;
        if (slamIndex + 1 >= rageSlamSounds.Length)
        {
            SoundManager.PlaySoundAtPoint(rageSlamSounds[slamIndex], transform.position);
            return;
        }

        SoundManager.PlaySoundAtPoint(rageSlamSounds[slamIndex + 1], transform.position);
    }

    public void PlayFootstepSound()
    {
        if (_footstepSound == null)
        {
            Debug.LogError("[AnimeationEvents] Footstep sound is not assigned.", gameObject);
            return;
        }

        SoundManager.PlaySoundAtPoint(_footstepSound, transform.position, volume: 0.5f, pitch: Random.Range(0.85f, 1.15f));
    }
}
