using Cysharp.Threading.Tasks;
using UnityEngine;

public class SkateboardingNPC : MonoBehaviour
{
    [SerializeField] private AnimationClip skateAnim;
    [SerializeField] private AnimationClip skatePushAnim;

    private Animator skatingAnim;
    private Animator npcAnim;

    private void Awake()
    {
        skatingAnim = GetComponent<Animator>();
        npcAnim = transform.GetChild(0).GetComponentInChildren<Animator>();

        npcAnim.Play(skateAnim.name);
    }

    public async void PlayPushAnim()
    {
        npcAnim.CrossFade(skatePushAnim.name, 0.2f);
        await UniTask.Delay(skatePushAnim.length.ToMS());
        npcAnim.CrossFade(skateAnim.name, 0.2f);
    }
}
