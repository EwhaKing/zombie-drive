using UnityEngine;
using System.Collections;

public class UISlideHider : MonoBehaviour
{
    public RectTransform targetUI; // 사라질 하단 UI (BottomUIPrefab)
    public float slideDistance = 300f; // 얼마나 아래로 이동할지
    public float duration = 0.3f; // 애니메이션 시간

    private Vector2 originalPosition;
    private bool isHidden = false;
    private bool initialized = false;

    void EnsureInitialized()
    {
        if (!initialized)
        {
            originalPosition = targetUI.anchoredPosition;
            initialized = true;
        }
    }

    public void HideUI()
    {
        EnsureInitialized();
        if (isHidden) return;
        isHidden = true;
        StopAllCoroutines();
        StartCoroutine(SlideTo(originalPosition + new Vector2(0, -slideDistance)));
    }

    public void ShowUI()
    {
        EnsureInitialized();
        if (!isHidden) return;
        isHidden = false;
        StopAllCoroutines();
        StartCoroutine(SlideTo(originalPosition));
    }

    IEnumerator SlideTo(Vector2 target)
    {
        Vector2 start = targetUI.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            targetUI.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        targetUI.anchoredPosition = target;
    }
}