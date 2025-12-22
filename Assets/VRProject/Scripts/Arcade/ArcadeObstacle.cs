using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class ArcadeObstacle : MonoBehaviour
{
    private Arcade arcade;
    public static UnityEvent OnPlayerScore = new();

    private void Start()
    {
        arcade = Arcade.Instance;
    }

    void Update()
    {
        if(arcade.State != Arcade.GameState.Playing) return;

        transform.position += Vector3.left * arcade.GameSpeed * Time.deltaTime * arcade.arcadeBackgroundObj.transform.localScale.x;
        if(transform.position.x < arcade.arcadeGameCamera.transform.position.x - arcade.arcadeGameCamera.orthographicSize - 1) // optimize this
        {
            arcade.obstacles.Remove(this);
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
    }

    private async void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("ArcadePlayer")) return;

        await UniTask.Delay(250);
        if(arcade.State == Arcade.GameState.Playing)
        {
            OnPlayerScore?.Invoke();
            arcade.UpdateScore(arcade.Score + 1);
        }
    }
}
