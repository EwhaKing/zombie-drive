using UnityEngine;

// 왼쪽 또는 오른쪽 창문의 UI 배경 두 장을
// 반복해서 이어 움직이게 하는 스크립트
public class SideWindowScroller : MonoBehaviour
{
    public enum ScrollDirection
    {
        Left,
        Right
    }

    [Header("반복 배경 이미지 2개")]
    [SerializeField] private RectTransform backgroundA;
    [SerializeField] private RectTransform backgroundB;

    [Header("이동 설정")]
    [SerializeField] private ScrollDirection direction = ScrollDirection.Left;
    [SerializeField] private float moveSpeed = 150f;

    [Header("시작할 때 두 배경 자동 배치")]
    [SerializeField] private bool arrangeOnStart = true;

    // 배경 이미지 한 장의 실제 너비
    private float backgroundWidth;

    private void Start()
    {
        // Inspector 연결이 빠졌는지 확인
        if (backgroundA == null || backgroundB == null)
        {
            Debug.LogError(
                gameObject.name +
                ": Background A 또는 Background B가 연결되지 않았습니다."
            );

            enabled = false;
            return;
        }

        // UI의 Rect 크기가 계산된 뒤 너비를 읽음
        Canvas.ForceUpdateCanvases();

        backgroundWidth = backgroundA.rect.width;

        if (backgroundWidth <= 0f)
        {
            Debug.LogError(
                gameObject.name +
                ": 배경 이미지의 Width가 0입니다."
            );

            enabled = false;
            return;
        }

        // 두 배경의 너비가 다르면 반복할 때 틈이 생길 수 있음
        if (Mathf.Abs(backgroundA.rect.width -
                      backgroundB.rect.width) > 0.1f)
        {
            Debug.LogWarning(
                gameObject.name +
                ": Background A와 B의 Width가 서로 다릅니다."
            );
        }

        if (arrangeOnStart)
        {
            ArrangeBackgrounds();
        }
    }

    private void Update()
    {
        float moveDirection =
            direction == ScrollDirection.Left ? -1f : 1f;

        float moveAmount =
            moveDirection * moveSpeed * Time.deltaTime;

        MoveBackground(backgroundA, moveAmount);
        MoveBackground(backgroundB, moveAmount);

        if (direction == ScrollDirection.Left)
        {
            RecycleMovingLeft(backgroundA, backgroundB);
            RecycleMovingLeft(backgroundB, backgroundA);
        }
        else
        {
            RecycleMovingRight(backgroundA, backgroundB);
            RecycleMovingRight(backgroundB, backgroundA);
        }
    }

    // 시작할 때 두 배경을 방향에 맞게 나란히 배치
    private void ArrangeBackgrounds()
    {
        float yPosition = backgroundA.anchoredPosition.y;

        backgroundA.anchoredPosition =
            new Vector2(0f, yPosition);

        if (direction == ScrollDirection.Left)
        {
            // 왼쪽 이동:
            // B를 A의 오른쪽에 배치
            backgroundB.anchoredPosition =
                new Vector2(backgroundWidth, yPosition);
        }
        else
        {
            // 오른쪽 이동:
            // B를 A의 왼쪽에 배치
            backgroundB.anchoredPosition =
                new Vector2(-backgroundWidth, yPosition);
        }
    }

    private void MoveBackground(
        RectTransform background,
        float moveAmount)
    {
        background.anchoredPosition +=
            Vector2.right * moveAmount;
    }

    // 왼쪽으로 완전히 빠진 배경을
    // 다른 배경의 오른쪽으로 이동
    private void RecycleMovingLeft(
        RectTransform target,
        RectTransform other)
    {
        if (target.anchoredPosition.x <= -backgroundWidth)
        {
            target.anchoredPosition = new Vector2(
                other.anchoredPosition.x + backgroundWidth,
                target.anchoredPosition.y
            );
        }
    }

    // 오른쪽으로 완전히 빠진 배경을
    // 다른 배경의 왼쪽으로 이동
    private void RecycleMovingRight(
        RectTransform target,
        RectTransform other)
    {
        if (target.anchoredPosition.x >= backgroundWidth)
        {
            target.anchoredPosition = new Vector2(
                other.anchoredPosition.x - backgroundWidth,
                target.anchoredPosition.y
            );
        }
    }
}