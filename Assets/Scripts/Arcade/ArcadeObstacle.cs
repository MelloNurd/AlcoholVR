using UnityEngine;

public class ArcadeObstacle : MonoBehaviour
{
    private Arcade arcade;

    private void Start()
    {
        arcade = Arcade.Instance;
    }

    void Update()
    {
        transform.position += Vector3.left * arcade.GameSpeed * Time.deltaTime * arcade.arcadeBackgroundObj.transform.localScale.x;
        if(transform.position.x < arcade.arcadeGameCamera.transform.position.x - arcade.arcadeGameCamera.orthographicSize - 1) // optimize this
        {
            Destroy(gameObject);
        }
    }
}
