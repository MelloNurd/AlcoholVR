using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerFace : MonoBehaviour
{
    public static PlayerFace Instance { get; private set; }

    [SerializeField] private AudioClip _drinkSound;
    private AudioSource _audioSource;

    private Volume _globalVolume; // Assign this in the Inspector or find it at runtime
    private DepthOfField dof;

    Tween blurTween;

    void Awake()
    {
        Instance = this;

        _audioSource = gameObject.GetOrAdd<AudioSource>();

        _globalVolume = GameObject.Find("Global Volume").GetComponent<Volume>(); // Just doing FindFirstByType was getting the arcade volume...

        Debug.Log($"Found Global Volume: {_globalVolume != null}");

        if (!_globalVolume.profile.TryGet(out dof))
        {
            dof = _globalVolume.profile.Add<DepthOfField>(true);
        }

        dof.active = false;
    }

    public void BlurVision(float duration = 1f) => BlurVisionAsync(duration).Forget();
    public async UniTask BlurVisionAsync(float duration = 1f)
    {
        dof.focalLength.value = 1;
        dof.active = true;

        blurTween.Stop();
        blurTween = Tween.Custom(1f, 30f, duration, (float val) => { dof.focalLength.value = val; }, Ease.InSine);
        await blurTween;
    }

    public void ClearBlur(float duration = 1f) => ClearBlurAsync(duration).Forget();
    public async UniTask ClearBlurAsync(float duration = 1f)
    {
        dof.focusDistance.value = 30f;

        blurTween.Stop();
        blurTween = Tween.Custom(30f, 1f, duration, (float val) => { dof.focalLength.value = val; }, Ease.OutSine);
        await blurTween;

        dof.active = false;
    }

    public async void ApplyBlurPulse()
    {
        await BlurVisionAsync();
        await ClearBlurAsync(1.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out OpenableBottle bottle) && bottle.IsOpen && bottle.IsFull)
        {
            if(Vector3.Dot(bottle.transform.forward, transform.forward) < -0.5f) // Bottle top is facing player's face
            {
                bottle.IsFull = false;
                if(_drinkSound != null) _audioSource.PlayOneShot(_drinkSound);

                if(bottle.IsAlcoholic)
                {
                    ApplyBlurPulse();
                    GlobalStats.DrinkCount++;
                }
            }
        }
    }
}
