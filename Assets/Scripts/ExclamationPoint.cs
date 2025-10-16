using System.Runtime.CompilerServices;
using UnityEngine;

public class ExclamationPoint : MonoBehaviour
{
    private Vector3 _initialPosition;

    [SerializeField] private float _bounceHeight = 0.1f;
    [SerializeField] private float _bounceSpeed = 3f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _initialPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf)
        {
            transform.localPosition = _initialPosition + Vector3.up * Mathf.Sin(Time.time * _bounceSpeed) * _bounceHeight;
        }
    }
}
