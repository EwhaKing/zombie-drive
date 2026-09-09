using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WheelRemovalMiniGame : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("게이지")]
    [SerializeField] private GameObject gaugeRoot;

    [Header("회전 대상")]
    [SerializeField] private RectTransform tireRect;
    [SerializeField] private GameObject tireObject;

    [Header("남겨둘 은색 틀")]
    [SerializeField] private GameObject rimObject;

    [Header("게이지")]
    [SerializeField] private Image gaugeFill; // Filled / Radial 360 으로 설정

    [Header("회전 설정")]
    [SerializeField] private float requiredRotation = 720f; // 2바퀴
    [SerializeField] private bool allowCounterClockwiseDecrease = false;

    [Header("제거 연출")]
    [SerializeField] private float removeMoveDistance = 180f;
    [SerializeField] private float removeDuration = 0.25f;

    [Header("나사 단계")]
    [SerializeField] private GameObject boltPhaseRoot;
    [SerializeField] private BoltPhaseManager boltPhaseManager;

    private Canvas rootCanvas;
    private Camera uiCamera;

    private float accumulatedRotation = 0f;
    private float previousAngle = 0f;
    private bool isDragging = false;
    private bool isCompleted = false;

    private Vector2 startAnchoredPos;
    private Quaternion startRotation;

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = rootCanvas.worldCamera;
        }

        startAnchoredPos = tireRect.anchoredPosition;
        startRotation = tireRect.localRotation;
    }

    public void ResetMiniGame()
    {
        isCompleted = false;
        isDragging = false;
        accumulatedRotation = 0f;

        tireObject.SetActive(true);
        rimObject.SetActive(true);

        // 게이지 다시 표시
        gaugeRoot.SetActive(true);

        tireRect.anchoredPosition = startAnchoredPos;
        tireRect.localRotation = startRotation;

        if (gaugeFill != null)
            gaugeFill.fillAmount = 0f;

        boltPhaseRoot.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isCompleted) return;

        isDragging = true;
        previousAngle = GetPointerAngle(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isCompleted || !isDragging) return;

        float currentAngle = GetPointerAngle(eventData);

        // 각도 차이
        float delta = Mathf.DeltaAngle(previousAngle, currentAngle);

        // 시계 방향만 진행(오른쪽으로 돌리기)
        float clockwiseDelta = -delta;

        if (clockwiseDelta > 0f)
        {
            accumulatedRotation += clockwiseDelta;
            tireRect.Rotate(0f, 0f, -clockwiseDelta);

            UpdateGauge();

            if (accumulatedRotation >= requiredRotation)
            {
                accumulatedRotation = requiredRotation;
                UpdateGauge();

                isCompleted = true;
                isDragging = false;
                StartCoroutine(RemoveTireRoutine());
            }
        }
        else
        {
            // 반시계 방향으로 돌렸을 때 진행도를 깎을지 여부
            if (allowCounterClockwiseDecrease)
            {
                accumulatedRotation += clockwiseDelta; // 음수값이라 감소 효과
                accumulatedRotation = Mathf.Max(0f, accumulatedRotation);

                tireRect.Rotate(0f, 0f, -clockwiseDelta);
                UpdateGauge();
            }
        }

        previousAngle = currentAngle;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    private float GetPointerAngle(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tireRect,
            eventData.position,
            uiCamera,
            out localPoint
        );

        return Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
    }

    private void UpdateGauge()
    {
        if (gaugeFill != null)
        {
            gaugeFill.fillAmount = Mathf.Clamp01(accumulatedRotation / requiredRotation);
        }
    }

    private IEnumerator RemoveTireRoutine()
    {
        Vector2 startPos = tireRect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(removeMoveDistance, 0f);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / removeDuration;

            tireRect.anchoredPosition =
                Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        tireObject.SetActive(false);
        gaugeRoot.SetActive(false);

        // 나사 단계 등장
        boltPhaseRoot.SetActive(true);

        // 랜덤 나사 생성
        boltPhaseManager.StartBoltPhase();
    }
}