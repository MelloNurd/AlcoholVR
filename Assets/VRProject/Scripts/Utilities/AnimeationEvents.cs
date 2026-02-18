using UnityEngine;

public enum Foot
{
    Left,
    Right
}

public class AnimeationEvents : MonoBehaviour
{
    [SerializeField] private AudioClip[] rageSlamSounds;

    [Header("Footstep Sounds")]
    [SerializeField] private AudioClip[] _footstepIndoorSounds;
    [SerializeField] private AudioClip[] _footstepOutdoorSounds;

    [SerializeField] private Transform _leftFoot;
    [SerializeField] private Transform _rightFoot;

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

    public void PlayFootstepSound(Foot foot)
    {
        if (_footstepOutdoorSounds.Length == 0 || _footstepIndoorSounds.Length == 0)
        {
            Debug.LogError("[AnimeationEvents] Footstep sounds are not assigned.", gameObject);
            return;
        }

        Transform footTransform = (foot == Foot.Left) ? _leftFoot : _rightFoot;

        // Raycast downwards, if it hits a terrain play the dirt sound, otherwise play the hard sound
        if (Physics.Raycast(footTransform.position, Vector3.down, out RaycastHit hit, 1f, _groundLayerMask) && hit.collider.gameObject.CompareTag("Terrain"))
        {
            SoundManager.PlaySoundAtPoint(_footstepOutdoorSounds.GetRandom(), footTransform.position, volume: 0.5f);
        }
        else
        {
            SoundManager.PlaySoundAtPoint(_footstepIndoorSounds.GetRandom(), footTransform.position, volume: 0.4f);
        }
    }
}
