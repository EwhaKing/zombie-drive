using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZombieSliceGameManager : MonoBehaviour
{
    public static ZombieSliceGameManager Instance;


    [Header("UI")]
    public RectTransform canvasRect;
    public RectTransform routeRoot;

    public Image routeDotPrefab;

    public GameObject zombieObject;


    [Header("경로 설정")]

    // 경로를 몇 개의 점으로 만들 것인지
    public int pathSampleCount = 25;

    // 화면 가장자리에서 어느 정도 떨어진 위치에
    // 경로를 생성할 것인지
    public float screenMargin = 200f;

    // 플레이어가 경로에서 얼마나 벗어나도 인정할지
    public float pathTolerance = 120f;

    // 시작점 / 끝점 허용 범위
    public float startEndTolerance = 160f;


    [Header("경로 표시 시간")]

    // 경로를 보여주는 시간
    public float previewTime = 1f;


    [Header("공격 타이밍")]

    // 좀비가 나타난 후 공격하는 순간
    public float attackMoment = 1.2f;

    // ± 시간 허용 범위
    public float timingTolerance = 0.5f;


    [Header("난이도 증가")]

    // 좀비 한 마리 처리할 때마다
    // 공격 타이밍이 얼마나 빨라지는지
    public float speedIncreaseAmount = 0.07f;

    // 아무리 빨라도 이 시간 이하로는 내려가지 않음
    public float minimumAttackMoment = 0.6f;


    [Header("다음 공격까지 대기")]
    public float nextZombieDelay = 0.3f;


    // 현재 정답 경로
    private List<Vector2> targetPath = new List<Vector2>();

    // 화면에 표시되는 경로 점들
    private List<GameObject> routeDots = new List<GameObject>();


    private bool canSwipe = false;

    private bool currentZombieFinished = false;

    private float zombieAppearTime;


    void Awake()
    {
        Instance = this;
    }


    void Start()
    {
        zombieObject.SetActive(false);

        StartCoroutine(StartZombieSequence());
    }


    IEnumerator StartZombieSequence()
    {
        while (true)
        {
            currentZombieFinished = false;

            canSwipe = false;

            zombieObject.SetActive(false);


            // -------------------------
            // 1. 새로운 경로 생성
            // -------------------------

            GenerateRandomPath();


            // -------------------------
            // 2. 경로 화면에 표시
            // -------------------------

            ShowPath();


            // -------------------------
            // 3. 1초간 보여주기
            // -------------------------

            yield return new WaitForSeconds(previewTime);


            // -------------------------
            // 4. 경로 숨기기
            // -------------------------

            HidePath();


            // -------------------------
            // 5. 좀비 등장
            // -------------------------

            zombieObject.SetActive(true);

            zombieAppearTime = Time.time;

            canSwipe = true;


            Debug.Log(
                "좀비 등장! 공격 타이밍 = "
                + attackMoment
                + "초 / 허용범위 ±"
                + timingTolerance
            );


            // 공격 타이밍 + 허용 범위가 끝날 때까지 기다림
            float maxWaitTime =
                attackMoment + timingTolerance;


            float timer = 0f;


            while (
                timer < maxWaitTime
                && !currentZombieFinished
            )
            {
                timer += Time.deltaTime;

                yield return null;
            }


            // 플레이어가 아무것도 안 했다면 실패
            if (!currentZombieFinished)
            {
                FailZombie("시간 초과");
            }


            // 잠깐 기다리고
            yield return new WaitForSeconds(nextZombieDelay);


            // -------------------------
            // 난이도 상승
            // -------------------------

            attackMoment -= speedIncreaseAmount;

            attackMoment =
                Mathf.Max(
                    attackMoment,
                    minimumAttackMoment
                );
        }
    }


    // ========================================
    // 랜덤 경로 생성
    // ========================================

    void GenerateRandomPath()
    {
        targetPath.Clear();


        // 시작점
        Vector2 start =
            GetRandomScreenPosition();


        // 중간 지점
        Vector2 middle =
            GetRandomScreenPosition();


        // 끝점
        Vector2 end =
            GetRandomScreenPosition();


        // 너무 짧은 경로 방지
        while (
            Vector2.Distance(start, end) < 400f
        )
        {
            end = GetRandomScreenPosition();
        }


        // Quadratic Bezier Curve
        // 시작 → 중간 → 끝

        for (int i = 0; i < pathSampleCount; i++)
        {
            float t =
                i / (float)(pathSampleCount - 1);


            Vector2 point =
                CalculateBezierPoint(
                    t,
                    start,
                    middle,
                    end
                );


            targetPath.Add(point);
        }
    }


    Vector2 GetRandomScreenPosition()
    {
        float x =
            Random.Range(
                screenMargin,
                Screen.width - screenMargin
            );


        float y =
            Random.Range(
                screenMargin,
                Screen.height - screenMargin
            );


        return new Vector2(x, y);
    }


    Vector2 CalculateBezierPoint(
        float t,
        Vector2 start,
        Vector2 middle,
        Vector2 end
    )
    {
        float oneMinusT = 1f - t;


        return
            oneMinusT * oneMinusT * start
            +
            2f * oneMinusT * t * middle
            +
            t * t * end;
    }


    // ========================================
    // 경로 표시
    // ========================================

    void ShowPath()
    {
        ClearPathDots();


        foreach (Vector2 screenPoint in targetPath)
        {
            Vector2 localPoint;


            RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPoint,
                    null,
                    out localPoint
                );


            Image dot =
                Instantiate(
                    routeDotPrefab,
                    routeRoot
                );


            dot.rectTransform.anchoredPosition =
                localPoint;


            routeDots.Add(dot.gameObject);
        }
    }


    // ========================================
    // 경로 숨김
    // ========================================

    void HidePath()
    {
        ClearPathDots();
    }


    void ClearPathDots()
    {
        foreach (GameObject dot in routeDots)
        {
            if (dot != null)
            {
                Destroy(dot);
            }
        }


        routeDots.Clear();
    }


    // ========================================
    // 플레이어 Swipe 전달받기
    // ========================================

    public void CheckSwipe(
        List<Vector2> playerSwipe
    )
    {
        if (!canSwipe)
            return;


        if (currentZombieFinished)
            return;


        if (playerSwipe.Count < 2)
            return;


        float elapsed =
            Time.time - zombieAppearTime;


        // ------------------------------------
        // 1. 시간 판정
        // ------------------------------------

        float minimumTime =
            attackMoment - timingTolerance;


        float maximumTime =
            attackMoment + timingTolerance;


        bool timingSuccess =
            elapsed >= minimumTime
            &&
            elapsed <= maximumTime;


        if (!timingSuccess)
        {
            FailZombie(
                "타이밍 실패 : "
                + elapsed.ToString("F2")
                + "초"
            );

            return;
        }


        // ------------------------------------
        // 2. 경로 판정
        // ------------------------------------

        bool pathSuccess =
            CheckPath(playerSwipe);


        if (!pathSuccess)
        {
            FailZombie("경로 실패");

            return;
        }


        // 둘 다 성공
        SuccessZombie();
    }


    // ========================================
    // 경로 판정
    // ========================================

    bool CheckPath(
        List<Vector2> playerSwipe
    )
    {
        if (targetPath.Count == 0)
            return false;


        // ------------------------------------
        // 시작 위치 검사
        // ------------------------------------

        float startDistance =
            Vector2.Distance(
                playerSwipe[0],
                targetPath[0]
            );


        if (startDistance > startEndTolerance)
        {
            Debug.Log(
                "시작점 너무 멀음 : "
                + startDistance
            );

            return false;
        }


        // ------------------------------------
        // 끝 위치 검사
        // ------------------------------------

        float endDistance =
            Vector2.Distance(
                playerSwipe[playerSwipe.Count - 1],
                targetPath[targetPath.Count - 1]
            );


        if (endDistance > startEndTolerance)
        {
            Debug.Log(
                "끝점 너무 멀음 : "
                + endDistance
            );

            return false;
        }


        // ------------------------------------
        // 전체 경로 검사
        // ------------------------------------

        int successfulPoints = 0;


        foreach (Vector2 targetPoint in targetPath)
        {
            float distance =
                DistanceToSwipe(
                    targetPoint,
                    playerSwipe
                );


            if (distance <= pathTolerance)
            {
                successfulPoints++;
            }
        }


        float successRatio =
            successfulPoints
            /
            (float)targetPath.Count;


        Debug.Log(
            "경로 일치율 : "
            + (successRatio * 100f).ToString("F1")
            + "%"
        );


        // 전체 경로 중 75% 이상
        // 허용 범위 안에 들어오면 성공

        return successRatio >= 0.75f;
    }


    // ========================================
    // 경로와 플레이어 선 사이 거리
    // ========================================

    float DistanceToSwipe(
        Vector2 point,
        List<Vector2> swipe
    )
    {
        float minimumDistance =
            float.MaxValue;


        for (
            int i = 0;
            i < swipe.Count - 1;
            i++
        )
        {
            float distance =
                DistancePointToLineSegment(
                    point,
                    swipe[i],
                    swipe[i + 1]
                );


            if (distance < minimumDistance)
            {
                minimumDistance = distance;
            }
        }


        return minimumDistance;
    }


    float DistancePointToLineSegment(
        Vector2 point,
        Vector2 start,
        Vector2 end
    )
    {
        Vector2 line = end - start;


        float lengthSquared =
            line.sqrMagnitude;


        if (lengthSquared == 0f)
        {
            return Vector2.Distance(
                point,
                start
            );
        }


        float t =
            Vector2.Dot(
                point - start,
                line
            )
            /
            lengthSquared;


        t = Mathf.Clamp01(t);


        Vector2 projection =
            start + t * line;


        return Vector2.Distance(
            point,
            projection
        );
    }


    // ========================================
    // 성공
    // ========================================

    void SuccessZombie()
    {
        currentZombieFinished = true;

        canSwipe = false;


        Debug.Log(
            "<color=green>좀비 공격 방어 성공!</color>"
        );


        zombieObject.SetActive(false);


        // 나중에
        // 점수 증가
        // 좀비 사라지는 애니메이션
        // 이펙트
    }


    // ========================================
    // 실패
    // ========================================

    void FailZombie(string reason)
    {
        if (currentZombieFinished)
            return;


        currentZombieFinished = true;

        canSwipe = false;


        Debug.Log(
            "<color=red>공격 실패 : "
            + reason
            + "</color>"
        );


        zombieObject.SetActive(false);


        // 나중에
        // HeartManager.Instance.TakeDamage();
    }
}