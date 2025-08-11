using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    [SerializeField] private float blinkSpeed = 1f; // This only applies to the beginning of the scene

    Animator UI;
    public AnimationClip blinkAnimation;

    public void Start()
    {
        UI = GetComponent<Animator>();
        if (UI == null)
        {
            Debug.Log("Animator component not found on Loading object.");
            return;
        }
        else
        {
            UI.speed = blinkSpeed;
        }
    }

    public void TransitionSceneByName(string sceneName) => TransitionSceneByName(sceneName, 1);
    public async void TransitionSceneByName(string sceneName, float speed)
    {
        await CloseEyesAsync(speed);

        LoadSceneByName(sceneName);
    }

    public void TransitionSceneById(int sceneId) => TransitionSceneById(sceneId, 1);
    public async void TransitionSceneById(int sceneId, float speed)
    {
        await CloseEyesAsync(speed);

        LoadSceneById(sceneId);
    }

    public void LoadSceneByName(string name)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name);
        if (asyncLoad == null)
        {
            Debug.LogError("Failed to load scene with name: " + name);
        }
    }
    public void LoadSceneById(int sceneId)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneId);
        if (asyncLoad == null)
        {
            Debug.LogError("Failed to load scene with ID: " + sceneId);
        }
    }

    public void CloseEyes(float speed = 1f) => CloseEyesAsync(speed).Forget();
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

    public void OpenEyes(float speed = 1f) => OpenEyesAsync(speed).Forget();
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
}
