using UnityEngine;

public class SideWindowMovingObject : MonoBehaviour
{
    private RectTransform rectTransform;

    private float speed;
    private float direction;
    private float destroyX;

    public void Setup(
        float moveSpeed,
        bool moveRight,
        float destroyPositionX)
    {
        rectTransform = GetComponent<RectTransform>();

        speed = moveSpeed;

        direction = moveRight ? 1f : -1f;

        destroyX = destroyPositionX;
    }

    private void Update()
    {
        if (rectTransform == null)
            return;

        // 좌/우로 이동
        rectTransform.anchoredPosition +=
            Vector2.right *
            direction *
            speed *
            Time.deltaTime;

        // 오른쪽으로 이동하는 경우
        if (direction > 0f)
        {
            if (rectTransform.anchoredPosition.x >= destroyX)
            {
                Destroy(gameObject);
            }
        }
        // 왼쪽으로 이동하는 경우
        else
        {
            if (rectTransform.anchoredPosition.x <= destroyX)
            {
                Destroy(gameObject);
            }
        }
    }
}