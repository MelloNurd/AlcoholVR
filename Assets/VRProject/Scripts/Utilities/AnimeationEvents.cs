using UnityEngine;

public class AnimeationEvents : MonoBehaviour
{
    [SerializeField] private AudioClip[] rageSlamSounds;

    [Header("Footstep Sounds")]
    [SerializeField] private AudioClip _footstepHardSound;
    [SerializeField] private AudioClip _footstepDirtSound;

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
        if (_footstepDirtSound == null || _footstepHardSound == null)
        {
            Debug.LogError("[AnimeationEvents] Footstep sound is not assigned.", gameObject);
            return;
        }

        // Raycast downwards, if it hits a terrain play the dirt sound, otherwise play the hard sound
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1f))
        {
            if (hit.collider.gameObject.CompareTag("Terrain"))
            {
                SoundManager.PlaySoundAtPoint(_footstepDirtSound, transform.position, volume: 0.5f, pitch: Random.Range(0.85f, 1.15f));
            }
        }
        else
        {
            SoundManager.PlaySoundAtPoint(_footstepHardSound, transform.position, volume: 0.5f, pitch: Random.Range(0.85f, 1.15f));
        }
    }
}
