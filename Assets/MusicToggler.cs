using UnityEngine;

public class MusicToggler : MonoBehaviour
{
    public void ToggleMusic()
    {
        ContinuousMusic.Instance?.ToggleMusicPlaying();
    }
}
