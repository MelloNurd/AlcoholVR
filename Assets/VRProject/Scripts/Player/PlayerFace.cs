using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PlayerFace : MonoBehaviour
{
    public static PlayerFace Instance { get; private set; }

    [SerializeField] private AudioClip _drinkSound;
    private AudioSource _audioSource;

    private Volume _globalVolume; // Assign this in the Inspector or find it at runtime
    private DepthOfField dof;

    [SerializeField] CanvasGroup drinkCanvas;
    [SerializeField] TextMeshProUGUI drinkText;
    [SerializeField] Image fillImage;
    [SerializeField] Loading eyelids;

    // List of 5 color to change the UI image color to based on how many drinks the player has had
    [SerializeField] private Color[] drinkColors;

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

#if UNITY_EDITOR
    private void Update()
    {
        if(Keyboard.current.gKey.wasPressedThisFrame)
        {
            BlurVision.BlurPlayerVision();
        }
    }
#endif

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
                    BlurVision.BlurPlayerVision();
                    GlobalStats.DrinkCount++;
                    drinkText.text = $"{GlobalStats.DrinkCount}";
                    fillImage.color = drinkColors[GlobalStats.DrinkCount - 1];
                    fillImage.fillAmount = (float)GlobalStats.DrinkCount / GlobalStats.maxDrinks;
                    DisplayDrinks(0.25f, 1, 0.25f);
                    if(GlobalStats.DrinkCount >= GlobalStats.maxDrinks)
                    {
                        GlobalStats.blackedOut = true;
                        eyelids.LoadSceneByName("EndScene");
                    }
                }
            }
        }
    }

    private async void DisplayDrinks(float fadeInTime, float duration, float fadeOutTime)
    {
        await Tween.Alpha(drinkCanvas, startValue: 0f, endValue: 1f, duration: fadeInTime);

        await UniTask.Delay(duration.ToMS());

        await Tween.Alpha(drinkCanvas, startValue: 1f, endValue: 0f, duration: fadeOutTime);
    }

    public void PlayDrinkSound()
    {
        if(_drinkSound != null) _audioSource.PlayOneShot(_drinkSound);
    }
}
