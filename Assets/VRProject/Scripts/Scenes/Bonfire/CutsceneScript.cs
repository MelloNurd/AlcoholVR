using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneScript : MonoBehaviour
{
    [SerializeField] private AudioSource _argumentAudioSource;
    [SerializeField] private AudioSource _fireAudioSource;
    [SerializeField] private Animator UI;

    async void Start()
    {
        float tempArgumentVolume = _argumentAudioSource.volume;
        float tempFireVolume = _fireAudioSource.volume;

        _ = Tween.AudioVolume(_argumentAudioSource, 0f, tempArgumentVolume, 2f);
        _ = Tween.AudioVolume(_fireAudioSource, 0f, tempFireVolume, 2f);

        await UniTask.Delay(2000);

        await CloseEyesAsync(.15f);

        await UniTask.Delay(800);

        await OpenEyesAsync(0.3f);

        await UniTask.Delay(100);

        await CloseEyesAsync(0.15f);

        await UniTask.Delay(4000);

        _ = Tween.AudioVolume(_argumentAudioSource, 0f, 8f);
        _ = Tween.AudioVolume(_fireAudioSource, 0f, 8f);

        await UniTask.Delay(8000);

        LoadSceneByName("EndScene");
    }

    public async UniTask CloseEyesAsync(float speed = 1f)
    {
        UI.speed = speed;
        UI.SetTrigger("BlinkClose");

        await UniTask.WaitForEndOfFrame();

        // Wait until the blink animation actually starts playing
        while (!UI.GetCurrentAnimatorStateInfo(0).IsName("BlinkClose"))
        {
            await UniTask.Yield();
        }

        // Now wait for the blink animation to complete
        while (UI.GetCurrentAnimatorStateInfo(0).IsName("BlinkClose") &&
               UI.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            await UniTask.Yield();
        }
    }

    public async UniTask OpenEyesAsync(float speed = 1f)
    {
        UI.speed = speed;
        UI.SetTrigger("BlinkOpen");

        await UniTask.WaitForEndOfFrame();

        // Wait until the blink animation actually starts playing
        while (!UI.GetCurrentAnimatorStateInfo(0).IsName("BlinkOpen"))
        {
            await UniTask.Yield();
        }

        // Now wait for the blink animation to complete
        while (UI.GetCurrentAnimatorStateInfo(0).IsName("BlinkOpen") &&
               UI.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            await UniTask.Yield();
        }
    }

    public void LoadSceneByName(string name)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name);
        if (asyncLoad == null)
        {
            Debug.LogError("Failed to load scene with name: " + name);
        }
    }
}
