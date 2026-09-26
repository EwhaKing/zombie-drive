using UnityEngine;

public class FrontWindowMovingObject : MonoBehaviour
{
    private RectTransform rectTransform;

    private Vector2 startPosition;
    private Vector2 endPosition;

    private float startScale;
    private float endScale;

    private float duration;
    private float elapsedTime;

    private AnimationCurve movementCurve;

    public void Setup(
        Vector2 start,
        Vector2 end,
        float minScale,
        float maxScale,
        float moveDuration,
        AnimationCurve curve)
    {
        rectTransform =
            GetComponent<RectTransform>();

        startPosition = start;
        endPosition = end;

        startScale = minScale;
        endScale = maxScale;

        duration = moveDuration;

        movementCurve = curve;

        elapsedTime = 0f;

        rectTransform.anchoredPosition =
            startPosition;

        rectTransform.localScale =
            Vector3.one * startScale;
    }

    private void Update()
    {
        if (rectTransform == null)
            return;

        elapsedTime += Time.deltaTime;

        float progress =
            Mathf.Clamp01(
                elapsedTime / duration
            );

        float curvedProgress =
            movementCurve != null
                ? movementCurve.Evaluate(progress)
                : progress;

        // 멀리 → 가까이
        rectTransform.anchoredPosition =
            Vector2.Lerp(
                startPosition,
                endPosition,
                curvedProgress
            );

        // 작게 → 크게
        float scale =
            Mathf.Lerp(
                startScale,
                endScale,
                curvedProgress
            );

        rectTransform.localScale =
            Vector3.one * scale;

        if (progress >= 1f)
        {
            Destroy(gameObject);
        }
    }
}