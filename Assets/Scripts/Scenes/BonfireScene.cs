using UnityEngine;

public class BonfireScene : MonoBehaviour
{
    [SerializeField] private AudioClip _natureSound;

    [Header("Player Friend")]
    [SerializeField] private Transform _coolerTransform;

    void Start()
    {
        PlayerAudio.PlayLoopingSound(_natureSound);
    }
}
