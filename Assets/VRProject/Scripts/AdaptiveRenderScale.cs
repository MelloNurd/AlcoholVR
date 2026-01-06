using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AdaptiveRenderScale : MonoBehaviour
{
    public static AdaptiveRenderScale Instance;

    [Header("Render Scale Limits")]
    [Range(0.5f, 1.2f)]
    public float minScale = 0.7f;
    [Range(0.5f, 1.2f)]
    public float maxScale = 1.0f;

    [Header("Frame Timing Target")]
    [Tooltip("Target frame time in milliseconds (72Hz ≈ 13.8, 90Hz ≈ 11.1)")]
    public float targetFrameTimeMs = 13.8f;
    public float toleranceMs = 1.0f;

    [Header("Adjustment")]
    [Tooltip("How much render scale changes per adjustment step")]
    public float adjustSpeed = 0.05f;
    [Tooltip("How often (seconds) to evaluate performance")]
    public float checkInterval = 0.5f;

    [Header("Smoothing")]
    [Range(0.01f, 1f)]
    public float smoothing = 0.1f;

    UniversalRenderPipelineAsset urp;
    float timer;
    float smoothedFrameTimeMs;

    void Awake()
    {
        // Singleton protection
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

        if (urp == null)
        {
            Debug.LogError("AdaptiveRenderScale: URP asset not found. Disabling.");
            enabled = false;
            return;
        }

        // Clamp initial value (Quest sometimes boots with odd defaults)
        urp.renderScale = Mathf.Clamp(urp.renderScale, minScale, maxScale);
    }

    void Update()
    {
        // Smooth frame time to avoid reacting to spikes
        smoothedFrameTimeMs = Mathf.Lerp(
            smoothedFrameTimeMs,
            Time.unscaledDeltaTime * 1000f,
            smoothing
        );

        timer += Time.unscaledDeltaTime;
        if (timer < checkInterval)
            return;

        timer = 0f;

        // If reprojection / SpaceWarp kicked in, don't overreact
        if (smoothedFrameTimeMs > targetFrameTimeMs * 1.8f)
            return;

        float currentScale = urp.renderScale;
        float newScale = currentScale;

        if (smoothedFrameTimeMs > targetFrameTimeMs + toleranceMs)
            newScale -= adjustSpeed;
        else if (smoothedFrameTimeMs < targetFrameTimeMs - toleranceMs)
            newScale += adjustSpeed;

        newScale = Mathf.Clamp(newScale, minScale, maxScale);

        // Avoid tiny oscillations
        if (Mathf.Abs(newScale - currentScale) > 0.01f)
            urp.renderScale = newScale;
    }
}
