using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackPathController : MonoBehaviour
{
    [Header("시작점 표시")]
    [SerializeField] private RectTransform startMarker;

    public event System.Action PreviewFinished;

    [Header("경로 영역")]
    [SerializeField] private RectTransform pathArea;

    [Header("경로 표시 설정")]
    [SerializeField] private float previewDuration = 1f;

    [SerializeField] private float pathWidth = 850f;
    [SerializeField] private float pathHeight = 500f;


    [Header("경로 모양 랜덤 설정")]
    [SerializeField] private float jitterAmount = 35f;

    [SerializeField] private float maxRotation = 12f;


    [Header("선 설정")]
    [SerializeField] private float lineThickness = 15f;

    [SerializeField] private Color lineColor = Color.yellow;


    [Header("화살표 설정")]
    [SerializeField] private bool showArrows = true;

    // 화살표 날개 길이
    [SerializeField] private float arrowLength = 45f;

    // 화살표 두께
    [SerializeField] private float arrowThickness = 12f;

    // 화살표 벌어진 각도
    [SerializeField] private float arrowAngle = 30f;

    // 선의 몇 % 지점에 화살표를 둘지
    // 0.7 = 시작점에서 70% 지점
    [Range(0.1f, 0.9f)]
    [SerializeField] private float arrowPosition = 0.7f;


    // ==============================
    // 실제 공격 경로 데이터
    // ==============================

    private List<Vector2> currentPathPoints =
        new List<Vector2>();


    // 선 + 화살표 UI 오브젝트 저장
    private List<GameObject> pathVisualObjects =
        new List<GameObject>();


    // ㅁ, ◇ 같은 닫힌 경로인지
    private bool currentPathClosed = false;


    // 다른 스크립트에서 사용 가능
    public IReadOnlyList<Vector2> CurrentPathPoints
    {
        get { return currentPathPoints; }
    }


    public bool IsCurrentPathClosed
    {
        get { return currentPathClosed; }
    }


    private void Start()
    {
        // 현재는 테스트용
        ShowRandomPath();
    }


    // ==============================
    // 새로운 랜덤 경로 생성
    // ==============================

    public void ShowRandomPath()
    {
        StopAllCoroutines();

        ClearPathVisual();

        if (startMarker != null)
            startMarker.gameObject.SetActive(false);

        GenerateRandomPath();

        DrawPath();

        ShowStartMarker();

        StartCoroutine(
            HidePathAfterDelay()
        );
    }

    // ==============================
    // 랜덤 패턴 생성
    // ==============================

    private void GenerateRandomPath()
    {
        currentPathPoints.Clear();

        currentPathClosed = false;


        // 현재 7가지 패턴
        int pattern =
            Random.Range(0, 7);


        List<Vector2> normalizedPoints =
            new List<Vector2>();


        switch (pattern)
        {
            // =====================
            // Z
            // =====================

            case 0:

                normalizedPoints.Add(
                    new Vector2(-0.9f, 0.8f)
                );

                normalizedPoints.Add(
                    new Vector2(0.9f, 0.8f)
                );

                normalizedPoints.Add(
                    new Vector2(-0.9f, -0.8f)
                );

                normalizedPoints.Add(
                    new Vector2(0.9f, -0.8f)
                );

                break;


            // =====================
            // N
            // =====================

            case 1:

                normalizedPoints.Add(
                    new Vector2(-0.8f, -0.8f)
                );

                normalizedPoints.Add(
                    new Vector2(-0.8f, 0.8f)
                );

                normalizedPoints.Add(
                    new Vector2(0.8f, -0.8f)
                );

                normalizedPoints.Add(
                    new Vector2(0.8f, 0.8f)
                );

                break;


            // =====================
            // ㅁ
            // =====================

            case 2:

                normalizedPoints.Add(
                    new Vector2(-0.8f, 0.8f)
                );

                normalizedPoints.Add(
                    new Vector2(0.8f, 0.8f)
                );

                normalizedPoints.Add(
                    new Vector2(0.8f, -0.8f)
                );

                normalizedPoints.Add(
                    new Vector2(-0.8f, -0.8f)
                );

                currentPathClosed = true;

                break;


            // =====================
            // U
            // =====================

            case 3:

                normalizedPoints.Add(
                    new Vector2(-0.8f, 0.8f)
                );

                normalizedPoints.Add(
                    new Vector2(-0.8f, -0.8f)
                );

                normalizedPoints.Add(
                    new Vector2(0.8f, -0.8f)
                );

                normalizedPoints.Add(
                    new Vector2(0.8f, 0.8f)
                );

                break;


            // =====================
            // ◇
            // =====================

            case 4:

                normalizedPoints.Add(
                    new Vector2(0f, 0.9f)
                );

                normalizedPoints.Add(
                    new Vector2(0.9f, 0f)
                );

                normalizedPoints.Add(
                    new Vector2(0f, -0.9f)
                );

                normalizedPoints.Add(
                    new Vector2(-0.9f, 0f)
                );

                currentPathClosed = true;

                break;


            // =====================
            // 번개
            // =====================

            case 5:

                normalizedPoints.Add(
                    new Vector2(-0.7f, 0.9f)
                );

                normalizedPoints.Add(
                    new Vector2(0.4f, 0.3f)
                );

                normalizedPoints.Add(
                    new Vector2(-0.3f, -0.2f)
                );

                normalizedPoints.Add(
                    new Vector2(0.7f, -0.9f)
                );

                break;


            // =====================
            // ㄹ / 계단 형태
            // =====================

            case 6:

                normalizedPoints.Add(
                    new Vector2(-0.8f, 0.7f)
                );

                normalizedPoints.Add(
                    new Vector2(0.5f, 0.7f)
                );

                normalizedPoints.Add(
                    new Vector2(-0.5f, -0.2f)
                );

                normalizedPoints.Add(
                    new Vector2(0.8f, -0.7f)
                );

                break;
        }


        // ==============================
        // 좌우 / 상하 랜덤 반전
        // ==============================

        bool flipX =
            Random.value > 0.5f;

        bool flipY =
            Random.value > 0.5f;


        float randomRotation =
            Random.Range(
                -maxRotation,
                maxRotation
            );


        for (int i = 0;
             i < normalizedPoints.Count;
             i++)
        {
            Vector2 point =
                normalizedPoints[i];


            if (flipX)
                point.x *= -1f;


            if (flipY)
                point.y *= -1f;


            // UI 실제 크기로 변환
            point.x *=
                pathWidth / 2f;

            point.y *=
                pathHeight / 2f;


            // 약간 랜덤하게 변형
            point.x +=
                Random.Range(
                    -jitterAmount,
                    jitterAmount
                );


            point.y +=
                Random.Range(
                    -jitterAmount,
                    jitterAmount
                );


            // 약간 회전
            point =
                RotatePoint(
                    point,
                    randomRotation
                );


            currentPathPoints.Add(
                point
            );
        }


        // 진행 방향도 랜덤
        if (Random.value > 0.5f)
        {
            currentPathPoints.Reverse();
        }
    }


    // ==============================
    // 포인트 회전
    // ==============================

    private Vector2 RotatePoint(
        Vector2 point,
        float degrees)
    {
        float radians =
            degrees * Mathf.Deg2Rad;


        float cos =
            Mathf.Cos(radians);

        float sin =
            Mathf.Sin(radians);


        float x =
            point.x * cos
            - point.y * sin;


        float y =
            point.x * sin
            + point.y * cos;


        return new Vector2(x, y);
    }


    // ==============================
    // 전체 경로 그리기
    // ==============================

    private void DrawPath()
    {
        if (pathArea == null)
        {
            Debug.LogError(
                "PathArea가 연결되지 않았습니다!"
            );

            return;
        }


        // Point 0 → Point 1
        // Point 1 → Point 2
        // Point 2 → Point 3

        for (int i = 0;
             i < currentPathPoints.Count - 1;
             i++)
        {
            DrawSegmentWithArrow(
                currentPathPoints[i],
                currentPathPoints[i + 1]
            );
        }


        // ㅁ / ◇는 마지막 → 처음도 연결
        if (currentPathClosed &&
            currentPathPoints.Count >= 2)
        {
            DrawSegmentWithArrow(
                currentPathPoints[
                    currentPathPoints.Count - 1
                ],
                currentPathPoints[0]
            );
        }
    }


    // ==============================
    // 선 + 화살표 그리기
    // ==============================

    private void DrawSegmentWithArrow(
        Vector2 startPoint,
        Vector2 endPoint)
    {
        // 기본 공격 경로 선
        CreateVisualLine(
            startPoint,
            endPoint,
            lineThickness,
            "PathLine"
        );


        if (showArrows)
        {
            CreateArrow(
                startPoint,
                endPoint
            );
        }
    }


    // ==============================
    // 화살표 생성
    // ==============================

    private void CreateArrow(
        Vector2 startPoint,
        Vector2 endPoint)
    {
        Vector2 direction =
            (endPoint - startPoint).normalized;


        // 선 중간보다 살짝 뒤쪽에 화살표 배치
        Vector2 arrowTip =
            Vector2.Lerp(
                startPoint,
                endPoint,
                arrowPosition
            );


        // 이동 방향의 반대 방향
        Vector2 backward =
            -direction;


        // 왼쪽 화살표 날개
        Vector2 leftDirection =
            RotateVector(
                backward,
                arrowAngle
            );


        // 오른쪽 화살표 날개
        Vector2 rightDirection =
            RotateVector(
                backward,
                -arrowAngle
            );


        Vector2 leftEnd =
            arrowTip +
            leftDirection *
            arrowLength;


        Vector2 rightEnd =
            arrowTip +
            rightDirection *
            arrowLength;


        //    \ /
        //     >
        //
        // 형태 만들기

        CreateVisualLine(
            arrowTip,
            leftEnd,
            arrowThickness,
            "ArrowLeft"
        );


        CreateVisualLine(
            arrowTip,
            rightEnd,
            arrowThickness,
            "ArrowRight"
        );
    }


    // ==============================
    // Vector 회전
    // ==============================

    private Vector2 RotateVector(
        Vector2 vector,
        float degrees)
    {
        float radians =
            degrees * Mathf.Deg2Rad;


        float cos =
            Mathf.Cos(radians);

        float sin =
            Mathf.Sin(radians);


        return new Vector2(
            vector.x * cos
            - vector.y * sin,

            vector.x * sin
            + vector.y * cos
        );
    }


    // ==============================
    // 두 점 사이에 UI 선 생성
    // ==============================

    private void CreateVisualLine(
        Vector2 startPoint,
        Vector2 endPoint,
        float thickness,
        string objectName)
    {
        GameObject lineObject =
            new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(Image)
            );


        lineObject.transform.SetParent(
            pathArea,
            false
        );


        Image image =
            lineObject.GetComponent<Image>();


        image.color =
            lineColor;


        // 터치 판정 방해 X
        image.raycastTarget =
            false;


        RectTransform rect =
            lineObject.GetComponent<RectTransform>();


        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);


        Vector2 middlePoint =
            (startPoint + endPoint)
            / 2f;


        rect.anchoredPosition =
            middlePoint;


        float distance =
            Vector2.Distance(
                startPoint,
                endPoint
            );


        rect.sizeDelta =
            new Vector2(
                distance,
                thickness
            );


        Vector2 direction =
            endPoint - startPoint;


        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;


        rect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );


        pathVisualObjects.Add(
            lineObject
        );
    }


    // ==============================
    // 1초 후 경로 제거
    // ==============================

    private IEnumerator HidePathAfterDelay()
    {
        yield return new WaitForSeconds(
            previewDuration
        );

        ClearPathVisual();

        if (startMarker != null)
        {
            startMarker.gameObject.SetActive(false);
        }

        Debug.Log(
            "공격 경로 미리보기 종료!"
        );

        PreviewFinished?.Invoke();
    }


    // ==============================
    // 화면 표시만 삭제
    // 좌표 데이터는 유지
    // ==============================

    private void ClearPathVisual()
    {
        foreach (
            GameObject visual
            in pathVisualObjects)
        {
            if (visual != null)
            {
                Destroy(visual);
            }
        }


        pathVisualObjects.Clear();
    }

    private void ShowStartMarker()
    {
        if (startMarker == null)
            return;

        if (currentPathPoints.Count == 0)
            return;

        startMarker.gameObject.SetActive(true);

        startMarker.anchorMin = new Vector2(0.5f, 0.5f);
        startMarker.anchorMax = new Vector2(0.5f, 0.5f);
        startMarker.pivot = new Vector2(0.5f, 0.5f);

        startMarker.anchoredPosition =
            currentPathPoints[0];

        // 선보다 앞에 표시
        startMarker.SetAsLastSibling();
    }

    public void CancelPreview()
    {
        // 1초 경로 표시 코루틴 중지
        StopAllCoroutines();

        // 현재 화면의 경로 / 화살표 제거
        ClearPathVisual();

        // START 표시 제거
        if (startMarker != null)
        {
            startMarker.gameObject.SetActive(false);
        }
    }
}