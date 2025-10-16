using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class CutsceneScript : MonoBehaviour
{
    [SerializeField] private AudioSource _argumentAudioSource;
    [SerializeField] private AudioSource _fireAudioSource;

    async void Start()
    {
        float tempArgumentVolume = _argumentAudioSource.volume;
        float tempFireVolume = _fireAudioSource.volume;

        _ = Tween.AudioVolume(_argumentAudioSource, 0f, tempArgumentVolume, 2f);
        _ = Tween.AudioVolume(_fireAudioSource, 0f, tempFireVolume, 2f);

        PlayerFace.Instance.BlurVision(0.01f);

        await UniTask.Delay(2000);

        Player.Instance.CloseEyes(0.15f);

        await UniTask.Delay(800);

        Player.Instance.OpenEyes(0.3f);

        await UniTask.Delay(100);

        Player.Instance.CloseEyes(0.15f);

        await UniTask.Delay(4000);

        _ = Tween.AudioVolume(_argumentAudioSource, 0f, 8f);
        _ = Tween.AudioVolume(_fireAudioSource, 0f, 8f);

        await UniTask.Delay(8000);

        Player.Instance.loading.LoadSceneByName("EndScene");
    }

    private void FadeOutAudio()
    {

    }
}
