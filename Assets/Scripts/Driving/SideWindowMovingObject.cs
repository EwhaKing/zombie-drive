using UnityEngine;

public class SideWindowMovingObject : MonoBehaviour
{
    [Header("좀비 여부")]
    [SerializeField] private bool isZombie = false;

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

        // 일반 풍경만 연료가 0일 때 정지
        // 좀비는 연료가 없어도 계속 이동
        if (!isZombie &&
            FuelManager.Instance != null &&
            !FuelManager.Instance.HasFuel)
        {
            return;
        }

        // 기존 좌우 이동 코드
        rectTransform.anchoredPosition +=
            Vector2.right *
            direction *
            speed *
            Time.deltaTime;

        // 오른쪽 이동
        if (direction > 0f)
        {
            if (rectTransform.anchoredPosition.x >= destroyX)
            {
                Destroy(gameObject);
            }
        }
        // 왼쪽 이동
        else
        {
            if (rectTransform.anchoredPosition.x <= destroyX)
            {
                Destroy(gameObject);
            }
        }
    }
}