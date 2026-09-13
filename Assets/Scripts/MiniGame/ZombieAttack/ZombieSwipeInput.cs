using System.Collections.Generic;
using UnityEngine;

public class ZombieSwipeInput : MonoBehaviour
{
    [Header("입력 설정")]

    // 너무 짧은 움직임은 기록하지 않음
    public float pointRecordDistance = 10f;

    // 너무 짧은 swipe 방지
    public float minimumSwipeLength = 150f;


    private List<Vector2> swipePoints =
        new List<Vector2>();


    private bool isDragging = false;


    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE

        HandleMouse();

#else

        HandleTouch();

#endif
    }


    // ========================================
    // PC 테스트
    // ========================================

    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartSwipe(
                Input.mousePosition
            );
        }


        if (
            Input.GetMouseButton(0)
            &&
            isDragging
        )
        {
            ContinueSwipe(
                Input.mousePosition
            );
        }


        if (
            Input.GetMouseButtonUp(0)
            &&
            isDragging
        )
        {
            EndSwipe(
                Input.mousePosition
            );
        }
    }


    // ========================================
    // 모바일
    // ========================================

    void HandleTouch()
    {
        if (Input.touchCount == 0)
            return;


        Touch touch =
            Input.GetTouch(0);


        switch (touch.phase)
        {
            case TouchPhase.Began:

                StartSwipe(
                    touch.position
                );

                break;


            case TouchPhase.Moved:

                ContinueSwipe(
                    touch.position
                );

                break;


            case TouchPhase.Ended:

                EndSwipe(
                    touch.position
                );

                break;
        }
    }


    // ========================================
    // Swipe 시작
    // ========================================

    void StartSwipe(Vector2 position)
    {
        swipePoints.Clear();

        swipePoints.Add(position);

        isDragging = true;
    }


    // ========================================
    // Swipe 진행
    // ========================================

    void ContinueSwipe(Vector2 position)
    {
        if (swipePoints.Count == 0)
            return;


        Vector2 previous =
            swipePoints[swipePoints.Count - 1];


        if (
            Vector2.Distance(
                previous,
                position
            )
            >= pointRecordDistance
        )
        {
            swipePoints.Add(position);
        }
    }


    // ========================================
    // Swipe 종료
    // ========================================

    void EndSwipe(Vector2 position)
    {
        swipePoints.Add(position);

        isDragging = false;


        float length =
            CalculateSwipeLength();


        if (length < minimumSwipeLength)
        {
            Debug.Log(
                "Swipe가 너무 짧음"
            );

            swipePoints.Clear();

            return;
        }


        ZombieSliceGameManager.Instance
            .CheckSwipe(
                new List<Vector2>(
                    swipePoints
                )
            );


        swipePoints.Clear();
    }


    // ========================================
    // 전체 Swipe 길이
    // ========================================

    float CalculateSwipeLength()
    {
        float total = 0f;


        for (
            int i = 0;
            i < swipePoints.Count - 1;
            i++
        )
        {
            total +=
                Vector2.Distance(
                    swipePoints[i],
                    swipePoints[i + 1]
                );
        }


        return total;
    }
}