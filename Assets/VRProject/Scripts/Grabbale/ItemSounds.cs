using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ItemSounds : MonoBehaviour
{
    public AudioClip _impactSound;

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

        SoundManager.PlaySoundAtPoint(_impactSound, transform.position, impactVelocity * 0.1f);

        RunCooldown();
    } 

    private async void RunCooldown()
    {
        _onCooldown = true;
        await UniTask.Delay(CooldownTimeMs);
        _onCooldown = false;
    }
}
