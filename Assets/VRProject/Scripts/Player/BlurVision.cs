using Cysharp.Threading.Tasks;
using PrimeTween;
using Unity.Hierarchy;
using UnityEngine;

public class BlurVision : MonoBehaviour
{
    private static CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void BlurPlayerVision() // non-static, non-async wrapper method for UnityEvents
    {
        BlurPlayerVision(0.25f, 1.5f, 0.25f);
    }

    public async static void BlurPlayerVision(float fadeInTime = 0.25f, float duration = 1.5f, float fadeOutTime = 0.25f)
    {
        await Tween.Alpha(_canvasGroup, startValue: 0f, endValue: 1f, duration: fadeInTime);

        await UniTask.Delay(duration.ToMS());

        await Tween.Alpha(_canvasGroup, startValue: 1f, endValue: 0f, duration: fadeOutTime);
    }
}
