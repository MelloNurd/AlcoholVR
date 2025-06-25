using UnityEngine;
using UnityEngine.Events;

public class ArcadePlayer : MonoBehaviour
{
    [SerializeField] private Arcade arcade;
    private float speed = 8f;

    private float offset;

    private int direction = 1;

    public UnityEvent OnPlayerDeath = new();

    private void Start()
    {
        offset = arcade.arcadeGameCamera.orthographicSize - transform.localScale.y * 0.5f;
    }

    void Update()
    {
        transform.position += Vector3.up * speed * direction * Time.deltaTime;

        // Clamp to screen bounds
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, arcade.arcadeGameCamera.transform.position.y - offset, arcade.arcadeGameCamera.transform.position.y + offset);
        transform.position = pos;
    }

    public void ChangeDirection()
    {
        direction *= -1;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        speed = 0f;
        OnPlayerDeath?.Invoke();
    }
}