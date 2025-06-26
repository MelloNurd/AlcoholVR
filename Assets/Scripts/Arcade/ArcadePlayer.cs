using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class ArcadePlayer : MonoBehaviour
{
    [SerializeField] private Arcade arcade;

    public bool isInvincible = false;
    public bool shouldMove = false;

    public UnityEvent OnPlayerDeath = new();

    private int direction = 1;
    private float _offset;
    public float speed = 8f;

    private void Start()
    {
        _offset = arcade.arcadeGameCamera.orthographicSize - transform.localScale.y * 0.5f;
    }

    void Update()
    {
        if (!shouldMove) return;

        transform.position += Vector3.up * speed * Mathf.Clamp01(arcade.GameSpeed) * direction * Time.deltaTime;

        // Clamp to screen bounds
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, arcade.arcadeGameCamera.transform.position.y - _offset, arcade.arcadeGameCamera.transform.position.y + _offset);
        transform.position = pos;
    }

    public void ChangeDirection()
    {
        direction *= -1;
        SetDirection(direction);
    }

    public void SetDirection(float direction)
    {
        this.direction = Math.Sign(direction);
        transform.GetChild(0).rotation = Quaternion.Euler(0, 0, this.direction * 10f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(isInvincible) return;

        transform.Find("TrailParticles").GetComponent<ParticleSystem>().Stop();
        transform.Find("cow").gameObject.SetActive(false);
        transform.Find("StoppedCow").GetComponent<SpriteRenderer>().enabled = true;
        speed = 0f;
        OnPlayerDeath?.Invoke();
    }
}