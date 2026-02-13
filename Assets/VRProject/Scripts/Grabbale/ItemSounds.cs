using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ItemSounds : MonoBehaviour
{
    public AudioClip _impactSound;
    public float _pitchVariation = 0.1f;
    public float _basePitch = 1f;

    private Rigidbody _rb;
    
    private bool _onCooldown = false;
    private const int CooldownTimeMs = 100;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_rb || !_impactSound) return;
        if (_onCooldown) return;

        float impactVelocity = collision.relativeVelocity.magnitude;
        if (impactVelocity < 0.35f) return;

        float pitch = CalculatePitch(impactVelocity);
        SoundManager.PlaySoundAtPoint(_impactSound, transform.position, impactVelocity * 0.1f, pitch);

        RunCooldown();
    }

    private float CalculatePitch(float velocity)
    {
        // Base pitch adjusted by velocity (higher velocity = slightly higher pitch)
        float velocityPitch = _basePitch + (velocity * 0.05f);
        
        // Add random variation
        float randomPitch = Random.Range(-_pitchVariation, _pitchVariation);
        
        return velocityPitch + randomPitch;
    }

    private async void RunCooldown()
    {
        _onCooldown = true;
        await UniTask.Delay(CooldownTimeMs);
        _onCooldown = false;
    }

    private void OnValidate()
    {
        if (!_impactSound && TryGetComponent<GrabbableAudioPlayer>(out var audioPlayer))
        {
            _impactSound = audioPlayer.audioClip;
        }
    }
}
