using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    [Header("옆으로 이어 붙인 배경 이미지 2개")]
    [SerializeField] private RectTransform backgroundA;
    [SerializeField] private RectTransform backgroundB;

    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 150f;
    [SerializeField] private float backgroundWidth = 1920f;

    private void Update()
    {
        // 두 배경을 매 프레임 왼쪽으로 이동시킨다.
        MoveBackground(backgroundA);
        MoveBackground(backgroundB);

        // 화면 왼쪽으로 완전히 사라진 배경을 다른 배경 오른쪽으로 옮긴다.
        RepositionIfNeeded(backgroundA, backgroundB);
        RepositionIfNeeded(backgroundB, backgroundA);
    }

    private void MoveBackground(RectTransform background)
    {
        background.anchoredPosition += Vector2.left * moveSpeed * Time.deltaTime;
    }

    private void RepositionIfNeeded(
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
}