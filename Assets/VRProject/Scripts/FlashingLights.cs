using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class FlashingLights : MonoBehaviour
{
    public Light[] lights;

    public float FlashInterval = 1f;
    public float FlashOffset = 0f;

    private CancellationTokenSource _cancelToken;

    private void Awake()
    {
        lights = GetComponentsInChildren<Light>();
    }

    public void StartLights()
    {
        FlashLights();
    }

    public void StopLights()
    {
        // Cancel the flashing task
        _cancelToken?.Cancel();
        _cancelToken?.Dispose();
        _cancelToken = null;

        // Turn off all lights
        foreach (Light light in lights)
        {
            light.enabled = false;
        }
    }

    private async void FlashLights()
    {
        _cancelToken = new CancellationTokenSource();

        // Wait for the offset time
        if (FlashOffset > 0f)
            await Task.Delay((int)(FlashOffset * 1000), _cancelToken.Token);

        bool isFlashing = false;

        while (!_cancelToken.IsCancellationRequested)
        {
            if (lights.Length > 0)
            {
                isFlashing = !isFlashing;
                foreach (Light light in lights)
                {
                    light.enabled = isFlashing;
                }
            }

            await Task.Delay((int)(FlashInterval * 1000), _cancelToken.Token);
        }
    }

    private void OnDisable()
    {
        // Clean up when disabled
        _cancelToken?.Cancel();
        _cancelToken?.Dispose();
    }

    private void OnDestroy()
    {
        // Additional cleanup
        _cancelToken?.Cancel();
        _cancelToken?.Dispose();
    }
}
