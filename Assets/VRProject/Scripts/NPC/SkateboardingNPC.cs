using Cysharp.Threading.Tasks;
using UnityEngine;

public class SkateboardingNPC : MonoBehaviour
{
    [SerializeField] private AudioClip _skatingAudio;
    [SerializeField] private AnimationClip skateAnim;
    [SerializeField] private AnimationClip skatePushAnim;

    private Animator skatingAnim;
    private Animator npcAnim;

    private async void Awake()
    {
        skatingAnim = GetComponent<Animator>();
        npcAnim = transform.GetChild(0).GetComponentInChildren<Animator>();

        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = _skatingAudio;
        audioSource.loop = true;
        audioSource.Play();

        await UniTask.Delay(200);

        npcAnim.Play(skateAnim.name);
    }

    public async void PlayPushAnim(int numberOfPushes)
    {
        npcAnim.CrossFade(skatePushAnim.name, 0.2f);
        
        await UniTask.Delay((skatePushAnim.length * numberOfPushes).ToMS());
        
        npcAnim.CrossFade(skateAnim.name, 0.2f);
    }
}
