using Unity.VisualScripting;
using UnityEngine;

public class ArcadeDetail : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    private Arcade arcade;

    private float speed;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        arcade = Arcade.Instance;
    }

    public void Initialize(Sprite sprite, float alpha, int order, Vector3 scale, float speed)
    {
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = order;
        spriteRenderer.color = spriteRenderer.color.WithAlpha(alpha);
        transform.localScale = scale;
        this.speed = speed;
    }

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime * arcade.arcadeBackgroundObj.transform.localScale.x;
        if (transform.position.x < arcade.arcadeGameCamera.transform.position.x - arcade.arcadeGameCamera.orthographicSize - 4) // optimize this
        {
            Destroy(gameObject);
        }
    }
}
