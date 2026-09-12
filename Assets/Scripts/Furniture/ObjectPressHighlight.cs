using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ObjectPressHighlight :
    MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [SerializeField] private GameObject highlightObject;

    [SerializeField] private UnityEvent onClick;

    private void Awake()
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }
}