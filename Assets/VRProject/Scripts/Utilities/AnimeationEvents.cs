using UnityEngine;

public class AnimeationEvents : MonoBehaviour
{
    [SerializeField] private AudioClip[] rageSlamSounds;

    [Header("Footstep Sounds")]
    [SerializeField] private AudioClip[] _footstepIndoorSounds;
    [SerializeField] private AudioClip[] _footstepOutdoorSounds;

    int slamIndex = -1;
    int _groundLayerMask;

    private void Start()
    {
        _groundLayerMask = LayerMask.GetMask("Ground");
    }

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
        if (_footstepOutdoorSounds.Length == 0 || _footstepIndoorSounds.Length == 0)
        {
            Debug.LogError("[AnimeationEvents] Footstep sounds are not assigned.", gameObject);
            return;
        }

        // Raycast downwards, if it hits a terrain play the dirt sound, otherwise play the hard sound
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1f, _groundLayerMask) && hit.collider.gameObject.CompareTag("Terrain"))
        {
            SoundManager.PlaySoundAtPoint(_footstepOutdoorSounds.GetRandom(), transform.position, volume: 0.5f, pitch: Random.Range(0.85f, 1.15f));

            if(transform.parent.parent.name == "Drunk Driving NPC")
                Debug.Log("Playing outdoor footstep sound.", gameObject);
        }
        else
        {
            SoundManager.PlaySoundAtPoint(_footstepIndoorSounds.GetRandom(), transform.position, volume: 0.5f, pitch: Random.Range(0.85f, 1.15f));

            if(transform.parent.parent.name == "Drunk Driving NPC")
                Debug.Log("Playing indoor footstep sound.", gameObject);
        }
    }
}
