using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerFace : MonoBehaviour
{
    private const float maxDistance = 4f;

    public PlayerFace Instance { get; private set; }

    [SerializeField] private AudioClip _drinkSound;
    private AudioSource _audioSource;

    private Volume _globalVolume; // Assign this in the Inspector or find it at runtime
    private DepthOfField dof;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _audioSource = gameObject.GetOrAdd<AudioSource>();

        _globalVolume = GameObject.Find("Global Volume").GetComponent<Volume>(); // Just doing FindFirstByType was getting the arcade volume...

        if (!_globalVolume.profile.TryGet(out dof))
        {
            dof = _globalVolume.profile.Add<DepthOfField>(true);
        }

        dof.active = false;
    }

    public async void ApplyBlurEffect()
    {
        Debug.Log("Applying blur effect to player face.... dof: " + dof);
        Debug.Log($"dof active: {dof.active}, focusDistance: {dof.focusDistance.value}");
        dof.active = true;
        dof.focusDistance.value = maxDistance;
        Debug.Log($"dof active: {dof.active}, focusDistance: {dof.focusDistance.value}");

        Tween.StopAll(dof.focusDistance);
        await Tween.Custom(1f, 0f, 1f, (float val) => { dof.focusDistance.value = val; }, Ease.OutCirc);
        Debug.Log($"dof active: {dof.active}, focusDistance: {dof.focusDistance.value}");

        Tween.StopAll(dof.focusDistance);
        await Tween.Custom(0f, maxDistance, 1.5f, (float val) => { dof.focusDistance.value = val; }, Ease.InCirc);

        dof.active = false;
        Debug.Log($"dof active: {dof.active}, focusDistance: {dof.focusDistance.value}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out OpenableBottle bottle) && bottle.IsOpen && bottle.IsFull)
        {
            if(Vector3.Dot(bottle.transform.forward, transform.forward) < -0.5f) // Bottle top is facing player's face
            {
                bottle.IsFull = false;
                ApplyBlurEffect();
                GlobalStats.DrinkCount++;
                if(_drinkSound != null) _audioSource.PlayOneShot(_drinkSound);
            }
        }
    }
}
