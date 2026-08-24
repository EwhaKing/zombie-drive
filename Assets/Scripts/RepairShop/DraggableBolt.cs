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

    private Vector2 originalPosition;
    private bool positionSaved;

    private BoltSlot targetSlot;

    private void CacheComponents()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (!positionSaved && rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition;
            positionSaved = true;
        }
    }

    public void Bind(BoltSlot slot)
    {
        CacheComponents();

        targetSlot = slot;

        ResetPosition();
    }

    public void ResetPosition()
    {
        CacheComponents();

        if (rectTransform != null && positionSaved)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        CacheComponents();

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null || canvas == null)
            return;

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (targetSlot == null)
        {
            ResetPosition();
            return;
        }

        bool inside =
            RectTransformUtility.RectangleContainsScreenPoint(
                targetSlot.DropArea,
                eventData.position,
                eventData.pressEventCamera
            );

        if (inside)
        {
            bool attached =
                targetSlot.AttachFromDrag();

            if (attached)
            {
                gameObject.SetActive(false);
                return;
            }
        }

        ResetPosition();
    }
}