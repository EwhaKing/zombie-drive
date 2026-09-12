using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WheelRemovalMiniGame : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    // =====================================
    // 1단계 : 기존 타이어 제거
    // =====================================

    [Header("게이지")]
    [SerializeField] private GameObject gaugeRoot;
    [SerializeField] private Image gaugeFill;

    [Header("회전 대상")]
    [SerializeField] private RectTransform tireRect;
    [SerializeField] private GameObject tireObject;

    [Header("남겨둘 은색 림")]
    [SerializeField] private GameObject rimObject;

    [Header("회전 설정")]
    [SerializeField] private float requiredRotation = 720f; // 2바퀴

    [SerializeField]
    private bool allowCounterClockwiseDecrease = false;

    [Header("제거 연출")]
    [SerializeField] private float removeMoveDistance = 180f;
    [SerializeField] private float removeDuration = 0.25f;

    // =====================================
    // 2단계 : 나사
    // =====================================

    [Header("나사 단계")]
    [SerializeField] private GameObject boltPhaseRoot;
    [SerializeField] private BoltPhaseManager boltPhaseManager;

    // =====================================
    // 3단계 : 새 타이어 장착
    // =====================================

    [Header("타이어 부착 단계")]
    [SerializeField] private TireInstallMiniGame tireInstallMiniGame;

    // =====================================
    // 전체 게임 관리자
    // =====================================

    [Header("전체 정비 관리자")]
    [SerializeField] private RepairShopGameManager gameManager;


    // =====================================
    // 내부 변수
    // =====================================

    private Canvas rootCanvas;
    private Camera uiCamera;

    private float accumulatedRotation = 0f;
    private float previousAngle = 0f;

    private bool isDragging = false;
    private bool isCompleted = false;

    private Vector2 startAnchoredPos;
    private Quaternion startRotation;


    // =====================================
    // 현재 1단계 진행도
    //
    // 0 = 0%
    // 1 = 100%
    // =====================================

    public float Completion01
    {
        get
        {
            if (requiredRotation <= 0f)
            {
                return 0f;
            }

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


    public bool IsCompleted
    {
        get
        {
            return isCompleted;
        }
    }


    // =====================================
    // 시작할 때 한 번 실행
    // =====================================

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null &&
            rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = rootCanvas.worldCamera;
        }

        if (tireRect != null)
        {
            startAnchoredPos = tireRect.anchoredPosition;
            startRotation = tireRect.localRotation;
        }
    }


    // =====================================
    // 미니게임 초기화
    // =====================================

    public void ResetMiniGame()
    {
        isCompleted = false;
        isDragging = false;

        accumulatedRotation = 0f;

        // -----------------------------
        // 기존 타이어 다시 표시
        // -----------------------------

        if (tireObject != null)
        {
            tireObject.SetActive(true);
        }


        // -----------------------------
        // 림 표시
        // -----------------------------

        if (rimObject != null)
        {
            rimObject.SetActive(true);
        }


        // -----------------------------
        // 1단계 게이지 표시
        // -----------------------------

        if (gaugeRoot != null)
        {
            gaugeRoot.SetActive(true);
        }


        if (gaugeFill != null)
        {
            gaugeFill.fillAmount = 0f;
        }


        // -----------------------------
        // 타이어 위치/회전 초기화
        // -----------------------------

        if (tireRect != null)
        {
            tireRect.anchoredPosition =
                startAnchoredPos;

            tireRect.localRotation =
                startRotation;
        }


        // -----------------------------
        // 2단계 숨기기
        // -----------------------------

        if (boltPhaseRoot != null)
        {
            boltPhaseRoot.SetActive(false);
        }


        // -----------------------------
        // 3단계 초기화
        // -----------------------------

        if (tireInstallMiniGame != null)
        {
            tireInstallMiniGame
                .ResetTireInstallPhase();
        }


        // -----------------------------
        // 진행도 초기화
        // -----------------------------

        if (gameManager != null)
        {
            gameManager
                .UpdateCurrentProgress(0f);
        }
    }


    // =====================================
    // 타이어 터치 시작
    // =====================================

    public void OnPointerDown(
        PointerEventData eventData)
    {
        if (isCompleted)
        {
            return;
        }

        isDragging = true;

        previousAngle =
            GetPointerAngle(eventData);
    }


    // =====================================
    // 타이어 회전
    // =====================================

    public void OnDrag(
        PointerEventData eventData)
    {
        if (isCompleted ||
            !isDragging)
        {
            return;
        }


        float currentAngle =
            GetPointerAngle(eventData);


        // 이전 터치 위치와
        // 현재 터치 위치의 각도 차이
        float delta =
            Mathf.DeltaAngle(
                previousAngle,
                currentAngle
            );


        // 시계 방향을 +
        float clockwiseDelta = -delta;


        // =================================
        // 시계 방향으로 돌렸을 때
        // =================================

        if (clockwiseDelta > 0f)
        {
            accumulatedRotation +=
                clockwiseDelta;


            accumulatedRotation =
                Mathf.Clamp(
                    accumulatedRotation,
                    0f,
                    requiredRotation
                );


            // 실제 타이어 회전
            if (tireRect != null)
            {
                tireRect.Rotate(
                    0f,
                    0f,
                    -clockwiseDelta
                );
            }


            // 게이지 + 진행도 갱신
            UpdateGauge();


            // =================================
            // 필요한 회전량을 전부 채움
            // =================================

            if (accumulatedRotation
                >= requiredRotation)
            {
                accumulatedRotation =
                    requiredRotation;


                UpdateGauge();


                isCompleted = true;
                isDragging = false;


                // -----------------------------
                // 1단계 100% 성공
                // -----------------------------

                if (gameManager != null)
                {
                    gameManager.CompleteStage(
                        1,
                        1f
                    );
                }


                // 타이어 제거 연출
                StartCoroutine(
                    RemoveTireRoutine()
                );
            }
        }


        // =================================
        // 반시계 방향으로 돌렸을 때
        // =================================

        else if (allowCounterClockwiseDecrease)
        {
            accumulatedRotation +=
                clockwiseDelta;


            accumulatedRotation =
                Mathf.Clamp(
                    accumulatedRotation,
                    0f,
                    requiredRotation
                );


            if (tireRect != null)
            {
                tireRect.Rotate(
                    0f,
                    0f,
                    -clockwiseDelta
                );
            }


            UpdateGauge();
        }


        previousAngle =
            currentAngle;
    }


    // =====================================
    // 터치 종료
    // =====================================

    public void OnPointerUp(
        PointerEventData eventData)
    {
        isDragging = false;
    }


    // =====================================
    // 마우스/터치 위치를
    // 타이어 중심 기준 각도로 변환
    // =====================================

    private float GetPointerAngle(
        PointerEventData eventData)
    {
        if (tireRect == null)
        {
            return 0f;
        }


        Vector2 localPoint;


        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                tireRect,
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
    // 1단계 게이지 + 진행도 갱신
    // =====================================

    private void UpdateGauge()
    {
        float progress = 0f;

        if (requiredRotation > 0f)
        {
            progress = Mathf.Clamp01(
                accumulatedRotation / requiredRotation
            );
        }

        // 게이지
        if (gaugeFill != null)
        {
            gaugeFill.fillAmount = progress;
        }

        // 진행도 기록
        if (gameManager != null)
        {
            gameManager.UpdateCurrentProgress(progress);
        }
    }


    // =====================================
    // 정상적으로 1단계 완료했을 때
    // 타이어 제거 연출
    // =====================================

    private IEnumerator RemoveTireRoutine()
    {
        if (tireRect == null)
        {
            FinishTireRemoval();
            yield break;
        }


        Vector2 startPos =
            tireRect.anchoredPosition;


        Vector2 endPos =
            startPos +
            new Vector2(
                removeMoveDistance,
                0f
            );


        float t = 0f;


        while (t < 1f)
        {
            if (removeDuration <= 0f)
            {
                t = 1f;
            }
            else
            {
                t +=
                    Time.deltaTime /
                    removeDuration;
            }


            tireRect.anchoredPosition =
                Vector2.Lerp(
                    startPos,
                    endPos,
                    t
                );


            yield return null;
        }


        FinishTireRemoval();
    }


    // =====================================
    // 1단계 종료 후
    // 2단계로 이동
    // =====================================

    private void FinishTireRemoval()
    {
        // 낡은 타이어 숨기기
        if (tireObject != null)
        {
            tireObject.SetActive(false);
        }


        // 1단계 게이지 숨기기
        if (gaugeRoot != null)
        {
            gaugeRoot.SetActive(false);
        }


        // 2단계 등장
        if (boltPhaseRoot != null)
        {
            boltPhaseRoot.SetActive(true);
        }


        // 2단계 시작
        if (boltPhaseManager != null)
        {
            boltPhaseManager
                .StartBoltPhase();
        }
    }


    // =====================================
    // 1단계 제한시간 초과
    //
    // RepairShopGameManager에서 호출
    // =====================================

    public void ForceFinishByTimeout()
    {
        // 이미 끝난 상태라면 중복 실행 방지
        if (isCompleted)
        {
            return;
        }


        Debug.Log(
            "1단계 제한시간 종료!"
        );


        isCompleted = true;
        isDragging = false;


        // ---------------------------------
        // 중요!
        //
        // 여기서는 CompleteStage를 호출하지 않음.
        //
        // 제한시간 당시 진행도 저장은
        // RepairShopGameManager가 이미 담당함.
        // ---------------------------------


        // 낡은 타이어 바로 제거
        if (tireObject != null)
        {
            tireObject.SetActive(false);
        }


        // 게이지 숨기기
        if (gaugeRoot != null)
        {
            gaugeRoot.SetActive(false);
        }


        // 2단계 등장
        if (boltPhaseRoot != null)
        {
            boltPhaseRoot.SetActive(true);
        }


        // 2단계 시작
        if (boltPhaseManager != null)
        {
            boltPhaseManager
                .StartBoltPhase();
        }
    }
}