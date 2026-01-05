using UnityEngine;

public class GrabbableAudioPlayer : MonoBehaviour
{
    public AudioClip audioClip; // The audio clip to play when grabbed
    
    public void PlayGrabAudio()
    {
        PlayerAudio.PlaySound(audioClip);
    }
}
