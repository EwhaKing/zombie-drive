using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableBolt : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 startPosition;

    public bool IsUsed { get; private set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // -----------------------------
    // 드래그 시작
    // -----------------------------
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsUsed)
            return;

        startPosition = rectTransform.anchoredPosition;

        canvasGroup.blocksRaycasts = false;

        transform.SetAsLastSibling();
    }

    // -----------------------------
    // 드래그 중
    // -----------------------------
    public void OnDrag(PointerEventData eventData)
    {
        if (IsUsed)
            return;

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    // -----------------------------
    // 드래그 종료
    // -----------------------------
    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsUsed)
            return;

        canvasGroup.blocksRaycasts = true;

        // 빈 구멍에 제대로 들어가지 않았다면
        // 원래 자리로 되돌아감
        rectTransform.anchoredPosition = startPosition;
    }

    // -----------------------------
    // 구멍에 정상적으로 들어감
    // -----------------------------
    public void UseBolt()
    {
        IsUsed = true;

        canvasGroup.blocksRaycasts = true;

        gameObject.SetActive(false);
    }
}