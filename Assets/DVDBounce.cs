using UnityEngine;
using UnityEngine.UI;

public class DVDBounce : MonoBehaviour
{
    public float speed = 50f; // Pixels per second
    private Vector2 appliedSpeed;
    private RectTransform rectTransform;
    private RectTransform canvasRect;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        appliedSpeed = new Vector2(speed, speed);
    }

    void Update()
    {
        Vector2 position = rectTransform.anchoredPosition;
        position += appliedSpeed * Time.deltaTime;

        Vector2 logoSize = rectTransform.sizeDelta;
        Vector2 canvasSize = canvasRect.sizeDelta;

        // Horizontal bounce
        if (position.x < -canvasSize.x / 2 + logoSize.x / 2 || position.x > canvasSize.x / 2 - logoSize.x / 2)
        {
            appliedSpeed.x *= -1;
            position.x = Mathf.Clamp(position.x, -canvasSize.x / 2 + logoSize.x / 2, canvasSize.x / 2 - logoSize.x / 2);
        }

        // Vertical bounce
        if (position.y < -canvasSize.y / 2 + logoSize.y / 2 || position.y > canvasSize.y / 2 - logoSize.y / 2)
        {
            appliedSpeed.y *= -1;
            position.y = Mathf.Clamp(position.y, -canvasSize.y / 2 + logoSize.y / 2, canvasSize.y / 2 - logoSize.y / 2);
        }

        rectTransform.anchoredPosition = position;
    }
}
