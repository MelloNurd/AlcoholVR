using UnityEngine;

public class CinematicCamera : MonoBehaviour
{
    [SerializeField] private Vector3 _moveDirection;
    [SerializeField] private float _moveSpeed = 1f;

    private Camera _thisCamera;

    private void Awake()
    {
        _thisCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        transform.position += _moveDirection.normalized * _moveSpeed * Time.deltaTime;

//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#endif
    }
}
