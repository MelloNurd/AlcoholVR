using System.Collections.Generic;
using UnityEngine;

public class ItemSounds : MonoBehaviour
{
    public AudioClip _impactSound;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_rb || !_impactSound) return;

        float impactVelocity = collision.relativeVelocity.magnitude;
        if (impactVelocity < 0.35f) return;

        Debug.Log($"[{gameObject.name}] Playing impact sound with velocity {impactVelocity}", this);

        SoundManager.PlaySoundAtPoint(_impactSound, transform.position, impactVelocity * 0.1f);
    }  
}
