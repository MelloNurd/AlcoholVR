using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    Animator UI;
    public AnimationClip blinkAnimation;

    public void Start()
    {
        UI = GetComponent<Animator>();
    }

    public void LoadScene(int sceneId)
    {
        StartCoroutine(LoadSceneAsync(sceneId));
    }

    IEnumerator LoadSceneAsync(int sceneId)
    {
        CloseEyes();

        // Wait for animator to enter the blink state
        yield return new WaitForEndOfFrame();

        // Wait until the blink animation actually starts playing
        while (!UI.GetCurrentAnimatorStateInfo(0).IsName("BlinkClose"))
        {
            yield return null;
        }

        // Now wait for the blink animation to complete
        while (UI.GetCurrentAnimatorStateInfo(0).IsName("BlinkClose") &&
               UI.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        UI.SetTrigger("Load");

        // Start loading the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneId);
        if (asyncLoad == null)
        {
            Debug.LogError("Failed to load scene with ID: " + sceneId);
            yield break;
        }
    }

    public void CloseEyes(float speed = 1f)
    {
        UI.speed = speed;
        UI.SetTrigger("BlinkClose");
    }

    public void OpenEyes(float speed = 1f)
    {
        UI.speed = speed;
        UI.SetTrigger("BlinkOpen");
    }
}
