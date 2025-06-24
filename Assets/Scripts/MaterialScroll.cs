using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class MaterialScroll : MonoBehaviour
{
    [field: SerializeField] public bool ScrollingActive { get; set; } = true;
    [field: SerializeField] public Vector2 ScrollSpeed { get; set; } = new Vector2(0.1f, 0.1f);

    private Material _material;

    private void Awake()
    {
        _material = GetComponent<Renderer>().material;
    }

    void FixedUpdate()
    {
        if (!ScrollingActive) return;

        _material.mainTextureOffset += ScrollSpeed * Time.fixedDeltaTime;
    }

    public void TweenScrollSpeed(Vector2 target, float duration, Ease ease = Ease.Default) => TweenScrollSpeedAsync(target, duration, ease).Forget();
    public async UniTask TweenScrollSpeedAsync(Vector2 target, float duration, Ease ease = Ease.Default)
    {
        await PrimeTween.Tween.Custom(ScrollSpeed, target, duration, v => ScrollSpeed = v, ease);
    }

    public void Play()
    {
        ScrollingActive = true;
    }

    public void Pause()
    {
        ScrollingActive = false;
    }

    public void SetScrollSpeedX(float x)
    {
        ScrollSpeed = new Vector2(x, ScrollSpeed.y);
    }

    public void SetScrollSpeedY(float y)
    {
        ScrollSpeed = new Vector2(ScrollSpeed.x, y);
    }

    public void SetScrollSpeed(float x, float y) => SetScrollSpeed(new Vector2(x, y));
    public void SetScrollSpeed(Vector2 speed)
    {
        ScrollSpeed = speed;
    }
}
