using Cysharp.Threading.Tasks;
using UnityEngine;

public class CutsceneScript : MonoBehaviour
{
    async void Start()
    {
        PlayerFace.Instance.BlurVision(0.01f);
        await UniTask.Delay(2000);
        Player.Instance.CloseEyes(0.15f);
        await UniTask.Delay(800);
        Player.Instance.OpenEyes(0.3f);
        await UniTask.Delay(100);
        Player.Instance.CloseEyes(0.15f);

        await UniTask.Delay(12000);

        Player.Instance.loading.LoadSceneByName("EndScene");
    }
}
