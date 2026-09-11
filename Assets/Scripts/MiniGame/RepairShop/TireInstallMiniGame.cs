using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TireInstallMiniGame : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [Header("새 타이어")]
    [SerializeField] private GameObject newTireObject;
    [SerializeField] private RectTransform newTireRect;
    [SerializeField] private CanvasGroup newTireCanvasGroup;

    [Header("게이지")]
    [SerializeField] private GameObject gaugeRoot;
    [SerializeField] private Image gaugeFill;

    [Header("회전 설정")]
    [SerializeField] private float requiredRotation = 720f;
    [SerializeField] private bool allowCounterClockwiseDecrease = false;

    [Header("타이어 투명도")]
    [Range(0f, 1f)]
    [SerializeField] private float startAlpha = 0.25f;

    [Header("전체 정비 관리자")]
    [SerializeField] private RepairShopGameManager gameManager;


    private Canvas rootCanvas;
    private Camera uiCamera;

    private float accumulatedRotation = 0f;
    private float previousAngle = 0f;

    private bool isDragging = false;
    private bool isPhaseActive = false;
    private bool isCompleted = false;

    private Quaternion startRotation;

    private bool initialized = false;


    // =====================================
    // 현재 진행도 0 ~ 1
    // =====================================

    public float Completion01
    {
        get
        {
            if (requiredRotation <= 0f)
                return 0f;

            return Mathf.Clamp01(
                accumulatedRotation / requiredRotation
            );
        }
    }


    public float CompletionPercent
    {
        get
        {
            return Completion01 * 100f;
        }
    }


    public bool IsCompleted => isCompleted;


    private void Awake()
    {
        Initialize();
    }


    // =====================================
    // 초기 설정
    // =====================================

    private void Initialize()
    {
        if (initialized)
            return;

        rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null &&
            rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = rootCanvas.worldCamera;
        }

        if (newTireRect != null)
        {
            startRotation = newTireRect.localRotation;
        }

        initialized = true;
    }


    // =====================================
    // 3단계 시작
    // =====================================

    public void StartTireInstallPhase()
    {
        Initialize();

        Debug.Log("3단계 타이어 부착 시작!");

        accumulatedRotation = 0f;

        isDragging = false;
        isCompleted = false;
        isPhaseActive = true;


        if (newTireObject != null)
        {
            newTireObject.SetActive(true);
        }


        if (gaugeRoot != null)
        {
            gaugeRoot.SetActive(true);
        }


        if (newTireRect != null)
        {
            newTireRect.localRotation = startRotation;
        }


        if (newTireCanvasGroup != null)
        {
            newTireCanvasGroup.alpha = startAlpha;
        }


        if (gaugeFill != null)
        {
            gaugeFill.fillAmount = 0f;
        }


        // 3단계 제한시간 시작
        if (gameManager != null)
        {
            gameManager.StartStage(3);
            gameManager.UpdateCurrentProgress(0f);
        }
    }


    // =====================================
    // 터치 시작
    // =====================================

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isPhaseActive || isCompleted)
            return;

        isDragging = true;

        previousAngle =
            GetPointerAngle(eventData);
    }


    // =====================================
    // 타이어 회전
    // =====================================

    public void OnDrag(PointerEventData eventData)
    {
        if (!isPhaseActive ||
            isCompleted ||
            !isDragging)
        {
            return;
        }


        float currentAngle =
            GetPointerAngle(eventData);


        float delta =
            Mathf.DeltaAngle(
                previousAngle,
                currentAngle
            );


        // 시계 방향을 +
        float clockwiseDelta = -delta;


        // ---------------------------------
        // 시계 방향
        // ---------------------------------

        if (clockwiseDelta > 0f)
        {
            accumulatedRotation += clockwiseDelta;

            accumulatedRotation =
                Mathf.Clamp(
                    accumulatedRotation,
                    0f,
                    requiredRotation
                );


            if (newTireRect != null)
            {
                newTireRect.Rotate(
                    0f,
                    0f,
                    -clockwiseDelta
                );
            }


            UpdateProgress();
        }


        // ---------------------------------
        // 반시계 방향
        // ---------------------------------

        else if (allowCounterClockwiseDecrease)
        {
            accumulatedRotation += clockwiseDelta;

            accumulatedRotation =
                Mathf.Clamp(
                    accumulatedRotation,
                    0f,
                    requiredRotation
                );


            if (newTireRect != null)
            {
                newTireRect.Rotate(
                    0f,
                    0f,
                    -clockwiseDelta
                );
            }


            UpdateProgress();
        }


        previousAngle = currentAngle;
    }


    // =====================================
    // 터치 종료
    // =====================================

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }


    // =====================================
    // 현재 터치 각도
    // =====================================

    private float GetPointerAngle(
        PointerEventData eventData)
    {
        if (newTireRect == null)
            return 0f;


        Vector2 localPoint;


        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                newTireRect,
                eventData.position,
                uiCamera,
                out localPoint
            );


        return Mathf.Atan2(
            localPoint.y,
            localPoint.x
        ) * Mathf.Rad2Deg;
    }


    // =====================================
    // 게이지 + 투명도 + 진행도
    // =====================================

    private void UpdateProgress()
    {
        float progress = Completion01;


        // 원형 게이지
        if (gaugeFill != null)
        {
            gaugeFill.fillAmount = progress;
        }


        // 진행될수록 타이어가 선명해짐
        if (newTireCanvasGroup != null)
        {
            newTireCanvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    1f,
                    progress
                );
        }


        // RepairShopGameManager에 진행도 전달
        if (gameManager != null)
        {
            gameManager.UpdateCurrentProgress(
                progress
            );
        }


        if (progress >= 1f)
        {
            CompleteTireInstall();
        }
    }


    // =====================================
    // 정상 완료
    // =====================================

    private void CompleteTireInstall()
    {
        if (isCompleted)
            return;


        isCompleted = true;
        isDragging = false;
        isPhaseActive = false;

        accumulatedRotation = requiredRotation;


        if (gaugeFill != null)
        {
            gaugeFill.fillAmount = 1f;
        }


        if (newTireCanvasGroup != null)
        {
            newTireCanvasGroup.alpha = 1f;
        }


        Debug.Log("타이어를 완전히 체결했습니다!");


        if (gameManager != null)
        {
            gameManager.CompleteStage(
                3,
                1f
            );

            gameManager.FinishRepair();
        }
    }


    // =====================================
    // 시간 초과
    // =====================================

    public void ForceFinishByTimeout()
    {
        if (isCompleted)
            return;

        isCompleted = true;
        isDragging = false;
        isPhaseActive = false;

        Debug.Log(
            "3단계 제한시간 종료! 진행도: "
            + CompletionPercent.ToString("F1")
            + "%"
        );
    }


    // =====================================
    // 초기화
    // =====================================

    public void ResetTireInstallPhase()
    {
        Initialize();

        accumulatedRotation = 0f;

        isDragging = false;
        isCompleted = false;
        isPhaseActive = false;


        if (gaugeFill != null)
        {
            gaugeFill.fillAmount = 0f;
        }


        if (newTireCanvasGroup != null)
        {
            newTireCanvasGroup.alpha =
                startAlpha;
        }


        if (newTireRect != null)
        {
            newTireRect.localRotation =
                startRotation;
        }


        if (gaugeRoot != null)
        {
            gaugeRoot.SetActive(false);
        }


        if (newTireObject != null)
        {
            newTireObject.SetActive(false);
        }
    }
}