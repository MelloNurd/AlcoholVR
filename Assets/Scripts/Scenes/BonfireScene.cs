using UnityEngine;

public class BonfireScene : MonoBehaviour
{
    [SerializeField] private AudioClip _natureSound;

    void Start()
    {
        PlayerAudio.PlayLoopingSound(_natureSound);
    }
}
