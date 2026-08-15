using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectPressHighlight :
    MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [SerializeField] private GameObject highlightObject;

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
}