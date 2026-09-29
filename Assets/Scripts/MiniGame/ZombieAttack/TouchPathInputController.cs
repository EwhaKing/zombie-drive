using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TouchPathInputController :
    MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [Header("필수 연결")]
    [SerializeField] private AttackPathController attackPathController;

    // 경로 좌표와 똑같은 기준으로
    // 터치 좌표를 변환하기 위해 PathArea를 연결
    [SerializeField] private RectTransform pathCoordinateArea;

    // TouchArea 자신의 Image
    [SerializeField] private Image touchReceiverImage;


    [Header("시간 판정")]

    // 좀비가 공격할 때
    // START 지점을 누르기까지 허용되는 시간
    [SerializeField] private float reactionTimeLimit = 1f;

    // START 지점을 누른 뒤
    // 전체 경로를 그리는 데 허용되는 시간
    [SerializeField] private float maxDrawDuration = 2f;


    [Header("경로 판정")]

    // START에서 어느 정도 떨어져 있어도
    // 시작으로 인정할지
    [SerializeField] private float startTolerance = 100f;

    // 실제 경로 선에서 벗어나도 허용되는 거리
    [SerializeField] private float pathTolerance = 80f;

    // 모서리 근처에 도달하면
    // 해당 구간을 완료했다고 판단
    [SerializeField] private float cornerTolerance = 90f;

    // 마지막 지점 판정 범위
    [SerializeField] private float finishTolerance = 100f;

    // 선의 몇 % 이상 진행하면
    // 해당 선을 통과했다고 인정할지
    [Range(0.5f, 1f)]
    [SerializeField] private float segmentCompleteThreshold = 0.85f;

    // 손가락이 약간 뒤로 흔들리는 것 허용
    [Range(0f, 0.5f)]
    [SerializeField] private float backwardTolerance = 0.2f;

    // 일시적으로 선에서 벗어난 샘플 허용
    [SerializeField] private int maxConsecutiveOffPathSamples = 6;

    // 빠르게 드래그했을 때 중간 좌표를 보간하는 간격
    [SerializeField] private float sampleSpacing = 20f;


    // ==============================
    // 외부 이벤트
    // ==============================

    public event Action AttemptSucceeded;
    public event Action AttemptFailed;


    // ==============================
    // 내부 상태
    // ==============================

    private readonly List<Vector2> targetPoints =
        new List<Vector2>();

    private bool inputWindowOpen = false;
    private bool isDrawing = false;
    private bool resultSent = false;

    private int currentSegmentIndex = 0;

    private float currentSegmentMaxProgress = 0f;

    private int consecutiveOffPathSamples = 0;

    private Vector2 lastPointerPosition;

    private Coroutine reactionCoroutine;
    private Coroutine drawCoroutine;


    private void Awake()
    {
        if (touchReceiverImage == null)
        {
            touchReceiverImage =
                GetComponent<Image>();
        }

        // 게임 시작하자마자
        // 아무 때나 터치되는 것을 방지
        SetRaycastEnabled(false);
    }


    // ==============================
    // 터치 판정 시작
    // ==============================

    public void BeginInputWindow()
    {
        CancelInputWindow();

        BuildTargetPath();

        if (targetPoints.Count < 2)
        {
            Debug.LogWarning(
                "판정할 공격 경로가 없습니다."
            );

            return;
        }

        inputWindowOpen = true;
        resultSent = false;
        isDrawing = false;

        currentSegmentIndex = 0;
        currentSegmentMaxProgress = 0f;
        consecutiveOffPathSamples = 0;

        SetRaycastEnabled(true);

        reactionCoroutine =
            StartCoroutine(
                ReactionTimeoutCoroutine()
            );

        Debug.Log(
            "공격 입력 가능! START 지점을 터치하세요."
        );
    }


    // ==============================
    // 공격 경로 복사
    // ==============================

    private void BuildTargetPath()
    {
        targetPoints.Clear();

        if (attackPathController == null)
            return;

        IReadOnlyList<Vector2> originalPoints =
            attackPathController.CurrentPathPoints;

        for (int i = 0;
             i < originalPoints.Count;
             i++)
        {
            targetPoints.Add(
                originalPoints[i]
            );
        }

        // ㅁ, ◇ 같은 닫힌 경로는
        // 마지막 → 처음 구간도 판정해야 함
        if (
            attackPathController.IsCurrentPathClosed &&
            targetPoints.Count > 0)
        {
            targetPoints.Add(
                targetPoints[0]
            );
        }
    }


    // ==============================
    // START 지점 터치
    // ==============================

    public void OnPointerDown(
        PointerEventData eventData)
    {
        if (!inputWindowOpen ||
            resultSent)
        {
            return;
        }

        Vector2 localPoint;

        if (!TryGetLocalPoint(
                eventData,
                out localPoint))
        {
            return;
        }


        // START에서 너무 멀리 누르면
        // 바로 목숨을 깎지는 않고
        // 그냥 무시한다.
        float distanceFromStart =
            Vector2.Distance(
                localPoint,
                targetPoints[0]
            );

        if (distanceFromStart >
            startTolerance)
        {
            Debug.Log(
                "START 지점에서 너무 멀리 터치했습니다."
            );

            return;
        }


        // 정상 시작
        isDrawing = true;

        currentSegmentIndex = 0;
        currentSegmentMaxProgress = 0f;
        consecutiveOffPathSamples = 0;

        lastPointerPosition =
            localPoint;


        // 반응시간 타이머 중지
        if (reactionCoroutine != null)
        {
            StopCoroutine(
                reactionCoroutine
            );

            reactionCoroutine = null;
        }


        // 이제부터는 드래그 완료 제한시간 시작
        drawCoroutine =
            StartCoroutine(
                DrawTimeoutCoroutine()
            );


        Debug.Log(
            "경로 드래그 시작!"
        );
    }


    // ==============================
    // 드래그
    // ==============================

    public void OnDrag(
        PointerEventData eventData)
    {
        if (!inputWindowOpen ||
            !isDrawing ||
            resultSent)
        {
            return;
        }


        Vector2 currentPoint;

        if (!TryGetLocalPoint(
                eventData,
                out currentPoint))
        {
            return;
        }


        // 손가락을 빠르게 움직여서
        // 프레임 사이 좌표가 크게 벌어지는 경우를 대비해서
        // 중간 지점을 자동 생성해서 검사한다.

        float distance =
            Vector2.Distance(
                lastPointerPosition,
                currentPoint
            );


        int sampleCount =
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    distance / sampleSpacing
                )
            );


        // 너무 많은 반복 방지
        sampleCount =
            Mathf.Min(
                sampleCount,
                30
            );


        for (int i = 1;
             i <= sampleCount;
             i++)
        {
            if (resultSent)
                break;


            float t =
                (float)i /
                sampleCount;


            Vector2 samplePoint =
                Vector2.Lerp(
                    lastPointerPosition,
                    currentPoint,
                    t
                );


            ProcessSample(
                samplePoint
            );
        }


        lastPointerPosition =
            currentPoint;
    }


    // ==============================
    // 손가락을 뗐을 때
    // ==============================

    public void OnPointerUp(
        PointerEventData eventData)
    {
        if (!inputWindowOpen ||
            !isDrawing ||
            resultSent)
        {
            return;
        }


        Vector2 localPoint;

        if (TryGetLocalPoint(
                eventData,
                out localPoint))
        {
            ProcessSample(
                localPoint
            );
        }


        // 모든 경로를 순서대로 지나왔는지
        bool completedAllSegments =
            currentSegmentIndex >=
            targetPoints.Count - 1;


        // 마지막 위치에 제대로 도착했는지
        bool reachedFinish =
            Vector2.Distance(
                localPoint,
                targetPoints[
                    targetPoints.Count - 1
                ]
            )
            <= finishTolerance;


        if (
            completedAllSegments &&
            reachedFinish)
        {
            Success();
        }
        else
        {
            Fail(
                "경로를 끝까지 따라가지 못했습니다."
            );
        }
    }


    // ==============================
    // 한 좌표 판정
    // ==============================

    private void ProcessSample(
        Vector2 point)
    {
        if (resultSent ||
            !isDrawing)
        {
            return;
        }


        if (
            currentSegmentIndex >=
            targetPoints.Count - 1)
        {
            return;
        }


        Vector2 segmentStart =
            targetPoints[
                currentSegmentIndex
            ];

        Vector2 segmentEnd =
            targetPoints[
                currentSegmentIndex + 1
            ];


        float progress;

        float distanceToSegment =
            DistanceToSegment(
                point,
                segmentStart,
                segmentEnd,
                out progress
            );


        // ==========================
        // 정상적으로 경로 위에 있음
        // ==========================

        if (distanceToSegment <=
            pathTolerance)
        {
            consecutiveOffPathSamples = 0;


            // 진행 방향을 거꾸로 크게 움직였는지 검사
            if (
                progress +
                backwardTolerance <
                currentSegmentMaxProgress)
            {
                RegisterOffPath();

                return;
            }


            currentSegmentMaxProgress =
                Mathf.Max(
                    currentSegmentMaxProgress,
                    progress
                );


            bool reachedSegmentEnd =
                currentSegmentMaxProgress >=
                segmentCompleteThreshold;


            bool nearCorner =
                Vector2.Distance(
                    point,
                    segmentEnd
                )
                <= cornerTolerance;


            if (
                reachedSegmentEnd ||
                nearCorner)
            {
                currentSegmentIndex++;

                currentSegmentMaxProgress = 0f;

                Debug.Log(
                    "경로 구간 통과 : "
                    + currentSegmentIndex
                );
            }


            return;
        }


        // ==========================
        // 현재 선을 거의 끝냈는데
        // 다음 선으로 넘어간 경우
        // ==========================

        if (
            currentSegmentMaxProgress >= 0.65f &&
            currentSegmentIndex + 1 <
            targetPoints.Count - 1)
        {
            float nextProgress;

            float nextDistance =
                DistanceToSegment(
                    point,
                    targetPoints[
                        currentSegmentIndex + 1
                    ],
                    targetPoints[
                        currentSegmentIndex + 2
                    ],
                    out nextProgress
                );


            if (nextDistance <=
                pathTolerance)
            {
                currentSegmentIndex++;

                currentSegmentMaxProgress =
                    nextProgress;

                consecutiveOffPathSamples = 0;

                return;
            }
        }


        RegisterOffPath();
    }


    // ==============================
    // 경로 이탈 처리
    // ==============================

    private void RegisterOffPath()
    {
        consecutiveOffPathSamples++;


        if (
            consecutiveOffPathSamples >
            maxConsecutiveOffPathSamples)
        {
            Fail(
                "공격 경로에서 너무 많이 벗어났습니다."
            );
        }
    }


    // ==============================
    // 점과 선분 사이 거리
    // ==============================

    private float DistanceToSegment(
        Vector2 point,
        Vector2 start,
        Vector2 end,
        out float progress)
    {
        Vector2 segment =
            end - start;


        float sqrLength =
            segment.sqrMagnitude;


        if (sqrLength <=
            Mathf.Epsilon)
        {
            progress = 0f;

            return Vector2.Distance(
                point,
                start
            );
        }


        progress =
            Vector2.Dot(
                point - start,
                segment
            )
            / sqrLength;


        progress =
            Mathf.Clamp01(
                progress
            );


        Vector2 nearestPoint =
            start +
            segment * progress;


        return Vector2.Distance(
            point,
            nearestPoint
        );
    }


    // ==============================
    // 화면 좌표 → PathArea 좌표
    // ==============================

    private bool TryGetLocalPoint(
        PointerEventData eventData,
        out Vector2 localPoint)
    {
        localPoint = Vector2.zero;


        if (pathCoordinateArea == null)
            return false;


        return RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                pathCoordinateArea,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );
    }


    // ==============================
    // START 누르기 제한시간
    // ==============================

    private IEnumerator
        ReactionTimeoutCoroutine()
    {
        yield return new WaitForSeconds(
            reactionTimeLimit
        );


        if (
            inputWindowOpen &&
            !isDrawing &&
            !resultSent)
        {
            Fail(
                "공격 타이밍을 놓쳤습니다."
            );
        }
    }


    // ==============================
    // 드래그 제한시간
    // ==============================

    private IEnumerator
        DrawTimeoutCoroutine()
    {
        yield return new WaitForSeconds(
            maxDrawDuration
        );


        if (
            inputWindowOpen &&
            isDrawing &&
            !resultSent)
        {
            Fail(
                "제한시간 안에 경로를 완성하지 못했습니다."
            );
        }
    }


    // ==============================
    // 성공
    // ==============================

    private void Success()
    {
        if (resultSent)
            return;


        resultSent = true;

        Debug.Log(
            "좀비 공격 성공! 좀비 처치!"
        );


        EndInputState();


        AttemptSucceeded?.Invoke();
    }


    // ==============================
    // 실패
    // ==============================

    private void Fail(
        string reason)
    {
        if (resultSent)
            return;


        resultSent = true;


        Debug.Log(
            "공격 실패 : " + reason
        );


        EndInputState();


        AttemptFailed?.Invoke();
    }


    // ==============================
    // 현재 입력 종료
    // ==============================

    private void EndInputState()
    {
        inputWindowOpen = false;
        isDrawing = false;

        SetRaycastEnabled(false);


        if (reactionCoroutine != null)
        {
            StopCoroutine(
                reactionCoroutine
            );

            reactionCoroutine = null;
        }


        if (drawCoroutine != null)
        {
            StopCoroutine(
                drawCoroutine
            );

            drawCoroutine = null;
        }
    }


    // ==============================
    // 외부에서 강제로 입력 취소
    // ==============================

    public void CancelInputWindow()
    {
        inputWindowOpen = false;
        isDrawing = false;
        resultSent = false;


        SetRaycastEnabled(false);


        if (reactionCoroutine != null)
        {
            StopCoroutine(
                reactionCoroutine
            );

            reactionCoroutine = null;
        }


        if (drawCoroutine != null)
        {
            StopCoroutine(
                drawCoroutine
            );

            drawCoroutine = null;
        }
    }


    private void SetRaycastEnabled(
        bool enabled)
    {
        if (touchReceiverImage != null)
        {
            touchReceiverImage.raycastTarget =
                enabled;
        }
    }
}