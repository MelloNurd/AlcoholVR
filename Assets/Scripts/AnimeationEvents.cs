using UnityEngine;

public class AnimeationEvents : MonoBehaviour
{
    [SerializeField] private AudioClip[] rageSlamSounds;

    [SerializeField] private AudioSource audioSource;

    int audioIndex = -1;

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
        audioIndex = GetRageSlamSounds();
        if (audioIndex == -1) return;

        audioSource.PlayOneShot(rageSlamSounds[audioIndex]);
    }

    public void PlaySecondSlam()
    {
        if (audioIndex == -1) return;
        if (audioIndex + 1 >= rageSlamSounds.Length)
        {
            audioSource.PlayOneShot(rageSlamSounds[audioIndex]);
            return;
        }

        audioSource.PlayOneShot(rageSlamSounds[audioIndex + 1]);
    }
}
