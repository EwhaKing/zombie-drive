using UnityEngine;

// 정면 창문의 이미지가
// 먼 곳에서 시작해 아래로 내려오며 점점 커지는 효과
public class FrontWindowPerspectiveScroller : MonoBehaviour
{
    [System.Serializable]
    public class PerspectiveImage
    {
        public RectTransform rectTransform;
        public CanvasGroup canvasGroup;

        [Range(0f, 1f)]
        public float startPhase;
    }

    [Header("반복해서 움직일 이미지")]
    [SerializeField] private PerspectiveImage imageA;
    [SerializeField] private PerspectiveImage imageB;

    [Header("이동 위치")]
    [SerializeField]
    private Vector2 startPosition =
        new Vector2(0f, 100f);

    [SerializeField]
    private Vector2 endPosition =
        new Vector2(0f, -250f);

    [Header("크기 변화")]
    [SerializeField] private float startScale = 0.25f;
    [SerializeField] private float endScale = 1.6f;

    [Header("속도")]
    [SerializeField] private float cycleDuration = 3f;

    [Header("움직임 가속도")]
    [SerializeField]
    private AnimationCurve movementCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private float elapsedTime;

    private void Start()
    {
        if (!IsValid(imageA) || !IsValid(imageB))
        {
            Debug.LogError(
                gameObject.name +
                ": 정면 이동 이미지 또는 CanvasGroup 연결이 빠졌습니다."
            );

            enabled = false;
            return;
        }

        // 두 이미지가 반 주기 차이로 반복되도록 설정
        imageA.startPhase = 0f;
        imageB.startPhase = 0.5f;
    }

    private void Update()
    {
        if (cycleDuration <= 0f)
            return;

        elapsedTime += Time.deltaTime;

        float cycle =
            Mathf.Repeat(elapsedTime / cycleDuration, 1f);

        UpdateImage(imageA, cycle);
        UpdateImage(imageB, cycle);
    }

    private void UpdateImage(
        PerspectiveImage image,
        float cycle)
    {
        float progress =
            Mathf.Repeat(cycle + image.startPhase, 1f);

        float curvedProgress =
            movementCurve.Evaluate(progress);

        // 위쪽 먼 곳에서 아래쪽 가까운 곳으로 이동
        image.rectTransform.anchoredPosition =
            Vector2.Lerp(
                startPosition,
                endPosition,
                curvedProgress
            );

        // 이동하면서 점점 크게 확대
        float currentScale =
            Mathf.Lerp(
                startScale,
                endScale,
                curvedProgress
            );

        image.rectTransform.localScale =
            Vector3.one * currentScale;

        // 처음에는 서서히 나타나고,
        // 끝부분에서는 서서히 사라지게 함
        float fadeIn =
            Mathf.InverseLerp(0f, 0.15f, progress);

        float fadeOut =
            1f - Mathf.InverseLerp(0.8f, 1f, progress);

        image.canvasGroup.alpha =
            Mathf.Clamp01(
                Mathf.Min(fadeIn, fadeOut)
            );
    }

    private bool IsValid(PerspectiveImage image)
    {
        return image != null &&
               image.rectTransform != null &&
               image.canvasGroup != null;
    }
}