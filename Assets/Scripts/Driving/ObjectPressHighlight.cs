using UnityEngine;
using UnityEngine.EventSystems;

// 마우스나 손가락으로 누르고 있는 동안에만
// 노란 테두리 이미지를 표시한다.
public class ObjectPressHighlight
    : MonoBehaviour,
      IPointerDownHandler,
      IPointerUpHandler,
      IPointerExitHandler
{
    [Header("평소 이미지")]
    [SerializeField] private GameObject normalObject;

    [Header("누르는 동안 표시할 노란 테두리")]
    [SerializeField] private GameObject highlightObject;

    private void Awake()
    {
        // 시작할 때는 일반 이미지만 표시
        if (normalObject != null)
        {
            normalObject.SetActive(true);
        }

        if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }
    }

    // 마우스 버튼 또는 손가락을 누른 순간
    public void OnPointerDown(PointerEventData eventData)
    {
        SetPressedState(true);
    }

    // 마우스 버튼 또는 손가락을 뗀 순간
    public void OnPointerUp(PointerEventData eventData)
    {
        SetPressedState(false);
    }

    // 누른 상태로 오브젝트 바깥으로 벗어났을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        SetPressedState(false);
    }

    private void SetPressedState(bool isPressed)
    {
        if (normalObject != null)
        {
            normalObject.SetActive(!isPressed);
        }

        if (highlightObject != null)
        {
            highlightObject.SetActive(isPressed);
        }
    }
}
