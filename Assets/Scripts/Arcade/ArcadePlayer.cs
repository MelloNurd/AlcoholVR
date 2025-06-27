using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class ArcadePlayer : MonoBehaviour
{
    public bool isInvincible = false;
    public bool shouldMove = false;

    public UnityEvent OnPlayerDeath = new();

    private Arcade _arcade;

    private int _direction = 1;
    private float _offset;
    private float _speed = 8f;

    private ParticleSystem _trailParticles;
    private ParticleSystem _stopParticles;

    private void Awake()
    {
        _trailParticles = transform.Find("TrailParticles").GetComponent<ParticleSystem>();
        _stopParticles = transform.Find("StopParticles").GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        _arcade = Arcade.Instance;
        _offset = _arcade.arcadeGameCamera.orthographicSize - transform.localScale.y * 0.5f;
    }

    void Update()
    {
        if (!shouldMove) return;

        transform.position += Vector3.up * _speed * Mathf.Clamp01(_arcade.GameSpeed) * _direction * Time.deltaTime;

        // Clamp to screen bounds
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, _arcade.arcadeGameCamera.transform.position.y - _offset, _arcade.arcadeGameCamera.transform.position.y + _offset);
        transform.position = pos;
    }

    public void ChangeDirection()
    {
        _direction *= -1;
        SetDirection(_direction);
    }

    public void SetDirection(float direction)
    {
        this._direction = Math.Sign(direction);
        transform.GetChild(0).rotation = Quaternion.Euler(0, 0, this._direction * 10f);
    }

    public void RestartPlayer(Vector3 pos)
    {
        transform.position = pos;
        transform.Find("TrailParticles").GetComponent<ParticleSystem>().Stop();
        _trailParticles.Play();
        _speed = 8f;

        transform.Find("cow").gameObject.SetActive(true);
        transform.Find("StoppedCow").GetComponent<SpriteRenderer>().enabled = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(isInvincible) return;

        _trailParticles.Stop();
        _stopParticles.Play();
        transform.Find("cow").gameObject.SetActive(false);
        transform.Find("StoppedCow").GetComponent<SpriteRenderer>().enabled = true;
        _speed = 0f;
        OnPlayerDeath?.Invoke();
    }
}