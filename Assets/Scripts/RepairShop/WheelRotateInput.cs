using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WheelRotateInput : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [SerializeField]
    private RepairShopGameManager gameManager;

    [SerializeField]
    private RectTransform centerRect;

    private RectTransform rotateTarget;
    private Graphic inputGraphic;

    private bool inputEnabled;
    private bool isHolding;

    private float lastAngle;

    private void Awake()
    {
        inputGraphic = GetComponent<Graphic>();
    }

    public void SetRotationTarget(RectTransform target)
    {
        rotateTarget = target;
    }

    public void SetInputEnabled(bool value)
    {
        inputEnabled = value;
        isHolding = false;

        if (inputGraphic != null)
        {
            inputGraphic.raycastTarget = value;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!inputEnabled)
            return;

        if (TryGetPointerAngle(eventData.position,
                eventData.pressEventCamera,
                out float angle))
        {
            lastAngle = angle;
            isHolding = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!inputEnabled || !isHolding)
            return;

        if (!TryGetPointerAngle(eventData.position,
                eventData.pressEventCamera,
                out float currentAngle))
            return;

        float deltaAngle = Mathf.DeltaAngle(lastAngle, currentAngle);

        // 갑자기 포인터가 튀어서 한 번에 지나치게 많은
        // 진행도가 들어가는 것을 방지
        deltaAngle = Mathf.Clamp(deltaAngle, -45f, 45f);

        lastAngle = currentAngle;

        if (rotateTarget != null)
        {
            rotateTarget.Rotate(0f, 0f, deltaAngle);
        }

        if (gameManager != null)
        {
            gameManager.OnWheelDragged(deltaAngle);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
    }

    private bool TryGetPointerAngle(
        Vector2 screenPosition,
        Camera eventCamera,
        out float angle)
    {
        angle = 0f;

        if (centerRect == null)
            return false;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            centerRect,
            screenPosition,
            eventCamera,
            out Vector2 localPoint
        );

        // 바퀴 정확히 가운데에서 드래그하면
        // 각도 계산이 불안정하기 때문에 무시
        if (localPoint.magnitude < 40f)
            return false;

        angle = Mathf.Atan2(localPoint.y, localPoint.x)
                * Mathf.Rad2Deg;

        return true;
    }
}